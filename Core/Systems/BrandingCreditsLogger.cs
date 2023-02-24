using JetBrains.Annotations;
using Terraria.ModLoader;

namespace Chronicles.Core.Systems;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class BrandingCreditsLogger : ModSystem {
    public override void OnModLoad() {
        base.OnModLoad();

        void log(string msg) {
            Mod.Logger.Info(msg);
        }
        
        log($"{Mod.DisplayName} ({Mod.Name}) v{Mod.Version}");
        log("by Tomat, ZENISIS, and Lunin, with help from any contributors.");
        log("");
        log("Assets licensed under All Rights Reserved (ARR), code licensed under the GNU General Public License, version 3.0 (GPLv3).");
        log("FOSS @ <https://github.com/steviegt6/chronicles-mod>");
        log("See COPYING in the root directory of the repository for further licensing information.");
    }
}
