using JetBrains.Annotations;
using Terraria.ModLoader;

namespace Chronicles.Core.ModLoader;

/// <summary>
///     Base class for all <see cref="ModNPC"/>s in Chronicles.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class ChroniclesNPC : ModNPC,
                                      IChroniclesType<ModNPC> {
    public override string Texture {
        get {
            var ns = (GetType().Namespace ?? "Chronicles").Split('.')[0];
            return $"{ns}/Assets/" + base.Texture[(ns.Length + 1)..].Replace("Content/", string.Empty);
        }
    }
}
