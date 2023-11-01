using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Chronicles.Core.ModLoader;

public abstract class VanillaItem : GlobalItem,
                                   IChroniclesType<GlobalItem> {

    /// <summary>Simply set to whos item types you want to change the functionality of.</summary>
    public abstract object ItemTypes { get; }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
        return ItemTypes switch {
            Array => ((int[])ItemTypes).Contains(entity.type),
            int => (int)ItemTypes == entity.type,
            short => (short)ItemTypes == entity.type,
            _ => false,
        };
    }
}