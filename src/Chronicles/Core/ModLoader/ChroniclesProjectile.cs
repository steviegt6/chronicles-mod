using JetBrains.Annotations;
using Terraria.ModLoader;

namespace Chronicles.Core.ModLoader;

/// <summary>
///     Base class for all <see cref="ModProjectile"/>s in Chronicles.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class ChroniclesProjectile : ModProjectile,
                                             IChroniclesType<ChroniclesProjectile> {
    public override string Texture {
        get {
            var ns = (GetType().Namespace ?? "Chronicles").Split('.')[0];
            return $"{ns}/Assets/" + base.Texture[(ns.Length + 1)..].Replace("Content/", string.Empty);
        }
    }
}
