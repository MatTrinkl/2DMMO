using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mmo.Generators;

/// <summary>
/// Source generator for Delta DTOs.
/// Generates delta transfer objects grouped by DirtyFlags for entities marked with [GenerateDirtyTracking].
/// </summary>
[Generator]
public class DeltaDtoGenerator : IIncrementalGenerator
{
    private const string GenerateDirtyTrackingAttributeFullName = "Mmo.Shared.Generators.GenerateDirtyTrackingAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all types with [GenerateDirtyTracking]
        IncrementalValuesProvider<TypeDeclarationSyntax?> typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => GeneratorHelpers.IsCandidateForGeneration(s),
                static (ctx, _) => GeneratorHelpers.GetSemanticTargetForGeneration(ctx, GenerateDirtyTrackingAttributeFullName))
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
                var config = GeneratorHelpers.ExtractConfiguration(typeSymbol);
                
                // Collect all tracked properties (including from base interfaces)
                var allTrackedProperties = new List<GeneratorHelpers.TrackedPropertyInfo>();
                
                // Get all properties from the type and its base interfaces
                var allMembers = new List<ISymbol>();
                CollectAllMembers(typeSymbol, allMembers);
                
                foreach (var member in allMembers)
                {
                    if (member is not IPropertySymbol property)
                        continue;
                        
                    var attr = property.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Mmo.Shared.DirtyTracking.Attributes.TrackDirtyAttribute");
                        
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
                
                // Generate unified Delta DTO with all tracked properties
                if (allTrackedProperties.Count > 0)
                {
                    string deltaDto = GenerateUnifiedDeltaDto(typeSymbol, config, allTrackedProperties);
                    string deltaName = $"{typeSymbol.Name}Delta";
                    context.AddSource($"{deltaName}.g.cs", 
                        SourceText.From(deltaDto, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DELTADTO001",
                        "Delta DTO Generation Error",
                        $"Error generating Delta DTOs for {typeSymbol.Name}: {ex.Message}",
                        "DeltaDtoGenerator",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }
    }

    private static string GenerateUnifiedDeltaDto(INamedTypeSymbol typeSymbol, GeneratorHelpers.DirtyTrackingConfig config, 
        List<GeneratorHelpers.TrackedPropertyInfo> properties)
    {
        var sb = new StringBuilder();
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        string deltaName = $"{typeSymbol.Name}Delta";
        
        // Check if this Delta DTO should implement a union interface
        string? unionInterface = null;
        var unionMemberAttr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Mmo.Shared.Generators.DeltaDtoUnionMemberAttribute");
        
        if (unionMemberAttr != null && unionMemberAttr.ConstructorArguments.Length >= 2)
        {
            var unionTypeArg = unionMemberAttr.ConstructorArguments[1];
            if (unionTypeArg.Value is INamedTypeSymbol unionRootType)
            {
                // Find the GenerateDeltaDtoUnion attribute on the union root
                var unionAttr = unionRootType.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Mmo.Shared.Generators.GenerateDeltaDtoUnionAttribute");
                
                if (unionAttr != null)
                {
                    string? unionName = null;
                    string? unionNs = null;
                    
                    foreach (var namedArg in unionAttr.NamedArguments)
                    {
                        if (namedArg.Key == "UnionName" && namedArg.Value.Value is string name)
                            unionName = name;
                        else if (namedArg.Key == "Namespace" && namedArg.Value.Value is string ns)
                            unionNs = ns;
                    }
                    
                    if (string.IsNullOrEmpty(unionName))
                    {
                        string typeName = unionRootType.Name.StartsWith("I") ? unionRootType.Name.Substring(1) : unionRootType.Name;
                        unionName = $"{typeName}DeltaUnion";
                    }
                    
                    if (string.IsNullOrEmpty(unionNs))
                    {
                        unionNs = $"{unionRootType.ContainingNamespace.ToDisplayString()}.Dtos";
                    }
                    
                    unionInterface = $"{unionNs}.{unionName}";
                }
            }
        }
        
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using MessagePack;");
        sb.AppendLine("using Mmo.Shared.Generators;");
        sb.AppendLine("using Mmo.Shared.Zones.Interfaces;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}.Dtos;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Unified Delta DTO for {typeSymbol.Name} containing all tracked properties as nullable.");
        sb.AppendLine($"/// Null values indicate no change for that property.");
        sb.AppendLine($"/// Auto-generated by DeltaDtoGenerator.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine("[MessagePackObject]");
        
        string idType = config.IdPropertyType ?? "System.Guid";
        string baseTypes = $"IDeltaDto<{idType}>";
        if (!string.IsNullOrEmpty(unionInterface))
        {
            baseTypes += $", {unionInterface}";
        }
        
        sb.AppendLine($"public class {deltaName} : {baseTypes}");
        sb.AppendLine("{");
        
        // Add ID property
        int keyIndex = 0;
        if (!string.IsNullOrEmpty(config.IdPropertyName))
        {
            sb.AppendLine($"    [Key({keyIndex})]");
            sb.AppendLine($"    [DeltaId]");
            sb.AppendLine($"    public {idType} {config.IdPropertyName} {{ get; set; }}");
            sb.AppendLine();
            keyIndex++;
        }
        
        // Add all tracked properties (nullable for delta pattern)
        foreach (var prop in properties)
        {
            string nullableType = GeneratorHelpers.MakeNullable(prop.Type);
            sb.AppendLine($"    [Key({keyIndex})]");
            sb.AppendLine($"    public {nullableType} {prop.Name} {{ get; set; }}");
            sb.AppendLine();
            keyIndex++;
        }
        
        // Add GetId() method
        if (!string.IsNullOrEmpty(config.IdPropertyName))
        {
            sb.AppendLine($"    public {idType} GetId() => {config.IdPropertyName};");
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }
    
    private static void CollectAllMembers(INamedTypeSymbol typeSymbol, List<ISymbol> members)
    {
        // Add members from this type
        foreach (var member in typeSymbol.GetMembers())
        {
            members.Add(member);
        }
        
        // Recursively collect from base interfaces
        foreach (var baseInterface in typeSymbol.Interfaces)
        {
            CollectAllMembers(baseInterface, members);
        }
    }
}
