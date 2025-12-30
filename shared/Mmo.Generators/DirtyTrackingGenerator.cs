using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mmo.Generators;

/// <summary>
/// Source generator for automatic dirty-tracking system.
/// Generates Delta DTOs, IDirtyTrackable implementations, and extension methods.
/// </summary>
[Generator]
public class DirtyTrackingGenerator : IIncrementalGenerator
{
    private const string GenerateDirtyTrackingAttributeFullName = "Mmo.Shared.Generators.GenerateDirtyTrackingAttribute";
    private const string TrackedPropertyAttributeFullName = "Mmo.Shared.Generators.TrackedPropertyAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all types with [GenerateDirtyTracking]
        IncrementalValuesProvider<TypeDeclarationSyntax?> typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => IsCandidateForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        IncrementalValueProvider<(Compilation Left, ImmutableArray<TypeDeclarationSyntax?> Right)> compilationAndTypes =
            context.CompilationProvider.Combine(typeDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndTypes,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateForGeneration(SyntaxNode node)
    {
        return node is TypeDeclarationSyntax typeDeclaration &&
               typeDeclaration.AttributeLists.Count > 0;
    }

    private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;

        foreach (AttributeListSyntax attributeList in typeDeclaration.AttributeLists)
        foreach (AttributeSyntax attribute in attributeList.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            INamedTypeSymbol? attributeContainingType = attributeSymbol.ContainingType;
            string fullName = attributeContainingType.ToDisplayString();

            if (fullName == GenerateDirtyTrackingAttributeFullName)
                return typeDeclaration;
        }

        return null;
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
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);

            if (typeSymbol == null)
                continue;

            try
            {
                var config = ExtractConfiguration(typeSymbol);
                
                // Generate IDirtyTrackable implementation
                string trackableImpl = GenerateIDirtyTrackableImplementation(typeSymbol, config);
                context.AddSource($"{typeSymbol.Name}.DirtyTracking.g.cs", 
                    SourceText.From(trackableImpl, Encoding.UTF8));
                
                // Generate Delta DTOs
                var deltaGroups = GroupPropertiesByDirtyFlags(typeSymbol);
                foreach (var group in deltaGroups)
                {
                    string deltaDto = GenerateDeltaDto(typeSymbol, config, group.Key, group.Value);
                    string deltaName = GetDeltaDtoName(typeSymbol.Name, group.Key);
                    context.AddSource($"{deltaName}.g.cs", 
                        SourceText.From(deltaDto, Encoding.UTF8));
                }
                
                // Generate Extension Methods
                string extensions = GenerateExtensionMethods(typeSymbol, config, deltaGroups);
                context.AddSource($"{typeSymbol.Name}.DeltaExtensions.g.cs",
                    SourceText.From(extensions, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DIRTYTRACK001",
                        "Dirty Tracking Generation Error",
                        $"Error generating dirty tracking for {typeSymbol.Name}: {ex.Message}",
                        "DirtyTrackingGenerator",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }
    }

    private static DirtyTrackingConfig ExtractConfiguration(INamedTypeSymbol typeSymbol)
    {
        var config = new DirtyTrackingConfig();
        
        var attr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == GenerateDirtyTrackingAttributeFullName);
            
        if (attr != null)
        {
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "IdPropertyName" && namedArg.Value.Value is string idPropName)
                {
                    config.IdPropertyName = idPropName;
                }
            }
        }
        
        // Find the ID property
        if (!string.IsNullOrEmpty(config.IdPropertyName))
        {
            var idProp = typeSymbol.GetMembers(config.IdPropertyName).OfType<IPropertySymbol>().FirstOrDefault();
            if (idProp != null)
            {
                config.IdPropertyType = idProp.Type.ToDisplayString();
            }
        }
        
        return config;
    }

    private static Dictionary<string, List<TrackedPropertyInfo>> GroupPropertiesByDirtyFlags(INamedTypeSymbol typeSymbol)
    {
        var groups = new Dictionary<string, List<TrackedPropertyInfo>>();
        
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;
                
            var attr = property.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == TrackedPropertyAttributeFullName);
                
            if (attr == null || attr.ConstructorArguments.Length == 0)
                continue;
                
            var flagValue = attr.ConstructorArguments[0];
            string flagName = GetDirtyFlagGroupName(flagValue);
            
            if (!groups.ContainsKey(flagName))
                groups[flagName] = new List<TrackedPropertyInfo>();
                
            groups[flagName].Add(new TrackedPropertyInfo
            {
                Name = property.Name,
                Type = property.Type.ToDisplayString(),
                FlagValue = flagValue.Value?.ToString() ?? "0"
            });
        }
        
        return groups;
    }

    private static string GetDirtyFlagGroupName(TypedConstant flagValue)
    {
        // Map DirtyFlags values to group names
        if (flagValue.Value == null) return "Unknown";
        
        uint value = Convert.ToUInt32(flagValue.Value);
        
        // Check for common flag combinations
        if ((value & 0x07) != 0) return "Position"; // Position, Velocity, Rotation
        if ((value & 0x01F8) != 0) return "State";  // Health, MaxHealth, Resource, MaxResource, State, Model, Level
        
        return "Custom";
    }

    private static string GetDeltaDtoName(string typeName, string groupName)
    {
        return $"{typeName}{groupName}Delta";
    }

    private static string GenerateIDirtyTrackableImplementation(INamedTypeSymbol typeSymbol, DirtyTrackingConfig config)
    {
        var sb = new StringBuilder();
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine($"partial class {typeSymbol.Name} : Mmo.Shared.Entities.Interfaces.IDirtyTrackable");
        sb.AppendLine("{");
        sb.AppendLine("    private Mmo.Shared.Entities.Enums.DirtyFlags _dirtyFlags = Mmo.Shared.Entities.Enums.DirtyFlags.None;");
        sb.AppendLine();
        sb.AppendLine("    public Mmo.Shared.Entities.Enums.DirtyFlags DirtyFlags => _dirtyFlags;");
        sb.AppendLine("    public bool IsDirty => _dirtyFlags != Mmo.Shared.Entities.Enums.DirtyFlags.None;");
        sb.AppendLine();
        sb.AppendLine("    public void ClearDirtyFlags() => _dirtyFlags = Mmo.Shared.Entities.Enums.DirtyFlags.None;");
        sb.AppendLine("    public void MarkDirty(Mmo.Shared.Entities.Enums.DirtyFlags flags) => _dirtyFlags |= flags;");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private static string GenerateDeltaDto(INamedTypeSymbol typeSymbol, DirtyTrackingConfig config, 
        string groupName, List<TrackedPropertyInfo> properties)
    {
        var sb = new StringBuilder();
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        string deltaName = GetDeltaDtoName(typeSymbol.Name, groupName);
        
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
        sb.AppendLine($"/// Delta DTO for {groupName}-related changes in {typeSymbol.Name}.");
        sb.AppendLine($"/// Auto-generated by DirtyTrackingGenerator.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine("[MessagePackObject]");
        
        string idType = config.IdPropertyType ?? "System.Guid";
        sb.AppendLine($"public class {deltaName} : IDeltaDto<{idType}>");
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
        
        // Add tracked properties (nullable for delta pattern)
        foreach (var prop in properties)
        {
            string nullableType = MakeNullable(prop.Type);
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

    private static string GenerateExtensionMethods(INamedTypeSymbol typeSymbol, DirtyTrackingConfig config,
        Dictionary<string, List<TrackedPropertyInfo>> deltaGroups)
    {
        var sb = new StringBuilder();
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}.Extensions;");
        sb.AppendLine();
        sb.AppendLine($"public static class {typeSymbol.Name}DeltaExtensions");
        sb.AppendLine("{");
        
        foreach (var group in deltaGroups)
        {
            string deltaName = GetDeltaDtoName(typeSymbol.Name, group.Key);
            string methodName = $"To{group.Key}Delta";
            
            sb.AppendLine($"    public static {namespaceName}.Dtos.{deltaName} {methodName}(this {typeSymbol.ToDisplayString()} entity)");
            sb.AppendLine("    {");
            sb.AppendLine($"        return new {namespaceName}.Dtos.{deltaName}");
            sb.AppendLine("        {");
            
            if (!string.IsNullOrEmpty(config.IdPropertyName))
            {
                sb.AppendLine($"            {config.IdPropertyName} = entity.{config.IdPropertyName},");
            }
            
            foreach (var prop in group.Value)
            {
                sb.AppendLine($"            {prop.Name} = entity.{prop.Name},");
            }
            
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private static string MakeNullable(string typeName)
    {
        // Check if already nullable or reference type
        if (typeName.EndsWith("?") || typeName == "string")
            return typeName;
            
        // Value types need ?
        if (typeName == "int" || typeName == "float" || typeName == "double" || 
            typeName == "long" || typeName == "byte" || typeName == "uint" ||
            typeName == "bool" || typeName == "System.Guid" || typeName.StartsWith("System."))
        {
            return $"{typeName}?";
        }
        
        return typeName;
    }

    private class DirtyTrackingConfig
    {
        public string? IdPropertyName { get; set; }
        public string? IdPropertyType { get; set; }
    }

    private class TrackedPropertyInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string FlagValue { get; set; } = "";
    }
}
