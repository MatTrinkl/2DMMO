using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mmo.Generators;

/// <summary>
///     Source generator for Delta extension methods.
///     Generates ToXXXDelta() extension methods for entities marked with [GenerateDirtyTracking].
/// </summary>
[Generator]
public class DeltaExtensionsGenerator : IIncrementalGenerator
{
    private const string GenerateDirtyTrackingAttributeFullName =
        "Mmo.Shared.Generators.GenerateDirtyTrackingAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all types with [GenerateDirtyTracking]
        IncrementalValuesProvider<TypeDeclarationSyntax?> typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => GeneratorHelpers.IsCandidateForGeneration(s),
                static (ctx, _) =>
                    GeneratorHelpers.GetSemanticTargetForGeneration(ctx, GenerateDirtyTrackingAttributeFullName))
            .Where(static m => m is not null);

        IncrementalValueProvider<(Compilation Left, ImmutableArray<TypeDeclarationSyntax?> Right)> compilationAndTypes =
            context.CompilationProvider.Combine(typeDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndTypes,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax?> types,
        SourceProductionContext context)
    {
        if (types.IsDefaultOrEmpty)
            return;

        var distinctTypes = types.Where(c => c != null).Distinct().ToList();

        foreach (TypeDeclarationSyntax? typeDeclaration in distinctTypes)
        {
            if (typeDeclaration == null)
                continue;

            SemanticModel semanticModel = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;

            if (typeSymbol == null)
                continue;

            try
            {
                GeneratorHelpers.DirtyTrackingConfig config = GeneratorHelpers.ExtractConfiguration(typeSymbol);

                // Collect all tracked properties (including from base interfaces)
                var allTrackedProperties = new List<GeneratorHelpers.TrackedPropertyInfo>();

                // Get all properties from the type and its base interfaces
                var allMembers = new List<ISymbol>();
                CollectAllMembers(typeSymbol, allMembers);

                foreach (ISymbol? member in allMembers)
                {
                    if (member is not IPropertySymbol property)
                        continue;

                    AttributeData? attr = property.GetAttributes()
                        .FirstOrDefault(a =>
                            a.AttributeClass?.ToDisplayString() ==
                            "Mmo.Shared.DirtyTracking.Attributes.TrackDirtyAttribute");

                    if (attr == null || attr.ConstructorArguments.Length == 0)
                        continue;

                    // Avoid duplicates
                    if (allTrackedProperties.Any(p => p.Name == property.Name))
                        continue;

                    allTrackedProperties.Add(new GeneratorHelpers.TrackedPropertyInfo
                    {
                        Name = property.Name,
                        Type = property.Type.ToDisplayString(),
                        FlagValue = attr.ConstructorArguments[0].Value?.ToString() ?? "0"
                    });
                }

                if (allTrackedProperties.Count > 0)
                {
                    string extensions = GenerateUnifiedExtensionMethod(typeSymbol, config, allTrackedProperties);
                    context.AddSource($"{typeSymbol.Name}.DeltaExtensions.g.cs",
                        SourceText.From(extensions, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DELTAEXT001",
                        "Delta Extensions Generation Error",
                        $"Error generating extension methods for {typeSymbol.Name}: {ex.Message}",
                        "DeltaExtensionsGenerator",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }
    }

    private static string GenerateUnifiedExtensionMethod(INamedTypeSymbol typeSymbol,
        GeneratorHelpers.DirtyTrackingConfig config,
        List<GeneratorHelpers.TrackedPropertyInfo> properties)
    {
        var sb = new StringBuilder();
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        string deltaName = $"{typeSymbol.Name}Delta";

        // Check if this is part of a delta union
        bool isUnionMember = typeSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == "Mmo.Shared.Generators.DeltaDtoUnionMemberAttribute");

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}.Extensions;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Extension methods for converting {typeSymbol.Name} to unified Delta DTO.");
        sb.AppendLine("/// Auto-generated by DeltaExtensionsGenerator.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static class {typeSymbol.Name}DeltaExtensions");
        sb.AppendLine("{");

        // Generate ToDelta() method
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Converts {typeSymbol.Name} to its unified Delta DTO.");
        sb.AppendLine("    /// All tracked properties are included.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(
            $"    public static {namespaceName}.Dtos.{deltaName} ToDelta(this {typeSymbol.ToDisplayString()} entity)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return new {namespaceName}.Dtos.{deltaName}");
        sb.AppendLine("        {");

        if (!string.IsNullOrEmpty(config.IdPropertyName))
            sb.AppendLine($"            {config.IdPropertyName} = entity.{config.IdPropertyName},");

        foreach (GeneratorHelpers.TrackedPropertyInfo? prop in properties)
            sb.AppendLine($"            {prop.Name} = entity.{prop.Name},");

        sb.AppendLine("        };");
        sb.AppendLine("    }");

        // If this is a union member, generate a method that returns the union type
        if (isUnionMember)
        {
            AttributeData? unionMemberAttr = typeSymbol.GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass?.ToDisplayString() == "Mmo.Shared.Generators.DeltaDtoUnionMemberAttribute");

            if (unionMemberAttr != null && unionMemberAttr.ConstructorArguments.Length >= 2)
            {
                TypedConstant unionTypeArg = unionMemberAttr.ConstructorArguments[1];
                if (unionTypeArg.Value is INamedTypeSymbol unionRootType)
                {
                    AttributeData? unionAttr = unionRootType.GetAttributes()
                        .FirstOrDefault(a =>
                            a.AttributeClass?.ToDisplayString() ==
                            "Mmo.Shared.Generators.GenerateDeltaDtoUnionAttribute");

                    if (unionAttr != null)
                    {
                        string? unionName = null;
                        string? unionNs = null;

                        foreach (KeyValuePair<string, TypedConstant> namedArg in unionAttr.NamedArguments)
                            if (namedArg.Key == "UnionName" && namedArg.Value.Value is string name)
                                unionName = name;
                            else if (namedArg.Key == "Namespace" && namedArg.Value.Value is string ns)
                                unionNs = ns;

                        if (string.IsNullOrEmpty(unionName))
                        {
                            string typeName = unionRootType.Name.StartsWith("I")
                                ? unionRootType.Name.Substring(1)
                                : unionRootType.Name;
                            unionName = $"{typeName}DeltaUnion";
                        }

                        if (string.IsNullOrEmpty(unionNs))
                            unionNs = $"{unionRootType.ContainingNamespace.ToDisplayString()}.Dtos";

                        sb.AppendLine();
                        sb.AppendLine("    /// <summary>");
                        sb.AppendLine($"    /// Converts {typeSymbol.Name} to its unified Delta DTO as a union type.");
                        sb.AppendLine(
                            "    /// This allows different entity types to be stored in the same collection.");
                        sb.AppendLine("    /// </summary>");
                        sb.AppendLine(
                            $"    public static {unionNs}.{unionName} ToDeltaUnion(this {typeSymbol.ToDisplayString()} entity)");
                        sb.AppendLine("    {");
                        sb.AppendLine("        return entity.ToDelta();");
                        sb.AppendLine("    }");
                    }
                }
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void CollectAllMembers(INamedTypeSymbol typeSymbol, List<ISymbol> members)
    {
        // Add members from this type
        foreach (ISymbol? member in typeSymbol.GetMembers()) members.Add(member);

        // Recursively collect from base interfaces
        foreach (INamedTypeSymbol? baseInterface in typeSymbol.Interfaces) CollectAllMembers(baseInterface, members);
    }
}
