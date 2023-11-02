using Microsoft.CodeAnalysis;

namespace DarknessUnbound.CodeAssist.SourceGenerators;

[Generator]
public sealed class GlobalUsingsGenerator : ISourceGenerator {
    void ISourceGenerator.Initialize(GeneratorInitializationContext context) { }

    void ISourceGenerator.Execute(GeneratorExecutionContext context) {
        var assemblyName = context.Compilation.AssemblyName!;
        context.AddSource(
            "globals.g.cs",
            $"""
global using static {assemblyName}.Core.{assemblyName}Assets;
global using static {assemblyName}.Core.{assemblyName}Localization;
""".Trim()
        );
    }
}
