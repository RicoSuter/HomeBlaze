using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;

namespace Namotion.Proxy.Generator;

[Generator]
public class ProxyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classWithAttributeProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0 && cds.AttributeLists.Any(a => a.ToString() == "[GenerateProxy]"),
                transform: (ctx, ct) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                    var model = ctx.SemanticModel;
                    var classSymbol = model.GetDeclaredSymbol(classDeclaration, ct);
                    return new
                    {
                        Model = model,
                        ClassNode = (ClassDeclarationSyntax)ctx.Node,
                        Properties = classDeclaration.Members
                            .OfType<PropertyDeclarationSyntax>()
                            .Where(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) || p.AttributeLists.Any(a => a.ToString() == "[Derived]"))
                            .Select(p => new
                            {
                                Property = p,
                                Type = model.GetTypeInfo(p.Type, ct),
                                IsPartial = p.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
                                IsDerived = p.AttributeLists.Any(a => a.ToString() == "[Derived]"),
                                IsRequired = p.Modifiers.Any(m => m.IsKind(SyntaxKind.RequiredKeyword)),
                                HasGetter = p.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) == true ||
                                            p.ExpressionBody.IsKind(SyntaxKind.ArrowExpressionClause),
                                HasSetter = p.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) == true,
                                HasInit = p.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.InitAccessorDeclaration)) == true
                            })
                            .ToArray()
                    };
                })
            .Where(static m => m is not null)!;

        var compilationAndClasses = context.CompilationProvider.Combine(classWithAttributeProvider.Collect());

        context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            var (compilation, classes) = source;
            foreach (var cls in classes.GroupBy(c => c.ClassNode.Identifier.ValueText))
            {
                var fileName = $"{cls.First().ClassNode.Identifier.Value}.g.cs";
                try
                {
                    var semanticModel = cls.First().Model;
                    var baseClassName = cls.First().ClassNode.Identifier.ValueText;
                    var newClassName = baseClassName; //.Replace("Base", string.Empty);

                    var namespaceName = (cls.First().ClassNode.Parent as NamespaceDeclarationSyntax)?.Name.ToString() ?? "YourDefaultNamespace";

                    var generatedCode = $@"
using Namotion.Proxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.Json.Serialization;

#pragma warning disable CS8669

namespace {namespaceName} 
{{
    [System.CodeDom.Compiler.GeneratedCode(""Namotion.Proxy"", ""1.0.0"")]
    public partial class {baseClassName} : IProxy
    {{
        private IProxyContext? _context;
        private ConcurrentDictionary<string, object?> _data = new ConcurrentDictionary<string, object?>();

        [JsonIgnore]
        IProxyContext? IProxy.Context
        {{
            get => _context;
            set => _context = value;
        }}

        [JsonIgnore]
        ConcurrentDictionary<string, object?> IProxy.Data => _data;

        [JsonIgnore]
        IReadOnlyDictionary<string, ProxyPropertyInfo> IProxy.Properties => _properties;

        private static IReadOnlyDictionary<string, ProxyPropertyInfo> _properties = new Dictionary<string, ProxyPropertyInfo>
        {{";
                    foreach (var property in cls.SelectMany(c => c.Properties))
                    {
                        //var fullyQualifiedName = property.Type.Type!.ToDisplayString(symbolDisplayFormat);
                        var fullyQualifiedName = property.Type.Type!.ToString();
                        var propertyName = property.Property.Identifier.Value;

                        generatedCode +=
    $@"
            {{
                ""{propertyName}"",       
                new ProxyPropertyInfo(nameof({propertyName}), typeof({baseClassName}).GetProperty(nameof({propertyName})).PropertyType!, typeof({baseClassName}).GetProperty(nameof({propertyName})).GetCustomAttributes().ToArray()!, {(property.HasGetter ? ($"(o) => (({newClassName})o).{propertyName}") : "null")}, {(property.HasSetter ? ($"(o, v) => (({newClassName})o).{propertyName} = ({fullyQualifiedName})v") : "null")})
            }},";
                    }

                    generatedCode +=
    $@"
        }};
";

                    var firstConstructor = cls.SelectMany(c => c.ClassNode.Members)
                        .FirstOrDefault(m => m.IsKind(SyntaxKind.ConstructorDeclaration))
                        as ConstructorDeclarationSyntax;

                    if (firstConstructor == null)
                    {
                        generatedCode +=
    $@"
        public {newClassName}()
        {{
        }}
";
                    }

                    if (firstConstructor == null ||
                        firstConstructor.ParameterList.Parameters.Count == 0)
                    {
                        generatedCode +=
    $@"
        public {newClassName}(IProxyContext context) : this()
        {{
            this.SetContext(context);
        }}
";
                    }

                    foreach (var property in cls.SelectMany(c => c.Properties).Where(p => p.IsPartial))
                    {
                        //var fullyQualifiedName = property.Type.Type!.ToDisplayString(symbolDisplayFormat);
                        var fullyQualifiedName = property.Type.Type!.ToString();
                        var propertyName = property.Property.Identifier.Value;

                        generatedCode +=
    $@"
        private {fullyQualifiedName} _{propertyName};
        public partial {(property.IsRequired ? "required " : "")}{fullyQualifiedName} {propertyName}
        {{";
                        if (property.HasGetter)
                        {
                            var modifiers = string.Join(" ", property.Property.AccessorList?
                                .Accessors.First(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)).Modifiers.Select(m => m.Value) ?? []);

                            generatedCode +=
    $@"
            {modifiers} get => GetProperty<{fullyQualifiedName}>(nameof({propertyName}), () => _{propertyName});";

                        }

                        if (property.HasSetter || property.HasInit)
                        {
                            var accessor = property.Property.AccessorList?
                                .Accessors.Single(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || a.IsKind(SyntaxKind.InitAccessorDeclaration)) 
                                ?? throw new InvalidOperationException("Accessor not found.");

                            var accessorText = accessor.IsKind(SyntaxKind.InitAccessorDeclaration) ? "init" : "set";
                            var modifiers = string.Join(" ", accessor.Modifiers.Select(m => m.Value) ?? []);

                            generatedCode +=
    $@"
            {modifiers} {accessorText} => SetProperty(nameof({propertyName}), value, () => _{propertyName}, v => _{propertyName} = ({fullyQualifiedName})v!);";
                        }

                        generatedCode +=
    $@"
        }}
";
                    }

                    generatedCode +=
    $@"
        private T GetProperty<T>(string propertyName, Func<object?> readValue)
        {{
            return _context is not null ? (T?)_context.GetProperty(this, propertyName, readValue)! : (T?)readValue()!;
        }}

        private void SetProperty<T>(string propertyName, T? newValue, Func<object?> readValue, Action<object?> setValue)
        {{
            if (_context is null)
            {{
                setValue(newValue);
            }}
            else
            {{
                _context.SetProperty(this, propertyName, newValue, readValue, setValue);
            }}
        }}
    }}
}}
";
                    spc.AddSource(fileName, SourceText.From(generatedCode, Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    spc.AddSource(fileName, SourceText.From($"/* {ex} */", Encoding.UTF8));
                }
            }
        });
    }

    private string? GetFullTypeName(TypeSyntax? type, SemanticModel semanticModel)
    {
        if (type == null)
            return null;

        var typeInfo = semanticModel.GetTypeInfo(type);
        var symbol = typeInfo.Type;
        if (symbol != null)
        {
            return GetFullTypeName(symbol);
        }

        throw new InvalidOperationException("Unable to resolve type symbol.");
    }

    static string? GetFullTypeName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null)
            return null;

        if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
        {
            var genericArguments = string.Join(", ", namedTypeSymbol.TypeArguments.Select(GetFullTypeName));
            return $"{namedTypeSymbol.ContainingNamespace}.{namedTypeSymbol.Name}<{genericArguments}>";
        }

        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
