using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mmo.Generators;

/// <summary>
///     Shared helper methods for all dirty-tracking generators.
/// </summary>
public static class GeneratorHelpers
{
    private const string TrackDirtyAttributeFullName = "Mmo.Shared.DirtyTracking.Attributes.TrackDirtyAttribute";

    public static bool IsCandidateForGeneration(SyntaxNode node)
    {
        return node is TypeDeclarationSyntax typeDeclaration &&
               typeDeclaration.AttributeLists.Count > 0;
    }

    public static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context,
        string attributeFullName)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;

        foreach (AttributeListSyntax attributeList in typeDeclaration.AttributeLists)
        foreach (AttributeSyntax attribute in attributeList.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            INamedTypeSymbol? attributeContainingType = attributeSymbol.ContainingType;
            string fullName = attributeContainingType.ToDisplayString();

            if (fullName == attributeFullName)
                return typeDeclaration;
        }

        return null;
    }

    public static DirtyTrackingConfig ExtractConfiguration(INamedTypeSymbol typeSymbol)
    {
        var config = new DirtyTrackingConfig();

        AttributeData? attr = typeSymbol.GetAttributes()
            .FirstOrDefault(a =>
                a.AttributeClass?.ToDisplayString() == "Mmo.Shared.Generators.GenerateDirtyTrackingAttribute");

        if (attr != null)
            foreach (KeyValuePair<string, TypedConstant> namedArg in attr.NamedArguments)
                if (namedArg.Key == "IdPropertyName" && namedArg.Value.Value is string idPropName)
                    config.IdPropertyName = idPropName;

        // Find the ID property
        if (!string.IsNullOrEmpty(config.IdPropertyName))
        {
            IPropertySymbol? idProp = typeSymbol.GetMembers(config.IdPropertyName!).OfType<IPropertySymbol>()
                .FirstOrDefault();
            if (idProp != null) config.IdPropertyType = idProp.Type.ToDisplayString();
        }

        return config;
    }

    public static Dictionary<string, List<TrackedPropertyInfo>> GroupPropertiesByDirtyFlags(INamedTypeSymbol typeSymbol)
    {
        var groups = new Dictionary<string, List<TrackedPropertyInfo>>();

        foreach (ISymbol? member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            AttributeData? attr = property.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == TrackDirtyAttributeFullName);

            if (attr == null || attr.ConstructorArguments.Length == 0)
                continue;

            // TrackDirty uses string flag names
            TypedConstant flagNameArg = attr.ConstructorArguments[0];
            string flagName = flagNameArg.Value?.ToString() ?? "Unknown";

            if (!groups.ContainsKey(flagName))
                groups[flagName] = new List<TrackedPropertyInfo>();

            groups[flagName].Add(new TrackedPropertyInfo
            {
                Name = property.Name,
                Type = property.Type.ToDisplayString(),
                FlagValue = flagName
            });
        }

        return groups;
    }

    public static string GetDirtyFlagGroupName(TypedConstant flagValue)
    {
        // Map DirtyFlags values to group names
        if (flagValue.Value == null) return "Unknown";

        uint value = Convert.ToUInt32(flagValue.Value);

        // Check for common flag combinations
        if ((value & 0x07) != 0) return "Position"; // Position, Velocity, Rotation
        if ((value & 0x01F8) != 0) return "State"; // Health, MaxHealth, Resource, MaxResource, State, Model, Level

        return "Custom";
    }

    public static string GetDeltaDtoName(string typeName, string groupName) => $"{typeName}{groupName}Delta";

    public static string MakeNullable(string typeName)
    {
        // Check if already nullable
        if (typeName.EndsWith("?"))
            return typeName;

        // String and other reference types that could be null
        if (typeName == "string")
            return typeName;

        // All value types need ?
        // This includes: primitives, System types, and custom structs/records
        return $"{typeName}?";
    }

    public class DirtyTrackingConfig
    {
        public string? IdPropertyName { get; set; }
        public string? IdPropertyType { get; set; }
    }

    public class TrackedPropertyInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string FlagValue { get; set; } = "";
    }
}
