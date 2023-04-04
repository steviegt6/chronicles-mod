using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Chronicles.AssetGenerator;

/// <summary>
///     Generates <c>assets.g.cs</c> from the <c>Assets</c> directory of a mod.
/// </summary>
[Generator]
public sealed class AssetSourceGenerator : ISourceGenerator {
    void ISourceGenerator.Initialize(GeneratorInitializationContext context) { }

    void ISourceGenerator.Execute(GeneratorExecutionContext context) {
        context.AddSource("assets.g.cs", MakeAssets(context));
    }

    private static string MakeAssets(GeneratorExecutionContext ctx) {
        var fileText = @"
// This file is autogenerated from the following directory:
// {0}

#nullable enable

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace {1};

[CompilerGenerated]
public static class {1}Assets {{
{2}
}}
".Trim();

        var rootDir = Path.Combine(AssumeRootDir(ctx), "Assets") + Path.DirectorySeparatorChar;
        var projectName = ctx.Compilation.AssemblyName!;
        var assets = ctx.AdditionalFiles.Where(x => x.Path.StartsWith(rootDir)).Select(x => Asset.From(x, rootDir)).ToList();
        var assetClasses = MakeAssetClasses(assets);
        var classes = GenerateClasses(assetClasses, projectName);

        return string.Format(fileText, rootDir, projectName, classes);
    }

    private static List<AssetClass> MakeAssetClasses(List<Asset> assets) {
        var classMap = new Dictionary<string, AssetClass>();

        foreach (var asset in assets) {
            var segments = asset.Segments;
            if (!classMap.TryGetValue(segments[0], out var currentClass))
                classMap[segments[0]] = currentClass = new AssetClass(segments[0]);

            for (var i = 1; i < segments.Length; i++) {
                var segment = segments[i];
                var childClass = currentClass.Classes.Find(x => x.Name == segment);

                if (childClass is null) {
                    childClass = new AssetClass(segment);
                    currentClass.Classes.Add(childClass);
                }

                currentClass = childClass;
            }

            currentClass.Assets.Add(asset);
        }

        return classMap.Values.ToList();
    }

    private static string GenerateClasses(List<AssetClass> assetClasses, string modName) {
        var sb = new StringBuilder();

        for (var i = 0; i < assetClasses.Count; i++) {
            var assetClass = assetClasses[i];
            sb.Append(assetClass.ToString(modName, 1));

            if (i < assetClasses.Count - 1) {
                sb.AppendLine();
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    // https://stackoverflow.com/a/65093866
    // Solution lies within the replies to this answer.
    private static string AssumeRootDir(GeneratorExecutionContext ctx) {
        var opts = ctx.AnalyzerConfigOptions.GlobalOptions;
        opts.TryGetValue("build_property.projectdir", out var projectDirectory);
        return projectDirectory!;
    }
}