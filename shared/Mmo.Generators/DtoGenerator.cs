using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mmo.Generators;

[Generator]
public class DtoGenerator : IIncrementalGenerator
{
    private const string GenerateDtoAttributeFullName = "Mmo.Shared.Generators.GenerateDtoAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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

            if (fullName == GenerateDtoAttributeFullName) return typeDeclaration;
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax?> types,
        SourceProductionContext context)
    {
        if (types.IsDefaultOrEmpty)
            return;

        var distinctTypes = types.Where(c => c != null).Distinct().ToList();
        var unionRoots = new Dictionary<string, UnionInfo>();
        var unionMembers = new List<UnionMemberInfo>();

        // Erste Runde: DTOs generieren und Union-Infos sammeln
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
                string sourceCode;
                UnionMemberInfo? memberInfo = null;

                if (typeSymbol.TypeKind == TypeKind.Interface)
                    sourceCode = GenerateDtoFromInterface(typeSymbol, out memberInfo);
                else
                    sourceCode = GenerateDtoFromClassOrStruct(typeSymbol);

                string dtoName = GetDtoName(typeSymbol);
                string fileName = $"{dtoName}.g.cs";
                context.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));

                // Union-Member sammeln
                if (memberInfo != null) unionMembers.Add(memberInfo);

                // Union-Root erkennen
                GenerateDtoUnionAttributeData? unionAttr = GetGenerateDtoUnionAttribute(typeSymbol);
                if (unionAttr != null)
                {
                    string rootKey = typeSymbol.ToDisplayString();
                    if (!unionRoots.ContainsKey(rootKey))
                        unionRoots[rootKey] = new UnionInfo
                        {
                            RootInterface = rootKey,
                            RootInterfaceName = typeSymbol.Name,
                            UnionName = unionAttr.UnionName ?? $"{GetBaseName(typeSymbol)}DtoUnion",
                            Namespace = unionAttr.Namespace ??
                                        $"{typeSymbol.ContainingNamespace.ToDisplayString()}.Dtos"
                        };
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DTOGEN001",
                        "DTO Generation Error",
                        $"Error generating DTO for {typeSymbol.Name}: {ex.Message}",
                        "DtoGenerator",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }

        // Zweite Runde: Union-Interfaces und Extensions generieren
        foreach (KeyValuePair<string, UnionInfo> kvp in unionRoots)
        {
            UnionInfo? unionInfo = kvp.Value;
            var members = unionMembers
                .Where(m => m.RootInterface == unionInfo.RootInterface)
                .OrderBy(m => m.UnionIndex)
                .ToList();

            if (members.Count == 0)
                continue;

            try
            {
                string unionSource = GenerateUnionInterface(unionInfo, members);
                context.AddSource($"{unionInfo.UnionName}.g.cs", SourceText.From(unionSource, Encoding.UTF8));

                string extensionSource = GenerateUnionExtensions(unionInfo, members);
                context.AddSource($"{unionInfo.UnionName}Extensions.g.cs",
                    SourceText.From(extensionSource, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DTOGEN002",
                        "Union Generation Error",
                        $"Error generating Union for {unionInfo.RootInterface}:  {ex.Message}",
                        "DtoGenerator",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERFACE → DTO
    // ═══════════════════════════════════════════════════════════════════════════

    private static string GenerateDtoFromInterface(INamedTypeSymbol interfaceSymbol,
        out UnionMemberInfo? unionMemberInfo)
    {
        var sb = new StringBuilder();
        unionMemberInfo = null;

        GenerateDtoAttributeData generateDtoAttr = GetGenerateDtoAttribute(interfaceSymbol);

        string interfaceName = interfaceSymbol.Name;
        string baseName = interfaceName.StartsWith("I") && interfaceName.Length > 1 && char.IsUpper(interfaceName[1])
            ? interfaceName.Substring(1)
            : interfaceName;

        string dtoName = generateDtoAttr.DtoName ?? $"{baseName}{generateDtoAttr.DtoSuffix}";
        string dtoNamespace = generateDtoAttr.DtoNamespace ?? interfaceSymbol.ContainingNamespace.ToDisplayString();
        string dtoFullName = $"{dtoNamespace}.{dtoName}";

        // Check for Union membership
        unionMemberInfo = GetDtoUnionMemberAttribute(interfaceSymbol, dtoFullName);

        List<PropertyData> properties = CollectInterfaceProperties(interfaceSymbol);

        var interfaces = new List<string>();
        if (generateDtoAttr.ImplementSourceInterface) interfaces.Add(interfaceSymbol.ToDisplayString());

        List<INamedTypeSymbol> dtoImplementsAttrs = GetDtoImplementsAttributes(interfaceSymbol);
        foreach (INamedTypeSymbol? iface in dtoImplementsAttrs)
        {
            string ifaceName = iface.ToDisplayString();
            if (!interfaces.Contains(ifaceName)) interfaces.Add(ifaceName);
        }

        string interfaceList = interfaces.Count > 0 ? $" : {string.Join(", ", interfaces)}" : "";
        HashSet<string> usings = CollectUsings(interfaceSymbol, properties, interfaces);

        // Header
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// Generated by Mmo.Generators.DtoGenerator");
        sb.AppendLine($"// Source: {interfaceSymbol.ToDisplayString()}");
        sb.AppendLine("// Do not modify manually.");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        // Usings
        sb.AppendLine("using global::System;");
        sb.AppendLine("using global::System.Collections.Generic;");
        sb.AppendLine("using global::System.Linq;");
        sb.AppendLine("using MessagePack;");

        foreach (string? usingStatement in usings.OrderBy(u => u))
        {
            if (usingStatement is "System" or "System.Collections.Generic" or "System.Linq" or "MessagePack")
                continue;
            sb.AppendLine($"using {usingStatement};");
        }

        sb.AppendLine();
        sb.AppendLine($"namespace {dtoNamespace}");
        sb.AppendLine("{");

        // DTO CLASS
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Auto-generated DTO for <see cref=\"{interfaceSymbol.Name}\"/>.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [MessagePackObject]");
        sb.AppendLine($"    public sealed class {dtoName}{interfaceList}");
        sb.AppendLine("    {");

        // Parameterloser Constructor
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Parameterless constructor for MessagePack deserialization.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        [SerializationConstructor]");
        sb.AppendLine($"        public {dtoName}() {{ }}");
        sb.AppendLine();

        // Constructor mit Interface-Parameter
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Creates a new <see cref=\"{dtoName}\"/> from an <see cref=\"{interfaceSymbol.Name}\"/> instance.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine($"        /// <param name=\"source\">The source {interfaceSymbol.Name} to copy from.</param>");
        sb.AppendLine($"        public {dtoName}({interfaceSymbol.ToDisplayString()} source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        foreach (PropertyData? prop in properties)
        {
            string propName = prop.CustomName ?? prop.Symbol.Name;
            sb.AppendLine($"            {propName} = source.{prop.Symbol.Name};");
        }

        sb.AppendLine("        }");
        sb.AppendLine();

        // Properties
        foreach (PropertyData? prop in properties)
        {
            string propName = prop.CustomName ?? prop.Symbol.Name;
            string propType = GetFullTypeName(prop.Symbol.Type);
            int keyIndex = prop.ExplicitKey;

            string? defaultValue = GetDefaultValue(prop.Symbol.Type);
            string defaultAssignment = defaultValue != null ? $" = {defaultValue};" : "";

            sb.AppendLine($"        [Key({keyIndex})]");
            sb.AppendLine($"        public {propType} {propName} {{ get; init; }}{defaultAssignment}");
            sb.AppendLine();
        }

        // Computed Properties
        List<IPropertySymbol> computedProperties = GetComputedProperties(interfaceSymbol);
        foreach (IPropertySymbol? computed in computedProperties)
        {
            string computedType = GetFullTypeName(computed.Type);
            string? computedImpl = GetComputedPropertyImplementation(computed);

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Computed property from interface (not serialized).");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [IgnoreMember]");

            if (computedImpl != null)
                sb.AppendLine($"        public {computedType} {computed.Name} => {computedImpl};");
            else
                sb.AppendLine($"        public {computedType} {computed.Name} => default! ;");

            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // EXTENSION METHODS
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(
            $"    /// Extension methods for converting <see cref=\"{interfaceSymbol.Name}\"/> to <see cref=\"{dtoName}\"/>.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static class {dtoName}Extensions");
        sb.AppendLine("    {");

        // ToDto()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Converts an <see cref=\"{interfaceSymbol.Name}\"/> to a <see cref=\"{dtoName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source to convert.</param>");
        sb.AppendLine($"        /// <returns>A new <see cref=\"{dtoName}\"/> instance.</returns>");
        sb.AppendLine($"        public static {dtoName} ToDto(this {interfaceSymbol.ToDisplayString()} source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine($"            return new {dtoName}");
        sb.AppendLine("            {");
        foreach (PropertyData? prop in properties)
        {
            string propName = prop.CustomName ?? prop.Symbol.Name;
            sb.AppendLine($"                {propName} = source.{prop.Symbol.Name},");
        }

        sb.AppendLine("            };");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ToDtoList()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Converts a collection of <see cref=\"{interfaceSymbol.Name}\"/> to a list of <see cref=\"{dtoName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source collection to convert.</param>");
        sb.AppendLine($"        /// <returns>A new list of <see cref=\"{dtoName}\"/> instances.</returns>");
        sb.AppendLine(
            $"        public static List<{dtoName}> ToDtoList(this IEnumerable<{interfaceSymbol.ToDisplayString()}> source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine("            return source.Select(x => x.ToDto()).ToList();");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ToDtoArray()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Converts a collection of <see cref=\"{interfaceSymbol.Name}\"/> to an array of <see cref=\"{dtoName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source collection to convert.</param>");
        sb.AppendLine($"        /// <returns>A new array of <see cref=\"{dtoName}\"/> instances.</returns>");
        sb.AppendLine(
            $"        public static {dtoName}[] ToDtoArray(this IEnumerable<{interfaceSymbol.ToDisplayString()}> source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine("            return source.Select(x => x.ToDto()).ToArray();");
        sb.AppendLine("        }");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CLASS/STRUCT → DTO
    // ═══════════════════════════════════════════════════════════════════════════

    private static string GenerateDtoFromClassOrStruct(INamedTypeSymbol typeSymbol)
    {
        var sb = new StringBuilder();

        GenerateDtoAttributeData generateDtoAttr = GetGenerateDtoAttribute(typeSymbol);
        List<INamedTypeSymbol> dtoImplementsAttrs = GetDtoImplementsAttributes(typeSymbol);

        bool inheritInterfaces = generateDtoAttr.InheritInterfaces;
        string dtoName = generateDtoAttr.DtoName ?? $"{typeSymbol.Name}{generateDtoAttr.DtoSuffix}";
        string dtoNamespace = generateDtoAttr.DtoNamespace ?? typeSymbol.ContainingNamespace.ToDisplayString();

        List<string> interfaces = CollectInterfaces(typeSymbol, inheritInterfaces, dtoImplementsAttrs);
        string interfaceList = interfaces.Count > 0 ? $" : {string.Join(", ", interfaces)}" : "";

        List<PropertyData> properties = CollectClassProperties(typeSymbol);
        HashSet<string> usings = CollectUsings(typeSymbol, properties, interfaces);

        // Header
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// Generated by Mmo.Generators.DtoGenerator");
        sb.AppendLine($"// Source: {typeSymbol.ToDisplayString()}");
        sb.AppendLine("// Do not modify manually.");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        // Usings
        sb.AppendLine("using global::System;");
        sb.AppendLine("using global::System.Collections.Generic;");
        sb.AppendLine("using global::System.Linq;");
        sb.AppendLine("using MessagePack;");

        foreach (string? usingStatement in usings.OrderBy(u => u))
        {
            if (usingStatement is "System" or "System.Collections.Generic" or "System.Linq" or "MessagePack")
                continue;
            sb.AppendLine($"using {usingStatement};");
        }

        sb.AppendLine();
        sb.AppendLine($"namespace {dtoNamespace}");
        sb.AppendLine("{");

        // DTO CLASS
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Auto-generated DTO for <see cref=\"{typeSymbol.Name}\"/>.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [MessagePackObject]");
        sb.AppendLine($"    public sealed class {dtoName}{interfaceList}");
        sb.AppendLine("    {");

        // Parameterloser Constructor
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Parameterless constructor for MessagePack deserialization.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        [SerializationConstructor]");
        sb.AppendLine($"        public {dtoName}() {{ }}");
        sb.AppendLine();

        // Constructor mit Source-Parameter
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Creates a new <see cref=\"{dtoName}\"/> from a <see cref=\"{typeSymbol.Name}\"/> instance.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine($"        /// <param name=\"source\">The source {typeSymbol.Name} to copy from.</param>");
        sb.AppendLine($"        public {dtoName}({typeSymbol.ToDisplayString()} source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        foreach (PropertyData? prop in properties)
        {
            string propName = prop.CustomName ?? prop.Symbol.Name;
            sb.AppendLine($"            {propName} = source.{prop.Symbol.Name};");
        }

        sb.AppendLine("        }");
        sb.AppendLine();

        // Properties
        int keyIndex = 0;
        foreach (PropertyData? prop in properties)
        {
            string propName = prop.CustomName ?? prop.Symbol.Name;
            string propType = GetFullTypeName(prop.Symbol.Type);
            int keyIndexToUse = prop.ExplicitKey >= 0 ? prop.ExplicitKey : keyIndex;

            string? defaultValue = GetDefaultValue(prop.Symbol.Type);
            string defaultAssignment = defaultValue != null ? $" = {defaultValue};" : "";

            sb.AppendLine($"        [Key({keyIndexToUse})]");
            sb.AppendLine($"        public {propType} {propName} {{ get; init; }}{defaultAssignment}");
            sb.AppendLine();

            if (prop.ExplicitKey < 0)
                keyIndex++;
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // EXTENSION METHODS
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(
            $"    /// Extension methods for converting <see cref=\"{typeSymbol.Name}\"/> to <see cref=\"{dtoName}\"/>.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static class {dtoName}Extensions");
        sb.AppendLine("    {");

        // ToDto()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine($"        /// Converts a <see cref=\"{typeSymbol.Name}\"/> to a <see cref=\"{dtoName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source to convert.</param>");
        sb.AppendLine($"        /// <returns>A new <see cref=\"{dtoName}\"/> instance.</returns>");
        sb.AppendLine($"        public static {dtoName} ToDto(this {typeSymbol.ToDisplayString()} source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine($"            return new {dtoName}");
        sb.AppendLine("            {");
        foreach (PropertyData? prop in properties)
        {
            string propName = prop.CustomName ?? prop.Symbol.Name;
            sb.AppendLine($"                {propName} = source.{prop.Symbol.Name},");
        }

        sb.AppendLine("            };");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ToDtoList()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Converts a collection of <see cref=\"{typeSymbol.Name}\"/> to a list of <see cref=\"{dtoName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source collection to convert.</param>");
        sb.AppendLine($"        /// <returns>A new list of <see cref=\"{dtoName}\"/> instances.</returns>");
        sb.AppendLine(
            $"        public static List<{dtoName}> ToDtoList(this IEnumerable<{typeSymbol.ToDisplayString()}> source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine("            return source.Select(x => x.ToDto()).ToList();");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ToDtoArray()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Converts a collection of <see cref=\"{typeSymbol.Name}\"/> to an array of <see cref=\"{dtoName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source collection to convert.</param>");
        sb.AppendLine($"        /// <returns>A new array of <see cref=\"{dtoName}\"/> instances.</returns>");
        sb.AppendLine(
            $"        public static {dtoName}[] ToDtoArray(this IEnumerable<{typeSymbol.ToDisplayString()}> source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine("            return source.Select(x => x.ToDto()).ToArray();");
        sb.AppendLine("        }");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UNION GENERATION
    // ═══════════════════════════════════════════════════════════════════════════

    private static string GenerateUnionInterface(UnionInfo unionInfo, List<UnionMemberInfo> members)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// Generated by Mmo.Generators.DtoGenerator");
        sb.AppendLine("// Do not modify manually.");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using MessagePack;");

        var namespaces = new HashSet<string>();
        foreach (UnionMemberInfo? member in members)
        {
            int lastDot = member.DtoFullName.LastIndexOf('.');
            if (lastDot > 0)
            {
                string ns = member.DtoFullName.Substring(0, lastDot);
                if (ns != unionInfo.Namespace) namespaces.Add(ns);
            }
        }

        foreach (string? ns in namespaces.OrderBy(n => n)) sb.AppendLine($"using {ns};");

        sb.AppendLine();
        sb.AppendLine($"namespace {unionInfo.Namespace}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(
            $"    /// Auto-generated Union interface for polymorphic serialization of {unionInfo.RootInterfaceName}.");
        sb.AppendLine("    /// </summary>");

        foreach (UnionMemberInfo? member in members.OrderBy(m => m.UnionIndex))
        {
            string dtoTypeName = GetTypeName(member.DtoFullName);
            sb.AppendLine($"    [Union({member.UnionIndex}, typeof({dtoTypeName}))]");
        }

        sb.AppendLine($"    public interface {unionInfo.UnionName}");
        sb.AppendLine("    {");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateUnionExtensions(UnionInfo unionInfo, List<UnionMemberInfo> members)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// Generated by Mmo.Generators.DtoGenerator");
        sb.AppendLine("// Do not modify manually.");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using global::System;");
        sb.AppendLine("using global::System.Collections.Generic;");
        sb.AppendLine("using global::System.Linq;");

        var namespaces = new HashSet<string>();

        int rootNsIndex = unionInfo.RootInterface.LastIndexOf('.');
        if (rootNsIndex > 0) namespaces.Add(unionInfo.RootInterface.Substring(0, rootNsIndex));

        foreach (UnionMemberInfo? member in members)
        {
            int sourceNsIndex = member.SourceInterface.LastIndexOf('.');
            if (sourceNsIndex > 0) namespaces.Add(member.SourceInterface.Substring(0, sourceNsIndex));

            int dtoNsIndex = member.DtoFullName.LastIndexOf('.');
            if (dtoNsIndex > 0) namespaces.Add(member.DtoFullName.Substring(0, dtoNsIndex));
        }

        foreach (string? ns in namespaces.OrderBy(n => n))
            if (ns != unionInfo.Namespace)
                sb.AppendLine($"using {ns};");

        sb.AppendLine();
        sb.AppendLine($"namespace {unionInfo.Namespace}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(
            $"    /// Extension methods for polymorphic conversion of {unionInfo.RootInterfaceName} to {unionInfo.UnionName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static class {unionInfo.UnionName}Extensions");
        sb.AppendLine("    {");

        // ToUnionDto()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Converts an <see cref=\"{unionInfo.RootInterfaceName}\"/> to the appropriate <see cref=\"{unionInfo.UnionName}\"/> based on runtime type.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source to convert.</param>");
        sb.AppendLine(
            $"        /// <returns>The appropriate DTO implementing <see cref=\"{unionInfo.UnionName}\"/>.</returns>");
        sb.AppendLine($"        public static {unionInfo.UnionName} ToUnionDto(this {unionInfo.RootInterface} source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine("            return source switch");
        sb.AppendLine("            {");

        foreach (UnionMemberInfo? member in members.OrderByDescending(m => m.InterfaceDepth))
        {
            string sourceTypeName = GetTypeName(member.SourceInterface);
            sb.AppendLine($"                {sourceTypeName} x => x.ToDto(),");
        }

        sb.AppendLine(
            $"                _ => throw new ArgumentException($\"Unknown {unionInfo.RootInterfaceName} type:  {{source.GetType().Name}}\", nameof(source))");
        sb.AppendLine("            };");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ToUnionDtoList()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Converts a collection of <see cref=\"{unionInfo.RootInterfaceName}\"/> to a list of <see cref=\"{unionInfo.UnionName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source collection to convert.</param>");
        sb.AppendLine($"        /// <returns>A new list of <see cref=\"{unionInfo.UnionName}\"/> instances.</returns>");
        sb.AppendLine(
            $"        public static List<{unionInfo.UnionName}> ToUnionDtoList(this IEnumerable<{unionInfo.RootInterface}> source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine("            return source.Select(x => x.ToUnionDto()).ToList();");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ToUnionDtoArray()
        sb.AppendLine("        /// <summary>");
        sb.AppendLine(
            $"        /// Converts a collection of <see cref=\"{unionInfo.RootInterfaceName}\"/> to an array of <see cref=\"{unionInfo.UnionName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"source\">The source collection to convert.</param>");
        sb.AppendLine(
            $"        /// <returns>A new array of <see cref=\"{unionInfo.UnionName}\"/> instances.</returns>");
        sb.AppendLine(
            $"        public static {unionInfo.UnionName}[] ToUnionDtoArray(this IEnumerable<{unionInfo.RootInterface}> source)");
        sb.AppendLine("        {");
        sb.AppendLine("            ArgumentNullException.ThrowIfNull(source);");
        sb.AppendLine();
        sb.AppendLine("            return source.Select(x => x.ToUnionDto()).ToArray();");
        sb.AppendLine("        }");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTY COLLECTION - INTERFACES
    // ═══════════════════════════════════════════════════════════════════════════

    private static List<PropertyData> CollectInterfaceProperties(INamedTypeSymbol interfaceSymbol)
    {
        var properties = new List<PropertyData>();
        var processedNames = new HashSet<string>();

        var allInterfaces = new List<INamedTypeSymbol>();
        CollectInterfaceHierarchy(interfaceSymbol, allInterfaces);

        int autoKeyIndex = 0;

        foreach (INamedTypeSymbol? iface in allInterfaces)
        {
            var members = iface.GetMembers()
                .OfType<IPropertySymbol>()
                .OrderBy(p => p.Name)
                .ToList();

            foreach (IPropertySymbol? propertySymbol in members)
            {
                if (processedNames.Contains(propertySymbol.Name))
                    continue;

                if (propertySymbol.GetMethod == null)
                    continue;

                if (propertySymbol.IsIndexer)
                    continue;

                if (IsComputedProperty(propertySymbol))
                {
                    processedNames.Add(propertySymbol.Name);
                    continue;
                }

                ImmutableArray<AttributeData> attributes = propertySymbol.GetAttributes();
                int explicitKey = -1;
                string? customName = null;

                foreach (AttributeData? attr in attributes)
                {
                    string? attrName = attr.AttributeClass?.Name;

                    if (attrName == "DtoPropertyAttribute")
                        foreach (KeyValuePair<string, TypedConstant> namedArg in attr.NamedArguments)
                            switch (namedArg.Key)
                            {
                                case "Name":
                                    customName = namedArg.Value.Value as string;
                                    break;
                                case "Key":
                                    if (namedArg.Value.Value is int keyValue)
                                        explicitKey = keyValue;
                                    break;
                            }
                }

                processedNames.Add(propertySymbol.Name);

                int finalKey = explicitKey >= 0 ? explicitKey : autoKeyIndex++;

                properties.Add(new PropertyData
                {
                    Symbol = propertySymbol,
                    CustomName = customName,
                    ExplicitKey = finalKey
                });
            }
        }

        return properties.OrderBy(p => p.ExplicitKey).ToList();
    }

    private static void CollectInterfaceHierarchy(INamedTypeSymbol iface, List<INamedTypeSymbol> result)
    {
        foreach (INamedTypeSymbol? baseInterface in iface.Interfaces) CollectInterfaceHierarchy(baseInterface, result);

        if (!result.Contains(iface)) result.Add(iface);
    }

    private static bool IsComputedProperty(IPropertySymbol property)
    {
        if (property.IsAbstract)
            return false;

        if (property.SetMethod != null)
            return false;

        foreach (SyntaxReference? syntaxRef in property.DeclaringSyntaxReferences)
        {
            SyntaxNode syntax = syntaxRef.GetSyntax();
            if (syntax is PropertyDeclarationSyntax propSyntax)
            {
                if (propSyntax.ExpressionBody != null)
                    return true;

                AccessorDeclarationSyntax? getter = propSyntax.AccessorList?.Accessors
                    .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

                if (getter?.Body != null || getter?.ExpressionBody != null)
                    return true;
            }
        }

        return false;
    }

    private static List<IPropertySymbol> GetComputedProperties(INamedTypeSymbol interfaceSymbol)
    {
        var computed = new List<IPropertySymbol>();
        var processedNames = new HashSet<string>();

        var allInterfaces = new List<INamedTypeSymbol>();
        CollectInterfaceHierarchy(interfaceSymbol, allInterfaces);

        foreach (INamedTypeSymbol? iface in allInterfaces)
        foreach (ISymbol? member in iface.GetMembers())
            if (member is IPropertySymbol prop &&
                prop.GetMethod != null &&
                !processedNames.Contains(prop.Name) &&
                IsComputedProperty(prop))
            {
                computed.Add(prop);
                processedNames.Add(prop.Name);
            }

        return computed;
    }

    private static string? GetComputedPropertyImplementation(IPropertySymbol property)
    {
        foreach (SyntaxReference? syntaxRef in property.DeclaringSyntaxReferences)
        {
            SyntaxNode syntax = syntaxRef.GetSyntax();
            if (syntax is PropertyDeclarationSyntax propSyntax)
            {
                if (propSyntax.ExpressionBody != null) return propSyntax.ExpressionBody.Expression.ToString();

                AccessorDeclarationSyntax? getter = propSyntax.AccessorList?.Accessors
                    .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

                if (getter?.ExpressionBody != null) return getter.ExpressionBody.Expression.ToString();
            }
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTY COLLECTION - CLASSES/STRUCTS
    // ═══════════════════════════════════════════════════════════════════════════

    private static List<PropertyData> CollectClassProperties(INamedTypeSymbol typeSymbol)
    {
        var properties = new List<PropertyData>();
        var processedNames = new HashSet<string>();

        INamedTypeSymbol? currentType = typeSymbol;
        while (currentType != null)
        {
            foreach (ISymbol? member in currentType.GetMembers())
            {
                if (member is not IPropertySymbol propertySymbol)
                    continue;

                if (processedNames.Contains(propertySymbol.Name))
                    continue;

                if (propertySymbol.GetMethod == null)
                    continue;

                if (propertySymbol.IsStatic)
                    continue;

                ImmutableArray<AttributeData> attributes = propertySymbol.GetAttributes();
                bool shouldSkip = false;

                foreach (AttributeData? attr in attributes)
                {
                    string? attrName = attr.AttributeClass?.Name;
                    if (attrName is "ServerOnlyAttribute" or "DtoIgnoreAttribute")
                    {
                        shouldSkip = true;
                        break;
                    }
                }

                if (shouldSkip)
                {
                    processedNames.Add(propertySymbol.Name);
                    continue;
                }

                var propertyData = new PropertyData
                {
                    Symbol = propertySymbol,
                    CustomName = null,
                    ExplicitKey = -1
                };

                foreach (AttributeData? attr in attributes)
                {
                    if (attr.AttributeClass?.Name != "DtoPropertyAttribute")
                        continue;

                    foreach (KeyValuePair<string, TypedConstant> namedArg in attr.NamedArguments)
                        switch (namedArg.Key)
                        {
                            case "Name":
                                propertyData.CustomName = namedArg.Value.Value as string;
                                break;
                            case "Key":
                                if (namedArg.Value.Value is int keyValue)
                                    propertyData.ExplicitKey = keyValue;
                                break;
                        }
                }

                properties.Add(propertyData);
                processedNames.Add(propertySymbol.Name);
            }

            currentType = currentType.BaseType;

            if (currentType?.SpecialType == SpecialType.System_Object)
                break;
        }

        return properties;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    private static string GetDtoName(INamedTypeSymbol typeSymbol)
    {
        GenerateDtoAttributeData attr = GetGenerateDtoAttribute(typeSymbol);

        if (attr.DtoName != null)
            return attr.DtoName;

        if (typeSymbol.TypeKind == TypeKind.Interface)
        {
            string name = typeSymbol.Name;
            string baseName = name.StartsWith("I") && name.Length > 1 && char.IsUpper(name[1])
                ? name.Substring(1)
                : name;
            return $"{baseName}{attr.DtoSuffix}";
        }

        return $"{typeSymbol.Name}{attr.DtoSuffix}";
    }

    private static string GetBaseName(INamedTypeSymbol typeSymbol)
    {
        string name = typeSymbol.Name;
        if (typeSymbol.TypeKind == TypeKind.Interface && name.StartsWith("I") && name.Length > 1 &&
            char.IsUpper(name[1]))
            return name.Substring(1);

        return name;
    }

    private static string GetTypeName(string fullName)
    {
        int lastDot = fullName.LastIndexOf('.');
        return lastDot > 0 ? fullName.Substring(lastDot + 1) : fullName;
    }

    private static string? GetDefaultValue(ITypeSymbol type)
    {
        if (type.IsValueType)
            return null;

        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            return null;

        if (type.SpecialType == SpecialType.System_String)
            return "\"\"";

        return "default! ";
    }

    private static string GetFullTypeName(ITypeSymbol type)
    {
        string typeName = type.ToDisplayString();

        if (typeName.StartsWith("System.")) return "global::" + typeName;

        if (type is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            ITypeSymbol innerType = namedType.TypeArguments[0];
            string innerTypeName = GetFullTypeName(innerType);
            return innerTypeName + "?";
        }

        return typeName;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ATTRIBUTE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private static GenerateDtoAttributeData GetGenerateDtoAttribute(INamedTypeSymbol typeSymbol)
    {
        AttributeData? attr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "GenerateDtoAttribute");

        var data = new GenerateDtoAttributeData();

        if (attr == null)
            return data;

        foreach (KeyValuePair<string, TypedConstant> namedArg in attr.NamedArguments)
            switch (namedArg.Key)
            {
                case "InheritInterfaces":
                    data.InheritInterfaces = (bool)(namedArg.Value.Value ?? false);
                    break;
                case "DtoName":
                    data.DtoName = namedArg.Value.Value as string;
                    break;
                case "DtoNamespace":
                    data.DtoNamespace = namedArg.Value.Value as string;
                    break;
                case "DtoSuffix":
                    data.DtoSuffix = namedArg.Value.Value as string ?? "Dto";
                    break;
                case "ImplementSourceInterface":
                    data.ImplementSourceInterface = (bool)(namedArg.Value.Value ?? true);
                    break;
            }

        return data;
    }

    private static GenerateDtoUnionAttributeData? GetGenerateDtoUnionAttribute(INamedTypeSymbol typeSymbol)
    {
        AttributeData? attr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "GenerateDtoUnionAttribute");

        if (attr == null)
            return null;

        var data = new GenerateDtoUnionAttributeData();

        foreach (KeyValuePair<string, TypedConstant> namedArg in attr.NamedArguments)
            switch (namedArg.Key)
            {
                case "UnionName":
                    data.UnionName = namedArg.Value.Value as string;
                    break;
                case "Namespace":
                    data.Namespace = namedArg.Value.Value as string;
                    break;
            }

        return data;
    }

    private static UnionMemberInfo? GetDtoUnionMemberAttribute(INamedTypeSymbol typeSymbol, string dtoFullName)
    {
        AttributeData? attr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "DtoUnionMemberAttribute");

        if (attr == null)
            return null;

        if (attr.ConstructorArguments.Length < 2)
            return null;

        int unionIndex = (int)attr.ConstructorArguments[0].Value!;
        var unionType = attr.ConstructorArguments[1].Value as INamedTypeSymbol;

        if (unionType == null)
            return null;

        int depth = CalculateInterfaceDepth(typeSymbol);

        return new UnionMemberInfo
        {
            UnionIndex = unionIndex,
            SourceInterface = typeSymbol.ToDisplayString(),
            DtoFullName = dtoFullName,
            RootInterface = unionType.ToDisplayString(),
            InterfaceDepth = depth
        };
    }

    private static int CalculateInterfaceDepth(INamedTypeSymbol typeSymbol)
    {
        int depth = 0;
        var visited = new HashSet<string>();
        var queue = new Queue<INamedTypeSymbol>();
        queue.Enqueue(typeSymbol);

        while (queue.Count > 0)
        {
            INamedTypeSymbol? current = queue.Dequeue();
            string key = current.ToDisplayString();

            if (visited.Contains(key))
                continue;

            visited.Add(key);
            depth++;

            foreach (INamedTypeSymbol? baseInterface in current.Interfaces) queue.Enqueue(baseInterface);
        }

        return depth;
    }

    private static List<INamedTypeSymbol> GetDtoImplementsAttributes(INamedTypeSymbol typeSymbol)
    {
        var result = new List<INamedTypeSymbol>();

        foreach (AttributeData? attr in typeSymbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name != "DtoImplementsAttribute")
                continue;

            if (attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is INamedTypeSymbol typeArg)
                result.Add(typeArg);
        }

        return result;
    }

    private static List<string> CollectInterfaces(INamedTypeSymbol typeSymbol, bool inheritInterfaces,
        List<INamedTypeSymbol> explicitInterfaces)
    {
        var interfaces = new List<string>();

        if (inheritInterfaces)
            foreach (INamedTypeSymbol? iface in typeSymbol.AllInterfaces)
                interfaces.Add(iface.ToDisplayString());

        foreach (INamedTypeSymbol? iface in explicitInterfaces)
        {
            string ifaceName = iface.ToDisplayString();
            if (!interfaces.Contains(ifaceName)) interfaces.Add(ifaceName);
        }

        return interfaces;
    }

    private static HashSet<string> CollectUsings(INamedTypeSymbol typeSymbol, List<PropertyData> properties,
        List<string> interfaces)
    {
        var usings = new HashSet<string>();

        string? sourceNamespace = typeSymbol.ContainingNamespace?.ToDisplayString();
        if (!string.IsNullOrEmpty(sourceNamespace) && sourceNamespace != "<global namespace>")
            usings.Add(sourceNamespace);

        foreach (PropertyData? prop in properties) AddTypeUsings(prop.Symbol.Type, usings);

        foreach (string? iface in interfaces)
        {
            int lastDot = iface.LastIndexOf('.');
            if (lastDot > 0) usings.Add(iface.Substring(0, lastDot));
        }

        return usings;
    }

    private static void AddTypeUsings(ITypeSymbol type, HashSet<string> usings)
    {
        string? ns = type.ContainingNamespace?.ToDisplayString();
        if (!string.IsNullOrEmpty(ns) && ns != "<global namespace>") usings.Add(ns);

        if (type is INamedTypeSymbol namedType)
            foreach (ITypeSymbol? typeArg in namedType.TypeArguments)
                AddTypeUsings(typeArg, usings);

        if (type is IArrayTypeSymbol arrayType) AddTypeUsings(arrayType.ElementType, usings);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER CLASSES
    // ═══════════════════════════════════════════════════════════════════════════

    private sealed class GenerateDtoAttributeData
    {
        public bool InheritInterfaces { get; set; }
        public string? DtoName { get; set; }
        public string? DtoNamespace { get; set; }
        public string DtoSuffix { get; set; } = "Dto";
        public bool ImplementSourceInterface { get; set; } = true;
    }

    private sealed class GenerateDtoUnionAttributeData
    {
        public string? UnionName { get; set; }
        public string? Namespace { get; set; }
    }

    private sealed class UnionInfo
    {
        public string RootInterface { get; set; } = "";
        public string RootInterfaceName { get; set; } = "";
        public string UnionName { get; set; } = "";
        public string Namespace { get; set; } = "";
    }

    private sealed class UnionMemberInfo
    {
        public int UnionIndex { get; set; }
        public string SourceInterface { get; set; } = "";
        public string DtoFullName { get; set; } = "";
        public string RootInterface { get; set; } = "";
        public int InterfaceDepth { get; set; }
    }

    private sealed class PropertyData
    {
        public IPropertySymbol Symbol { get; set; } = null!;
        public string? CustomName { get; set; }
        public int ExplicitKey { get; set; } = -1;
    }
}
