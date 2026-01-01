using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mmo.Generators;

/// <summary>
///     Source generator for Delta DTO Unions.
///     Generates union interfaces for polymorphic Delta DTO serialization.
/// </summary>
[Generator]
public class DeltaDtoUnionGenerator : IIncrementalGenerator
{
    private const string GenerateDeltaDtoUnionAttributeFullName =
        "Mmo.Shared.Generators.GenerateDeltaDtoUnionAttribute";

    private const string DeltaDtoUnionMemberAttributeFullName = "Mmo.Shared.Generators.DeltaDtoUnionMemberAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all types with [GenerateDeltaDtoUnion]
        IncrementalValuesProvider<TypeDeclarationSyntax?> typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => GeneratorHelpers.IsCandidateForGeneration(s),
                static (ctx, _) =>
                    GeneratorHelpers.GetSemanticTargetForGeneration(ctx, GenerateDeltaDtoUnionAttributeFullName))
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
                AttributeData? attribute = typeSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == GenerateDeltaDtoUnionAttributeFullName);

                if (attribute == null)
                    continue;

                // Extract configuration from attribute
                string? unionName = null;
                string? unionNamespace = null;

                foreach (KeyValuePair<string, TypedConstant> namedArg in attribute.NamedArguments)
                    if (namedArg.Key == "UnionName" && namedArg.Value.Value is string name)
                        unionName = name;
                    else if (namedArg.Key == "Namespace" && namedArg.Value.Value is string ns)
                        unionNamespace = ns;

                // Default values
                if (string.IsNullOrEmpty(unionName))
                {
                    string typeName = typeSymbol.Name.StartsWith("I") ? typeSymbol.Name.Substring(1) : typeSymbol.Name;
                    unionName = $"{typeName}DeltaUnion";
                }

                if (string.IsNullOrEmpty(unionNamespace))
                    unionNamespace = $"{typeSymbol.ContainingNamespace.ToDisplayString()}.Dtos";

                // Find all types that are members of this union
                List<(int Index, string DeltaTypeName)> unionMembers = FindUnionMembers(compilation, typeSymbol);

                if (unionMembers.Count > 0)
                {
                    string unionInterface = GenerateUnionInterface(unionName!, unionNamespace!, unionMembers);
                    context.AddSource($"{unionName}.g.cs",
                        SourceText.From(unionInterface, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DELTAUNION001",
                        "Delta DTO Union Generation Error",
                        $"Error generating Delta DTO Union for {typeSymbol.Name}: {ex.Message}",
                        "DeltaDtoUnionGenerator",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }
    }

    private static List<(int Index, string DeltaTypeName)> FindUnionMembers(Compilation compilation,
        INamedTypeSymbol unionRoot)
    {
        var members = new List<(int Index, string DeltaTypeName)>();

        // Find all types with [DeltaDtoUnionMember] that reference this union
        foreach (SyntaxTree? syntaxTree in compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            SyntaxNode root = syntaxTree.GetRoot();

            foreach (TypeDeclarationSyntax? typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
                if (typeSymbol == null)
                    continue;

                AttributeData? memberAttribute = typeSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == DeltaDtoUnionMemberAttributeFullName);

                if (memberAttribute == null || memberAttribute.ConstructorArguments.Length < 2)
                    continue;

                // Check if this member belongs to our union
                TypedConstant unionTypeArg = memberAttribute.ConstructorArguments[1];
                if (unionTypeArg.Value is INamedTypeSymbol referencedUnion &&
                    SymbolEqualityComparer.Default.Equals(referencedUnion, unionRoot))
                {
                    int index = (int)(memberAttribute.ConstructorArguments[0].Value ?? 0);
                    string deltaTypeName = $"{typeSymbol.Name}Delta";
                    string fullTypeName = $"{typeSymbol.ContainingNamespace.ToDisplayString()}.Dtos.{deltaTypeName}";
                    members.Add((index, fullTypeName));
                }
            }
        }

        return members.OrderBy(m => m.Index).ToList();
    }

    private static string GenerateUnionInterface(string unionName, string unionNamespace,
        List<(int Index, string DeltaTypeName)> members)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using MessagePack;");
        sb.AppendLine();
        sb.AppendLine($"namespace {unionNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Union interface for polymorphic Delta DTO serialization.");
        sb.AppendLine("/// Auto-generated by DeltaDtoUnionGenerator.");
        sb.AppendLine("/// </summary>");

        foreach ((int Index, string DeltaTypeName) member in members)
            sb.AppendLine($"[Union({member.Index}, typeof({member.DeltaTypeName}))]");

        sb.AppendLine($"public interface {unionName}");
        sb.AppendLine("{");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
