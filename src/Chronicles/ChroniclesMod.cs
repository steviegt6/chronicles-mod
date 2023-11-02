using Chronicles.Common.IO;
using Chronicles.Core;
using JetBrains.Annotations;
using ReLogic.Content.Sources;
using Terraria.ModLoader;

namespace Chronicles;

/// <summary>
///     The central <see cref="Mod"/> class for Chronicles. Manages very little
///     information by itself, but is employed heavily by the tModLoader API.
/// </summary>
/// <remarks>
///     This class should be responsible for very little. Information and logic
///     should be delegated to various <see cref="ModSystem"/>s instead (or
///     elsewhere when appropriate).
/// </remarks>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public sealed class ChroniclesMod : Mod {
    public override IContentSource CreateDefaultContentSource() {
        var source = new SmartContentSource(base.CreateDefaultContentSource());
        source.AddDirectoryRedirect("Content", "Assets/Textures");
        return source;

        _ = Mods.Chronicles.ItemName.Katana;
    }
}
