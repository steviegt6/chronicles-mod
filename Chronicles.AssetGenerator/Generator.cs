using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Chronicles.AssetGenerator;

public enum AssetType {
    Image,
    Unknown,
}

public readonly record struct Asset {
    public string Path { get; }

    public string Name { get; }
    
    public string[] Segments { get; }
    
    public AssetType Type { get; }

    public Asset(string path) {
        Path = GetAssetPath(path);
        Name = GetAssetName(path);
        Segments = GetAssetSegments(path);
        Type = GetAssetType(path);
    }

    public string GetMemberType() {
        return Type switch {
            AssetType.Image => "Texture2D",
            AssetType.Unknown => "#error",
            _ => throw new NotImplementedException(),
        };
    }

    public string MakeField() {
        return $"private static Asset<{GetMemberType()}>? _{Name};";
    }
    
    public string MakeProperty() {
        var path = System.IO.Path.Combine("Chronicles", "Assets", Path);
        path = path.Replace('\\', '/');
        return $"public static Asset<{GetMemberType()}> {Name} => _{Name} ??= ModContent.Request<{GetMemberType()}>(\"{path}\", AssetRequestMode.ImmediateLoad);";
    }

    private static string GetAssetPath(string path) {
        var dir = System.IO.Path.GetDirectoryName(path)!;
        var name = System.IO.Path.GetFileNameWithoutExtension(path);
        return System.IO.Path.Combine(dir, name);
    }
    
    private static string GetAssetName(string path) {
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }
    
    private static string[] GetAssetSegments(string path) {
        var dir = System.IO.Path.GetDirectoryName(path)!;
        return dir.Split(System.IO.Path.DirectorySeparatorChar);
    }

    private static AssetType GetAssetType(string path) {
        return System.IO.Path.GetExtension(path) switch {
            ".png" => AssetType.Image,
            _ => AssetType.Unknown,
        };
    }

    public static Asset From(AdditionalText file, string rootDir) {
        return new Asset(file.Path.Substring(rootDir.Length));
    }
}

[Generator]
public sealed class Generator : ISourceGenerator {
    void ISourceGenerator.Initialize(GeneratorInitializationContext context) { }

    void ISourceGenerator.Execute(GeneratorExecutionContext context) {
        context.AddSource("assets.g.cs", MakeAssets(context));
    }

    private static string MakeAssets(GeneratorExecutionContext ctx) {
        var sb = new StringBuilder();
        var rootDir = Path.Combine(AssumeRootDir(ctx), "Assets") + Path.DirectorySeparatorChar;
        var assets = ctx.AdditionalFiles.Select(x => Asset.From(x, rootDir)).ToList();

        sb.AppendLine("#nullable enable");
        sb.AppendLine("");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using Microsoft.Xna.Framework.Graphics;");
        sb.AppendLine("using ReLogic.Content;");
        sb.AppendLine("using Terraria.ModLoader;");
        sb.AppendLine();
        sb.AppendLine("namespace Chronicles;");
        sb.AppendLine();
        sb.AppendLine("[CompilerGenerated]");
        sb.AppendLine("public static class ChroniclesAssets {");

        foreach (var asset in assets) {
            var indentLevel = 1;
            
            foreach (var segment in asset.Segments) {
                sb.AppendLine($"{new string(' ', indentLevel * 4)}[CompilerGenerated]");
                sb.AppendLine($"{new string(' ', indentLevel * 4)}public static partial class {segment} {{");
                indentLevel++;
            }

            sb.AppendLine(new string(' ', indentLevel * 4) + asset.MakeField());
            sb.AppendLine(new string(' ', indentLevel * 4) + asset.MakeProperty());
            
            while (indentLevel > 1) {
                indentLevel--;
                sb.AppendLine($"{new string(' ', indentLevel * 4)}}}");
            }
        }

        sb.AppendLine("}");

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
