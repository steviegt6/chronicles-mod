using System;
using Microsoft.CodeAnalysis;

namespace Chronicles.AssetGenerator;

/// <summary>
///     Represents an asset.
/// </summary>
public readonly record struct Asset {
    /// <summary>
    ///     The path to the asset, relative to the root directory.
    /// </summary>
    public string Path { get; }

    /// <summary>
    ///     The name of the asset. This is the name of the file without the
    ///     extension.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     An ordered array of the segments (directories) in the path.
    /// </summary>
    public string[] Segments { get; }

    /// <summary>
    ///     The type of the asset.
    /// </summary>
    public AssetType Type { get; }

    public Asset(string path) {
        Path = GetAssetPath(path);
        Name = GetAssetName(path);
        Segments = GetAssetSegments(path);
        Type = GetAssetType(path);
    }

    public string ToString(string modName, int tabCount) {
        var tabs = new string(' ', tabCount * 4);

        if (Type == AssetType.Unknown)
            return $"{tabs}#error Unknown asset type: {Path}";

        var memberType = Type switch {
            AssetType.Image => "Texture2D",
            AssetType.Unknown => "#error",
            _ => throw new NotImplementedException(),
        };
        var path = System.IO.Path.Combine(modName, "Assets", Path);
        path = path.Replace('\\', '/');

        return $"{tabs}private static Asset<{memberType}> backing_{Name};\n"
             + $"{tabs}public static Asset<{memberType}> {Name} => backing_{Name} ??= ModContent.Request<{memberType}>(\"{path}\", AssetRequestMode.ImmediateLoad);";
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
