using System.Collections.Generic;
using System.Text;

namespace Chronicles.AssetGenerator;

public sealed class AssetClass {
    public string Name { get; }

    public List<Asset> Assets { get; } = new();

    public List<AssetClass> Classes { get; } = new();

    public AssetClass(string name) {
        Name = name;
    }

    public string ToString(string modName, int tabSize) {
        var tabs = new string(' ', tabSize * 4);
        var sb = new StringBuilder();

        sb.AppendLine($"{tabs}public static class {Name} {{");

        for (var i = 0; i < Classes.Count; i++) {
            var assetClass = Classes[i];
            sb.Append(assetClass.ToString(modName, tabSize + 1));
            
            if (i < Classes.Count - 1)
                sb.AppendLine();
        }

        for (var i = 0; i < Assets.Count; i++) {
            var asset = Assets[i];
            sb.Append(asset.ToString(modName, tabSize + 1));
            
            if (i < Assets.Count - 1)
                sb.AppendLine();
        }

        sb.Append($"\n{tabs}}}");

        return sb.ToString();
    }
}
