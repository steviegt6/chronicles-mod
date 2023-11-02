using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DarknessUnbound.CodeAssist.SourceGenerators;

[Generator]
public sealed class AssetReferenceGenerator : ISourceGenerator {
    private record struct AssetFile(string Name, string Path, IAssetReference Reference);

    private sealed class DirectoryNode {
        public string Name { get; }

        public Dictionary<string, DirectoryNode> Children { get; }

        public List<AssetFile> Files { get; }

        public DirectoryNode(string name) {
            Name = Path.GetFileNameWithoutExtension(name);
            Children = new Dictionary<string, DirectoryNode>();
            Files = new List<AssetFile>();
        }

        public void AddFile(string filePath, Dictionary<string, IAssetReference> referencesByExtension) {
            var pathNodes = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var currentNode = this;

            for (var i = 0; i < pathNodes.Length; i++) {
                if (i == pathNodes.Length - 1)
                    break;
                
                var pathNode = pathNodes[i];

                if (!currentNode.Children.TryGetValue(pathNode, out var nextNode)) {
                    nextNode = new DirectoryNode(pathNode);
                    currentNode.Children.Add(pathNode, nextNode);
                }

                currentNode = nextNode;
            }

            var ext = Path.GetExtension(filePath);
            if (referencesByExtension.TryGetValue(ext, out var reference))
                currentNode.Files.Add(new AssetFile(Path.GetFileNameWithoutExtension(filePath), filePath, reference));
        }
    }

    private interface IAssetReference {
        string QualifiedType { get; }

        string Extension { get; }
    }

    private abstract class Texture2DAssetReference : IAssetReference {
        public string QualifiedType => "global::Microsoft.Xna.Framework.Graphics.Texture2D";

        public abstract string Extension { get; }
    }

    private sealed class PngAssetReference : Texture2DAssetReference {
        public override string Extension => ".png";
    }

    private readonly List<IAssetReference> assetReferences = new() {
        new PngAssetReference(),
    };

    void ISourceGenerator.Initialize(GeneratorInitializationContext context) { }

    void ISourceGenerator.Execute(GeneratorExecutionContext context) {
        var referencesByExtension = assetReferences.ToDictionary(x => x.Extension, x => x);
        var files = context.AdditionalFiles.Where(x => referencesByExtension.ContainsKey(Path.GetExtension(x.Path)));

        context.AddSource("assets.g.cs", GenerateAssetReferences(referencesByExtension, files.ToList(), context.Compilation.AssemblyName!));
    }

    private string GenerateAssetReferences(Dictionary<string, IAssetReference> referencesByExtension, List<AdditionalText> files, string assemblyName) {
        var sb = new StringBuilder();

        foreach (var file in files)
            sb.AppendLine($"#error {file.Path}");
        sb.AppendLine("#error test");

        var root = new DirectoryNode("Root");

        foreach (var file in files)
            root.AddFile(file.Path.Substring(file.Path.IndexOf(assemblyName, StringComparison.InvariantCulture)), referencesByExtension);

        root = root.Children.Single().Value;

        sb.AppendLine("using System;");
        sb.AppendLine("using ReLogic.Content;");
        sb.AppendLine("using Terraria.ModLoader;");
        sb.AppendLine();
        sb.AppendLine($"namespace {assemblyName}.Core;");
        sb.AppendLine();
        sb.AppendLine($"internal static class {assemblyName}Assets {{");

        sb.Append(GenerateTextFromPathNode(root));

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateTextFromPathNode(DirectoryNode pathNode, int depth = 0) {
        var sb = new StringBuilder();

        // Special logic if this is the root.
        if (depth == 0) {
            foreach (var file in pathNode.Files) {
                sb.AppendLine($"    private static readonly Lazy<Asset<{file.Reference.QualifiedType}>> lazy_{file.Name} = new(() => ModContent.Request<{file.Reference.QualifiedType}>(\"{file.Path.Replace('\\', '/')}\"));");
                sb.AppendLine(
                    $"    private static readonly Lazy<Asset<{file.Reference.QualifiedType}>> lazy_{file.Name}_immediate = new(() => ModContent.Request<{file.Reference.QualifiedType}>(\"{file.Path.Replace('\\', '/')}\", AssetRequestMode.ImmediateLoad));");
                sb.AppendLine($"    public static Asset<{file.Reference.QualifiedType}> {file.Name} => lazy_{file.Name}.Value;");
                sb.AppendLine($"    public static Asset<{file.Reference.QualifiedType}> {file.Name}_Immediate => lazy_{file.Name}_immediate.Value;");
                sb.AppendLine($"    public const string {file.Name}_Name = \"{file.Name}\";");
            }

            foreach (var node in pathNode.Children.Values)
                sb.AppendLine(GenerateTextFromPathNode(node, depth + 1));

            return sb.ToString();
        }

        var indent = new string(' ', depth * 4);

        sb.AppendLine($"{indent}public static class {pathNode.Name} {{");

        foreach (var file in pathNode.Files) {
            sb.AppendLine($"{indent}    private static readonly Lazy<Asset<{file.Reference.QualifiedType}>> lazy_{file.Name} = new(() => ModContent.Request<{file.Reference.QualifiedType}>(\"{file.Path.Replace('\\', '/')}\"));");
            sb.AppendLine(
                $"{indent}    private static readonly Lazy<Asset<{file.Reference.QualifiedType}>> lazy_{file.Name}_immediate = new(() => ModContent.Request<{file.Reference.QualifiedType}>(\"{file.Path.Replace('\\', '/')}\", AssetRequestMode.ImmediateMode));");
            sb.AppendLine($"{indent}    public static Asset<{file.Reference.QualifiedType}> {file.Name} => lazy_{file.Name}.Value;");
            sb.AppendLine($"{indent}    public static Asset<{file.Reference.QualifiedType}> {file.Name}_Immediate => lazy_{file.Name}_immediate.Value;");
            sb.AppendLine($"{indent}    public const string {file.Name}_Name = \"{file.Name}\";");
        }

        foreach (var node in pathNode.Children.Values)
            sb.Append(GenerateTextFromPathNode(node, depth + 1));

        sb.AppendLine($"{indent}}}");

        return sb.ToString();
    }
}
