using JetBrains.Annotations;
using Terraria.ModLoader;

namespace Chronicles.Core.ModLoader;

/// <summary>
///     Base class for all <see cref="ModItem"/>s in Chronicles, featuring
///     robust abstractions and utilities for quickly scaffolding new items.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class ChroniclesItem : ModItem,
                                       IChroniclesType<ModItem> {
    public override string Texture {
        get {
            var ns = (GetType().Namespace ?? "Chronicles").Split('.')[0];
            return $"{ns}/Assets/" + base.Texture[(ns.Length + 1)..];
        }
    }
}
