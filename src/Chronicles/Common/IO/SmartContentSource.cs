using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Chronicles.Common.IO;

/// <summary>
///     A "smart" implementation of <see cref="IContentSource"/>, which allows
///     for advanced features such as redirecting directories (as if they were
///     symbolic links).
/// </summary>
/// <remarks>
///     This wraps an existing <see cref="IContentSource"/> instance.
/// </remarks>
internal sealed class SmartContentSource : IContentSource {
    public IContentValidator ContentValidator {
        get => source.ContentValidator;
        set => source.ContentValidator = value;
    }

    public RejectedAssetCollection Rejections => source.Rejections;

    private readonly IContentSource source;
    private readonly Dictionary<string, string> redirects = new();

    public SmartContentSource(IContentSource source) {
        this.source = source;
    }

    public void AddDirectoryRedirect(string fromDir, string toDir) {
        redirects.Add(fromDir, toDir);
    }

    private string[] GetRewrittenPaths(string path) {
        var result = new List<string>();

        foreach (var (from, to) in redirects) {
            if (path.StartsWith(from))
                result.Add(path.Replace(from, to));
        }

        return result.ToArray();
    }

    IEnumerable<string> IContentSource.EnumerateAssets() {
        // TODO: Do we need to check if they exist? Hopefully not?
        // return source.EnumerateAssets().SelectMany(GetRewrittenPaths).Where(x => GetExtension(x) is not null);
        return source.EnumerateAssets().SelectMany(GetRewrittenPaths);
    }

    string? IContentSource.GetExtension(string assetName) {
        foreach (var path in GetRewrittenPaths(assetName)) {
            var ext = source.GetExtension(path);
            if (ext is not null)
                return ext;
        }

        return null;
    }

    Stream IContentSource.OpenStream(string fullAssetName) {
        foreach (var path in GetRewrittenPaths(fullAssetName)) {
            var stream = source.OpenStream(path);
            if (stream is not null)
                return stream;
        }

        throw new FileNotFoundException($"Could not find asset {fullAssetName}");
    }
}
