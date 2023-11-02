using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Chronicles.Core.ModLoader;

public abstract class VanillaProjectile : GlobalProjectile,
                                   IChroniclesType<GlobalProjectile> {

    /// <summary>Simply set to whos projectile types you want to change the behaviour of.</summary>
    public abstract object ProjectileTypes { get; }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
        return ProjectileTypes switch {
            Array => ((int[])ProjectileTypes).Contains(entity.type),
            int => (int)ProjectileTypes == entity.type,
            short => (short)ProjectileTypes == entity.type,
            _ => false,
        };
    }
}