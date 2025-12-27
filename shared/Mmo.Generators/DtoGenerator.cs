using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mmo.Generators
{
    [Generator]
    public class DtoGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsCandidateForGeneration(s),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null);

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
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

            foreach (var attributeList in typeDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
                        continue;

                    var attributeContainingType = attributeSymbol.ContainingType;
                    var fullName = attributeContainingType.ToDisplayString();

                    if (fullName == "Mmo.Shared.Generators.GenerateDtoAttribute")
                    {
                        return typeDeclaration;
                    }
                }
            }

            return null;
        }

        private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax?> classes,
            SourceProductionContext context)
        {
            // DEBUG: Immer eine Datei generieren um zu sehen ob Generator läuft
            var debugSb = new StringBuilder();
            debugSb.AppendLine("// Generator Debug Output");
            debugSb.AppendLine($"// Timestamp: {DateTime.Now}");
            debugSb.AppendLine($"// Compilation Assembly: {compilation.AssemblyName}");
            debugSb.AppendLine($"// Candidate classes count: {classes.Length}");
            debugSb.AppendLine();

            // Liste alle gefundenen Kandidaten
            foreach (var cls in classes.Where(c => c != null).Take(10))
            {
                debugSb.AppendLine($"// Candidate:  {cls!.Identifier.Text}");
            }

            context.AddSource("_GeneratorDebug.g.cs", SourceText.From(debugSb.ToString(), Encoding.UTF8));

            if (classes.IsDefaultOrEmpty)
            {
                context.AddSource("_NoClassesFound.g.cs",
                    SourceText.From("// ERROR: No classes with [GenerateDto] attribute found!", Encoding.UTF8));
                return;
            }

            var distinctClasses = classes.Where(c => c != null).Distinct().ToList();

            foreach (var typeDeclaration in distinctClasses)
            {
                if (typeDeclaration == null)
                    continue;

                var semanticModel = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;

                if (typeSymbol == null)
                    continue;

                try
                {
                    var sourceCode = GenerateDto(typeSymbol);
                    var fileName = $"{typeSymbol.Name}Dto.g.cs";
                    context.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
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
        }

        private static string GenerateDto(INamedTypeSymbol typeSymbol)
        {
            var sb = new StringBuilder();

            var generateDtoAttr = GetGenerateDtoAttribute(typeSymbol);
            var dtoImplementsAttrs = GetDtoImplementsAttributes(typeSymbol);

            var inheritInterfaces = generateDtoAttr.InheritInterfaces;
            var dtoName = generateDtoAttr.DtoName ?? $"{typeSymbol.Name}{generateDtoAttr.DtoSuffix}";
            var dtoNamespace = generateDtoAttr.DtoNamespace ?? typeSymbol.ContainingNamespace.ToDisplayString();

            var interfaces = CollectInterfaces(typeSymbol, inheritInterfaces, dtoImplementsAttrs);
            var interfaceList = interfaces.Count > 0 ? $" : {string.Join(", ", interfaces)}" : "";

            // Properties sammeln (inkl. Basis-Klassen!)
            var properties = CollectProperties(typeSymbol);

            var usings = CollectUsings(typeSymbol, properties, interfaces);

            // Header
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// This file was generated by Mmo.Generators.DtoGenerator");
            sb.AppendLine("// Do not modify this file manually.");
            sb.AppendLine();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            // Usings - global::System zuerst um Namespace-Konflikte zu vermeiden
            sb.AppendLine("using global::System;");
            sb.AppendLine("using MessagePack;");

            foreach (var usingStatement in usings.OrderBy(u => u))
            {
                if (usingStatement == "System" || usingStatement == "MessagePack")
                    continue;

                sb.AppendLine($"using {usingStatement};");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {dtoNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Auto-generated DTO for <see cref=\"{typeSymbol.Name}\"/>.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    [MessagePackObject]");
            sb.AppendLine($"    public sealed class {dtoName}{interfaceList}");
            sb.AppendLine("    {");

            // Properties
            int keyIndex = 0;
            foreach (var prop in properties)
            {
                var propName = prop.CustomName ?? prop.Symbol.Name;
                var propType = GetFullTypeName(prop.Symbol.Type);
                var keyIndexToUse = prop.ExplicitKey >= 0 ? prop.ExplicitKey : keyIndex;

                sb.AppendLine($"        [Key({keyIndexToUse})]");
                sb.AppendLine($"        public {propType} {propName} {{ get; set; }}");
                sb.AppendLine();

                if (prop.ExplicitKey < 0)
                    keyIndex++;
            }

            // FromEntity Methode
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Creates a {dtoName} from a {typeSymbol.Name}.");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine(
                $"        public static {dtoName} From{typeSymbol.Name}({typeSymbol.ToDisplayString()} source)");
            sb.AppendLine("        {");
            sb.AppendLine($"            return new {dtoName}");
            sb.AppendLine("            {");

            foreach (var prop in properties)
            {
                var propName = prop.CustomName ?? prop.Symbol.Name;
                var sourcePropName = prop.Symbol.Name;
                sb.AppendLine($"                {propName} = source.{sourcePropName},");
            }

            sb.AppendLine("            };");
            sb.AppendLine("        }");

            // ApplyTo Methode
            sb.AppendLine();
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Applies this DTO's values to an existing {typeSymbol.Name}.");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        public void ApplyTo({typeSymbol.ToDisplayString()} target)");
            sb.AppendLine("        {");

            foreach (var prop in properties.Where(p => !p.Symbol.IsReadOnly && p.Symbol.SetMethod != null))
            {
                var propName = prop.CustomName ?? prop.Symbol.Name;
                var targetPropName = prop.Symbol.Name;
                sb.AppendLine($"            target.{targetPropName} = this.{propName};");
            }

            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Gibt den vollen Typnamen zurück, mit global:: für System-Typen
        /// </summary>
        private static string GetFullTypeName(ITypeSymbol type)
        {
            var typeName = type.ToDisplayString();

            // Ersetze System.  mit global::System.  um Namespace-Konflikte zu vermeiden
            if (typeName.StartsWith("System."))
            {
                return "global: :" + typeName;
            }

            // Für nullable System-Typen
            if (type is INamedTypeSymbol namedType &&
                namedType.IsGenericType &&
                namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                var innerType = namedType.TypeArguments[0];
                var innerTypeName = GetFullTypeName(innerType);
                return innerTypeName + "? ";
            }

            return typeName;
        }

        private static GenerateDtoAttributeData GetGenerateDtoAttribute(INamedTypeSymbol typeSymbol)
        {
            var attr = typeSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "GenerateDtoAttribute");

            var data = new GenerateDtoAttributeData();

            if (attr == null)
                return data;

            foreach (var namedArg in attr.NamedArguments)
            {
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
                        data.DtoSuffix = (namedArg.Value.Value as string) ?? "Dto";
                        break;
                }
            }

            return data;
        }

        private static List<INamedTypeSymbol> GetDtoImplementsAttributes(INamedTypeSymbol typeSymbol)
        {
            var result = new List<INamedTypeSymbol>();

            foreach (var attr in typeSymbol.GetAttributes())
            {
                if (attr.AttributeClass?.Name != "DtoImplementsAttribute")
                    continue;

                if (attr.ConstructorArguments.Length > 0 &&
                    attr.ConstructorArguments[0].Value is INamedTypeSymbol typeArg)
                {
                    result.Add(typeArg);
                }
            }

            return result;
        }

        private static List<string> CollectInterfaces(INamedTypeSymbol typeSymbol, bool inheritInterfaces,
            List<INamedTypeSymbol> explicitInterfaces)
        {
            var interfaces = new List<string>();

            if (inheritInterfaces)
            {
                foreach (var iface in typeSymbol.AllInterfaces)
                {
                    interfaces.Add(iface.ToDisplayString());
                }
            }

            foreach (var iface in explicitInterfaces)
            {
                var ifaceName = iface.ToDisplayString();
                if (!interfaces.Contains(ifaceName))
                {
                    interfaces.Add(ifaceName);
                }
            }

            return interfaces;
        }

        private static List<PropertyData> CollectProperties(INamedTypeSymbol typeSymbol)
        {
            var properties = new List<PropertyData>();
            var processedNames = new HashSet<string>();

            // Alle Properties sammeln (inkl. Basis-Klassen!)
            var currentType = typeSymbol;
            while (currentType != null)
            {
                foreach (var member in currentType.GetMembers())
                {
                    if (!(member is IPropertySymbol propertySymbol))
                        continue;

                    // Skip wenn bereits verarbeitet (Override in abgeleiteter Klasse)
                    if (processedNames.Contains(propertySymbol.Name))
                        continue;

                    if (propertySymbol.GetMethod == null)
                        continue;

                    if (propertySymbol.IsStatic)
                        continue;

                    var attributes = propertySymbol.GetAttributes();
                    var shouldSkip = false;

                    foreach (var attr in attributes)
                    {
                        var attrName = attr.AttributeClass?.Name;
                        if (attrName == "ServerOnlyAttribute" || attrName == "DtoIgnoreAttribute")
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

                    foreach (var attr in attributes)
                    {
                        if (attr.AttributeClass?.Name != "DtoPropertyAttribute")
                            continue;

                        foreach (var namedArg in attr.NamedArguments)
                        {
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
                    }

                    properties.Add(propertyData);
                    processedNames.Add(propertySymbol.Name);
                }

                // Zur Basis-Klasse gehen
                currentType = currentType.BaseType;

                // Stop bei object
                if (currentType?.SpecialType == SpecialType.System_Object)
                    break;
            }

            return properties;
        }

        private static HashSet<string> CollectUsings(INamedTypeSymbol typeSymbol, List<PropertyData> properties,
            List<string> interfaces)
        {
            var usings = new HashSet<string>();

            var sourceNamespace = typeSymbol.ContainingNamespace?.ToDisplayString();
            if (!string.IsNullOrEmpty(sourceNamespace) && sourceNamespace != "<global namespace>")
            {
                usings.Add(sourceNamespace);
            }

            foreach (var prop in properties)
            {
                AddTypeUsings(prop.Symbol.Type, usings);
            }

            foreach (var iface in interfaces)
            {
                var lastDot = iface.LastIndexOf('.');
                if (lastDot > 0)
                {
                    usings.Add(iface.Substring(0, lastDot));
                }
            }

            return usings;
        }

        private static void AddTypeUsings(ITypeSymbol type, HashSet<string> usings)
        {
            var ns = type.ContainingNamespace?.ToDisplayString();
            if (!string.IsNullOrEmpty(ns) && ns != "<global namespace>")
            {
                usings.Add(ns);
            }

            if (type is INamedTypeSymbol namedType)
            {
                foreach (var typeArg in namedType.TypeArguments)
                {
                    AddTypeUsings(typeArg, usings);
                }
            }

            if (type is IArrayTypeSymbol arrayType)
            {
                AddTypeUsings(arrayType.ElementType, usings);
            }
        }

        private class GenerateDtoAttributeData
        {
            public bool InheritInterfaces { get; set; }
            public string? DtoName { get; set; }
            public string? DtoNamespace { get; set; }
            public string DtoSuffix { get; set; } = "Dto";
        }

        private class PropertyData
        {
            public IPropertySymbol Symbol { get; set; } = null!;
            public string? CustomName { get; set; }
            public int ExplicitKey { get; set; } = -1;
        }
    }
}
