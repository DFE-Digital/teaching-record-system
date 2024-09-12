using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using GeneratedTypeInfo = (string DestinationType, string ReferenceType);
using VersionedReference = (string DestinationFullyQualifiedTypeName, string ReferenceFullyQualifiedTypeName, int ReferenceType);

[Generator(LanguageNames.CSharp)]
public class VersionedDtoGenerator : ISourceGenerator
{
    private const string GenerateVersionedDtoAttributeName = "TeachingRecordSystem.Api.V3.GenerateVersionedDtoAttribute";
    private const string BaseNamespace = "TeachingRecordSystem.Api.V3";

    private const int RecordReferenceType = 0;
    private const int EnumReferenceType = 1;

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = (SyntaxReceiver)context.SyntaxReceiver!;

        var generatedTypes = new HashSet<GeneratedTypeInfo>();
        var generatedRecords = new Dictionary<string, GeneratedRecordInfo>();
        var versionedReferences = new List<VersionedReference>();

        foreach (var generateDtoInfo in syntaxReceiver.Records)
        {
            var semanticModel = context.Compilation.GetSemanticModel(generateDtoInfo.Record.SyntaxTree);
            var typeSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(generateDtoInfo.Record)!;
            var destinationNamespace = typeSymbol.ContainingNamespace.ToString();

            if (!IsVersionedNamespace(destinationNamespace))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidNamespace,
                        generateDtoInfo.Location,
                        messageArgs: destinationNamespace));
                continue;
            }

            // Extract the arguments from the GenerateVersionedDtoAttribute
            var attr = typeSymbol.GetAttributes().Single(a => a.AttributeClass?.ContainingNamespace + "." + a.AttributeClass?.Name == GenerateVersionedDtoAttributeName);
            var sourceType = (INamedTypeSymbol)attr.ConstructorArguments[0].Value!;
            var excludeMembers = attr.ConstructorArguments[1].Values!.Select(t => t.Value!.ToString()).ToArray();
            var sourceNamespace = sourceType.ContainingNamespace.ToString();

            if (!IsVersionedNamespace(sourceNamespace))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidReferenceNamespace,
                        generateDtoInfo.Location,
                        messageArgs: sourceNamespace));
                continue;
            }

            var sourceFullyQualifiedTypeName = sourceType.ContainingNamespace.ToString() + "." + sourceType.Name;
            var destinationVersion = GetTypeVersion(destinationNamespace);
            var destinationFullyQualifiedTypeName = destinationNamespace + "." + typeSymbol.Name;

            if (generatedTypes.Any(t => t.DestinationType == destinationFullyQualifiedTypeName))
            {
                return;
            }

            GeneratePartialRecordDeclaration(
                destinationFullyQualifiedTypeName,
                sourceFullyQualifiedTypeName,
                copyAttributes: false,
                excludeMembers);
        }

        void EnsureReference(VersionedReference reference)
        {
            if (!versionedReferences.Contains(reference))
            {
                versionedReferences.Add(reference);

                // If the type has already been defined we don't need to generate it
                if (context.Compilation.GetTypeByMetadataName(reference.DestinationFullyQualifiedTypeName) is not null)
                {
                    return;
                }

                // If we've already generated the type there's nothing to do
                if (generatedTypes.Any(t => t.DestinationType == reference.DestinationFullyQualifiedTypeName))
                {
                    return;
                }

                if (reference.ReferenceType == RecordReferenceType)
                {
                    GeneratePartialRecordDeclaration(
                        reference.DestinationFullyQualifiedTypeName,
                        reference.ReferenceFullyQualifiedTypeName,
                        copyAttributes: true,
                        excludeMembers: []);
                }
                else
                {
                    Debug.Assert(reference.ReferenceType == EnumReferenceType);

                    GenerateEnum(
                        reference.DestinationFullyQualifiedTypeName,
                        reference.ReferenceFullyQualifiedTypeName);
                }
            }
        }

        void GeneratePartialRecordDeclaration(
            string destinationFullyQualifiedTypeName,
            string referenceFullyQualifiedTypeName,
            bool copyAttributes,
            string[] excludeMembers)
        {
            var destinationVersion = GetTypeVersion(destinationFullyQualifiedTypeName);
            var destinationNamespace = GetNamespaceFromFullyQualifiedTypeName(destinationFullyQualifiedTypeName);
            var referenceNamespace = GetNamespaceFromFullyQualifiedTypeName(referenceFullyQualifiedTypeName);
            var destinationTypeName = destinationFullyQualifiedTypeName.Split('.').Last();

            var usings = new HashSet<string>();
            var attributeLists = new List<string>();
            var propertyDeclarations = new List<string>();

            var referenceTypeSymbol = context.Compilation.GetTypeByMetadataName(referenceFullyQualifiedTypeName);

            // If the reference type was itself generated we need to include the generated members too
            generatedRecords.TryGetValue(referenceFullyQualifiedTypeName, out var referenceGeneratedTypeInfo);

            var allProperties = new List<(PropertyDeclarationSyntax PropertySyntax, INamedTypeSymbol? PropertyType)>();
            var allUsings = new List<UsingDirectiveSyntax>();

            void AddProperty(PropertyDeclarationSyntax property, INamedTypeSymbol? propertyType)
            {
                // If this type references another versioned type in the same namespace, add it to the list to ensure it's generated
                if (propertyType is not null)
                {
                    RecordReferenceIfTypeIsVersionedAndInSameNamespace(propertyType, referenceNamespace);
                }

                propertyDeclarations.Add(property.GetText().ToString());
                allProperties.Add((property, propertyType));
            }

            void AddUsing(UsingDirectiveSyntax usingSyntax)
            {
                var usingStatement = usingSyntax.GetText().ToString();

                if (usingSyntax.Name is QualifiedNameSyntax qualifiedNameSyntax)
                {
                    var fullName = qualifiedNameSyntax.Left + "." + qualifiedNameSyntax.Right;
                }

                usings.Add(usingStatement);
                allUsings.Add(usingSyntax);
            }

            foreach (var declaringSyntax in referenceTypeSymbol?.DeclaringSyntaxReferences ?? [])
            {
                var referenceSyntax = declaringSyntax.GetSyntax();
                var semanticModel = context.Compilation.GetSemanticModel(referenceSyntax.SyntaxTree);

                var rootSyntax = referenceSyntax;
                while (rootSyntax.Parent is not null)
                {
                    rootSyntax = rootSyntax.Parent;
                }

                foreach (var usingSyntax in rootSyntax.DescendantNodes().OfType<UsingDirectiveSyntax>())
                {
                    AddUsing(usingSyntax);
                }

                if (copyAttributes)
                {
                    foreach (var attributeList in referenceSyntax.ChildNodes().OfType<AttributeListSyntax>())
                    {
                        attributeLists.Add(attributeList.GetText().ToString());
                    }
                }

                foreach (var property in referenceSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>())
                {
                    if (excludeMembers.Contains(property.Identifier.ValueText))
                    {
                        continue;
                    }

                    var propertyType = semanticModel.GetTypeInfo(property.Type).Type as INamedTypeSymbol;

                    AddProperty(property, propertyType);
                }
            }

            if (referenceGeneratedTypeInfo is not null)
            {
                foreach (var @using in referenceGeneratedTypeInfo.Usings)
                {
                    AddUsing(@using);
                }

                foreach (var (property, propertyType) in referenceGeneratedTypeInfo.Properties)
                {
                    if (excludeMembers.Contains(property.Identifier.ValueText))
                    {
                        continue;
                    }

                    AddProperty(property, propertyType);
                }
            }

            var codeBuilder = new StringBuilder();

            codeBuilder.AppendLine("// <auto-generated/>");
            codeBuilder.AppendLine("#nullable enable");
            codeBuilder.AppendLine();

            foreach (var usingStatement in usings)
            {
                codeBuilder.Append(usingStatement);
            }
            if (usings.Count > 0)
            {
                codeBuilder.AppendLine();
            }

            codeBuilder.AppendLine($"namespace {destinationNamespace};");
            codeBuilder.AppendLine();

            if (copyAttributes)
            {
                foreach (var attributeList in attributeLists)
                {
                    codeBuilder.Append(attributeList.ToString());
                }
            }

            codeBuilder.AppendLine($"public partial record {destinationTypeName}");
            codeBuilder.AppendLine("{");
            foreach (var propertyDeclaration in propertyDeclarations)
            {
                codeBuilder.Append(propertyDeclaration);
            }
            codeBuilder.AppendLine("}");

            var relativeName = destinationNamespace.Substring(BaseNamespace.Length + 1);
            context.AddSource($"{relativeName}.{destinationTypeName}.g.cs", codeBuilder.ToString());

            var generatedTypeInfo = new GeneratedTypeInfo(destinationFullyQualifiedTypeName, referenceFullyQualifiedTypeName);
            generatedTypes.Add(generatedTypeInfo);

            var generatedRecordInfo = new GeneratedRecordInfo(allUsings.ToArray(), allProperties.ToArray());
            generatedRecords.Add(destinationFullyQualifiedTypeName, generatedRecordInfo);

            void RecordReferenceIfTypeIsVersionedAndInSameNamespace(INamedTypeSymbol symbol, string @namespace)
            {
                var fullPropertyType = symbol.ContainingNamespace + "." + symbol.Name;

                if (symbol.ContainingNamespace.ToString() == @namespace && IsVersionedType(fullPropertyType))
                {
                    if (symbol.TypeKind is not TypeKind.Enum && !(symbol.TypeKind is TypeKind.Class && symbol.IsRecord))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.UnsupportedTypeKind,
                                symbol.Locations.FirstOrDefault(),
                                messageArgs: symbol.TypeKind.ToString()));
                        return;
                    }

                    var referenceType = symbol.TypeKind is TypeKind.Enum ? EnumReferenceType : RecordReferenceType;

                    EnsureReference((ReplaceVersion(fullPropertyType, destinationVersion), fullPropertyType, referenceType));
                }

                if (symbol.IsGenericType)
                {
                    foreach (var genericArg in symbol.TypeArguments.OfType<INamedTypeSymbol>())
                    {
                        RecordReferenceIfTypeIsVersionedAndInSameNamespace(genericArg, @namespace);
                    }
                }
            }
        }

        void GenerateEnum(
            string destinationFullyQualifiedTypeName,
            string referenceFullyQualifiedTypeName)
        {
            var destinationVersion = GetTypeVersion(destinationFullyQualifiedTypeName);
            var destinationNamespace = GetNamespaceFromFullyQualifiedTypeName(destinationFullyQualifiedTypeName);
            var referenceVersion = GetTypeVersion(referenceFullyQualifiedTypeName);

            // If the reference enum was itself generated we won't find it in context.Compilation.
            // Instead, recursively look for its source type until we find a version that is in the Compilation.
            var sourceGeneratedFrom = generatedTypes.FirstOrDefault(g => g.DestinationType == referenceFullyQualifiedTypeName);
            if (sourceGeneratedFrom.ReferenceType is not null)
            {
                GenerateEnum(
                    destinationFullyQualifiedTypeName,
                    sourceGeneratedFrom.ReferenceType);
                return;
            }

            var referenceTypeSymbol = context.Compilation.GetTypeByMetadataName(referenceFullyQualifiedTypeName) ??
                throw new Exception($"Could not find '{referenceFullyQualifiedTypeName}.");
            var destinationTypeName = referenceTypeSymbol.Name;

            var declaringSyntax = referenceTypeSymbol.DeclaringSyntaxReferences.Single();

            var referenceSyntax = declaringSyntax.GetSyntax();
            var semanticModel = context.Compilation.GetSemanticModel(referenceSyntax.SyntaxTree);

            var enumMemberDeclarations = new List<string>();

            foreach (var member in referenceSyntax.DescendantNodes().OfType<EnumMemberDeclarationSyntax>())
            {
                enumMemberDeclarations.Add(member.GetText().ToString());
            }

            var codeBuilder = new StringBuilder();

            codeBuilder.AppendLine("// <auto-generated/>");
            codeBuilder.AppendLine("#nullable enable");
            codeBuilder.AppendLine();

            codeBuilder.AppendLine($"namespace {destinationNamespace};");
            codeBuilder.AppendLine();

            codeBuilder.AppendLine($"public enum {destinationTypeName}");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine(string.Join(",\n", enumMemberDeclarations));
            codeBuilder.AppendLine("}");

            var relativeName = destinationNamespace.Substring(BaseNamespace.Length + 1);
            context.AddSource($"{relativeName}.{destinationTypeName}.g.cs", codeBuilder.ToString());

            var generatedTypeInfo = new GeneratedTypeInfo(destinationFullyQualifiedTypeName, referenceFullyQualifiedTypeName);
            generatedTypes.Add(generatedTypeInfo);
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static bool IsVersionedType(string typeName) => typeName.StartsWith(BaseNamespace + ".V");

    private static bool IsVersionedNamespace(string @namespace) => @namespace.StartsWith(BaseNamespace + ".V");

    private static string ReplaceVersion(string typeName, string newVersion) =>
        typeName.Replace(GetTypeVersion(typeName), newVersion);

    private static string GetTypeVersion(string typeName)
    {
        var relativeNamespace = typeName.Substring(BaseNamespace.Length + 1);
        return relativeNamespace.Split('.')[0];
    }

    private static string GetNamespaceFromFullyQualifiedTypeName(string typeName) =>
        typeName.Substring(0, typeName.LastIndexOf('.'));

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<GenerateDtoInfo> Records { get; } = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is RecordDeclarationSyntax recordDeclarationSyntax)
            {
                var generateDtoAttrs = recordDeclarationSyntax.AttributeLists.SelectMany(al => al.Attributes)
                    .Where(a => a.Name is SimpleNameSyntax sns && sns.Identifier.Text is "GenerateVersionedDtoAttribute" or "GenerateVersionedDto");

                foreach (var attr in generateDtoAttrs)
                {
                    Records.Add(new GenerateDtoInfo(recordDeclarationSyntax, attr, syntaxNode.GetLocation()));
                }
            }
        }
    }

    private sealed class GenerateDtoInfo(RecordDeclarationSyntax record, AttributeSyntax attribute, Location location)
    {
        public RecordDeclarationSyntax Record { get; } = record;

        public AttributeSyntax Attribute { get; } = attribute;

        public Location Location { get; } = location;
    }

    private sealed class GeneratedRecordInfo(UsingDirectiveSyntax[] usings, (PropertyDeclarationSyntax Property, INamedTypeSymbol? PropertyType)[] properties)
    {
        public UsingDirectiveSyntax[] Usings { get; } = usings;

        public (PropertyDeclarationSyntax Property, INamedTypeSymbol? PropertyType)[] Properties { get; } = properties;
    }

    internal static class DiagnosticDescriptors
    {
        private const string Category = "Design";

        public static DiagnosticDescriptor UnsupportedTypeKind { get; } = new DiagnosticDescriptor(
            id: "TRSDTOGEN001",
            title: "Unsupported type kind",
            messageFormat: "{0} is not a supported type kind.",
            category: Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.NotConfigurable);

        public static DiagnosticDescriptor InvalidReferenceNamespace { get; } = new DiagnosticDescriptor(
            id: "TRSDTOGEN002",
            title: "Unsupported reference namespace",
            messageFormat: "{0} is not a versioned namespace.",
            category: Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.NotConfigurable);

        public static DiagnosticDescriptor InvalidNamespace { get; } = new DiagnosticDescriptor(
            id: "TRSDTOGEN003",
            title: "Unsupported namespace",
            messageFormat: "{0} is not a versioned namespace.",
            category: Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.NotConfigurable);
    }
}
