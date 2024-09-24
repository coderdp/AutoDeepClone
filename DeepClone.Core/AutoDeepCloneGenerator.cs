using AutoDeepClone.Core.Modes;
using AutoDeepClone.Core.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AutoDeepClone.Core;

[Generator(LanguageNames.CSharp)]
public partial class AutoDeepCloneGenerator : IIncrementalGenerator
{
    private const string GenerateDeepCloneAttributeFullName = "AutoDeepClone.Core.DeepCloneAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        //System.Diagnostics.Debugger.Launch();
#endif

        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource(
                "DeepCloneAttribute.g.cs", SourceText.From(Consts.GenerateDeepCloneAttribute, Encoding.UTF8));
        });

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: GenerateDeepCloneAttributeFullName,
            predicate: (node, token) => IsSyntaxTargetForGeneration(node),
            transform: TransformToCommand);

        context.RegisterSourceOutput(pipeline, GenerateCore);
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        var classDeclarationFlag = node.IsKind(SyntaxKind.ClassDeclaration);
        return classDeclarationFlag;
    }

    static GenerateComparerCommand TransformToCommand(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken token)
    {
        var classSymbol = syntaxContext.TargetSymbol as INamedTypeSymbol;
        var command = TransformToCommand(classSymbol, token);

        return command;
    }

    internal static GenerateComparerCommand TransformToCommand(INamedTypeSymbol classSymbol, CancellationToken token)
    {
        var classDescription = GetClassDescription(classSymbol);

        var command = new GenerateComparerCommand
        {
            TargetClassDescription = classDescription,
        };

        command.UsingNamespaces.AddIfNotContains("System");
        command.UsingNamespaces.AddIfNotContains("System.Linq");
        command.UsingNamespaces.AddIfNotContains("System.Collections.Generic");

        var propertySymbols = GetProperties(classSymbol);

        var propertyDescriptions = new List<GeneratorPropertyDescription>();
        foreach (var property in propertySymbols)
        {
            if (property is not IPropertySymbol propertySymbol)
            {
                continue;
            }

            if (propertySymbol.IsReadOnly)
            {
                continue;
            }

            var propertyName = propertySymbol.Name;
            if (propertyDescriptions.Any(o => o.Name == propertyName))
            {
                continue;
            }

            var hasGetSetMethod = propertySymbol.GetMethod != null && propertySymbol.SetMethod != null;
            if (!hasGetSetMethod)
            {
                continue;
            }

            var propertyDescription = GetPropertyDescription(propertySymbol);
            propertyDescriptions.Add(propertyDescription);
        }

        command.Properties = new(propertyDescriptions);

        return command;
    }

    internal static GeneratorClassDescription GetClassDescription(ITypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol == null || namedTypeSymbol.ContainingNamespace == null)
        {
            return null;
        }

        var namespaceName = namedTypeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        var containingClassDescription = GetClassDescription(namedTypeSymbol.ContainingType);
        var classDescription = new GeneratorClassDescription
        {
            Accessibility = namedTypeSymbol.GetAccessibilityCode(),
            ClassName = namedTypeSymbol.Name,
            Namespace = namespaceName,
            FullClassName = GetFullTypeName(namedTypeSymbol),
            ContainingClass = containingClassDescription,
        };

        return classDescription;
    }

    static IList<ISymbol> GetProperties(INamedTypeSymbol classSymbol)
    {
        var result = new List<ISymbol>();

        void GetPropertiesForClass(INamedTypeSymbol symbol)
        {
            if (symbol == null)
            {
                return;
            }

            var propertySymbols = symbol.GetMembers().Where(o => (o.Kind == SymbolKind.Property && o.DeclaredAccessibility == Accessibility.Public));
            result.AddRange(propertySymbols);

            GetPropertiesForClass(symbol.BaseType);
        }

        GetPropertiesForClass(classSymbol);

        return result;
    }

    static GeneratorPropertyDescription GetPropertyDescription(IPropertySymbol propertySymbol)
    {
        var propertyDescription = new GeneratorPropertyDescription
        {
            Name = propertySymbol.Name,
            FullTypeName = GetFullTypeName(propertySymbol.Type),
            ValueType = GeneratorPropertyValueType.Unknown,
            Ignore = false,
        };

        var propertyTypeSymbol = propertySymbol.Type.TryGetUnderlyingNullableTypeOrSelf();

        void TryFillWithKnownBasicType()
        {
            if (propertyTypeSymbol.IsKnownBasicCSharpType())
            {
                propertyDescription.ValueType = GeneratorPropertyValueType.Basic;
                propertyDescription.MapType = GeneratorPropertyValueMapType.Simple;
            }
        }

        void TryFillWithListType()
        {
            ITypeSymbol elementTypeSymbol = null;

            if (propertyTypeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                elementTypeSymbol = arrayTypeSymbol.ElementType;
                if (elementTypeSymbol != null)
                {
                    var elementUnderlyingTypeSymbol = elementTypeSymbol.TryGetUnderlyingNullableTypeOrSelf();

                    propertyDescription.ValueType = GeneratorPropertyValueType.Array;
                    if (elementUnderlyingTypeSymbol.IsKnownBasicCSharpType())
                    {
                        propertyDescription.MapType = GeneratorPropertyValueMapType.Select;
                    }
                    else
                    {
                        propertyDescription.MapType = GeneratorPropertyValueMapType.SelectDeepClone;
                    }
                }
            }
            else if (propertyTypeSymbol is INamedTypeSymbol classSymbol && IsKnownGenericListType(classSymbol))
            {
                elementTypeSymbol = classSymbol.TypeArguments[0];
                if (elementTypeSymbol != null)
                {
                    var elementUnderlyingTypeSymbol = elementTypeSymbol.TryGetUnderlyingNullableTypeOrSelf();
                    propertyDescription.ValueType = GeneratorPropertyValueType.List;
                    if (elementUnderlyingTypeSymbol.IsKnownBasicCSharpType())
                    {
                        propertyDescription.MapType = GeneratorPropertyValueMapType.Select;
                    }
                    else
                    {
                        propertyDescription.MapType = GeneratorPropertyValueMapType.SelectDeepClone;
                    }
                }
            }
        }

        void TryFillWithDictionaryType()
        {
            ITypeSymbol pairValueTypeSymbol = null;

            if (propertyTypeSymbol is INamedTypeSymbol classSymbol && IsKnownGenericDictionaryType(classSymbol))
            {
                pairValueTypeSymbol = classSymbol.TypeArguments[1];
            }

            if (pairValueTypeSymbol != null)
            {
                var pairValueUnderlyingTypeSymbol = pairValueTypeSymbol.TryGetUnderlyingNullableTypeOrSelf();
                propertyDescription.ValueType = GeneratorPropertyValueType.Dictionary;

                if (pairValueUnderlyingTypeSymbol.IsKnownBasicCSharpType())
                {
                    propertyDescription.MapType = GeneratorPropertyValueMapType.Simple;
                }
                else
                {
                    propertyDescription.MapType = GeneratorPropertyValueMapType.DeepClone;
                }
            }
        }

        void TryFillWithKnownNotSupportedType()
        {
            if (propertyTypeSymbol is INamedTypeSymbol classSymbol
                && (IsFromSystemRootNameSpace(classSymbol) || IsFromMicrosoftRootNameSpace(classSymbol)))
            {
                propertyDescription.ValueType = GeneratorPropertyValueType.NotSupported;
            }
        }

        void TryFillWithDefaultType()
        {
            propertyDescription.ValueType = GeneratorPropertyValueType.Object;
            propertyDescription.MapType = GeneratorPropertyValueMapType.DeepClone;
        }

        propertyDescription.FillWith(
            TryFillWithKnownBasicType,
            TryFillWithListType,
            TryFillWithDictionaryType,
            TryFillWithKnownNotSupportedType,
            TryFillWithDefaultType);

        return propertyDescription;
    }

    static bool IsDeepCloneAttribute(AttributeData x)
    {
        var r = x.AttributeClass is
        {
            Name: "ObjectPropertyComparerAttribute", ContainingNamespace:
            {
                Name: "ObjectComparison", ContainingNamespace:
                {
                    Name: "Common", ContainingNamespace:
                    {
                        Name: "Hc",
                    }
                }
            }
        };
        return r;
    }

    private static bool IsKnownGenericListType(INamedTypeSymbol classSymbol)
    {
        if (!classSymbol.IsGenericType)
        {
            return false;
        }

        if (classSymbol.TypeArguments.Length != 1)
        {
            return false;
        }

        var isKnowListType = (classSymbol.Name == "List" || classSymbol.Name == "IList")
                             && classSymbol.ContainingNamespace is
                             {
                                 Name: "Generic", ContainingNamespace:
                                 {
                                     Name: "Collections", ContainingNamespace:
                                     {
                                         Name: "System",
                                     }
                                 }
                             };

        return isKnowListType;
    }

    private static bool IsKnownGenericDictionaryType(INamedTypeSymbol classSymbol)
    {
        if (!classSymbol.IsGenericType)
        {
            return false;
        }

        if (classSymbol.TypeArguments.Length != 2)
        {
            return false;
        }

        var className = classSymbol.Name;
        var isKnowSystemCollectionsGenericDictionaryType = (className == "IDictionary"
                                                            || className == "Dictionary"
                                                            || className == "IReadOnlyDictionary")
                                                           && classSymbol.ContainingNamespace is
                                                           {
                                                               Name: "Generic", ContainingNamespace:
                                                               {
                                                                   Name: "Collections", ContainingNamespace:
                                                                   {
                                                                       Name: "System",
                                                                   }
                                                               }
                                                           };

        if (isKnowSystemCollectionsGenericDictionaryType)
        {
            return true;
        }

        var isKnownReadOnlyDictionaryType = className == "ReadOnlyDictionary"
                                            && classSymbol.ContainingNamespace is
                                            {
                                                Name: "ObjectModel", ContainingNamespace:
                                                {
                                                    Name: "Collections", ContainingNamespace:
                                                    {
                                                        Name: "System",
                                                    }
                                                }
                                            };
        if (isKnownReadOnlyDictionaryType)
        {
            return true;
        }

        return false;
    }

    private static bool IsFromSystemRootNameSpace(INamedTypeSymbol namedTypeSymbol)
    => IsFromRootNameSpace(namedTypeSymbol?.ContainingNamespace, "System");

    private static bool IsFromMicrosoftRootNameSpace(INamedTypeSymbol namedTypeSymbol)
        => IsFromRootNameSpace(namedTypeSymbol?.ContainingNamespace, "Microsoft");

    private static bool IsFromRootNameSpace(INamespaceSymbol namespaceSymbol, string rootNamespaceName)
    {
        if (namespaceSymbol == null)
        {
            return false;
        }

        var containingNamespace = namespaceSymbol.ContainingNamespace;
        if (containingNamespace != null && !string.IsNullOrWhiteSpace(containingNamespace.Name))
        {
            return IsFromRootNameSpace(containingNamespace, rootNamespaceName);
        }

        var r = namespaceSymbol.Name == rootNamespaceName;

        return r;
    }

    static string GetFullTypeName(ITypeSymbol type) => type.ToDisplayString(NullableFlowState.MaybeNull, SymbolDisplayFormat.FullyQualifiedFormat);
}