using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Realynx.CatTail.Generators {
    [Generator]
    public class BuildScriptRegistrations : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {

            // Grab all class symbols with a base list
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax cds && cds.BaseList != null,
                    transform: static (ctx, _) => (INamedTypeSymbol?)ctx.SemanticModel.GetDeclaredSymbol(ctx.Node)
                )
                .Where(symbol => symbol is not null);

            // Get the IBuildScript symbol from the compilation
            var buildScriptInterface = context.CompilationProvider
                .Select((compilation, _) =>
                    compilation.GetTypeByMetadataName("Realynx.CatTail.Attributes.IBuildScript"));

            // Pair up class declarations with the IBuildScript symbol
            var buildScripts = classDeclarations.Combine(buildScriptInterface)
                .Where(pair =>
                    pair.Left is not null &&
                    pair.Right is not null &&
                    pair.Left!.AllInterfaces.Contains(pair.Right, SymbolEqualityComparer.Default))
                .Select((pair, _) => pair.Left!);

            // Generate source
            context.RegisterSourceOutput(buildScripts.Collect(), (spc, scripts) => {
                var sb = new StringBuilder();
                sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
                sb.AppendLine("namespace Realynx.CatTail.Generators.Generated {");
                sb.AppendLine("  public static class BuildScriptServiceCollectionExtensions {");
                sb.AppendLine("    public static IServiceCollection AddBuildScripts(this IServiceCollection services) {");

                foreach (var type in scripts.Distinct(SymbolEqualityComparer.Default)) {
                    sb.AppendLine($"      services.AddSingleton<IBuildScript, {type.ToDisplayString()}>();");
                }

                sb.AppendLine("      return services;");
                sb.AppendLine("    }");
                sb.AppendLine("  }");
                sb.AppendLine("}");

                spc.AddSource("BuildScriptRegistration.g.cs", sb.ToString());
            });
        }
    }
}
