using Chronicles.Core.ModLoader;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.Items.Vanilla;

public class ThrowingKnife : VanillaItem {
    public override object ItemTypes => ItemID.ThrowingKnife;

    public override void SetDefaults(Item item) {
        //item.shoot = ModContent.ProjectileType<ThrowingKnifeHeld>();
        item.useStyle = ItemUseStyleID.Swing;
        item.useTime = item.useAnimation = 10;
        item.UseSound = null;
        item.noUseGraphic = true;
        item.channel = true;
        item.useTurn = false;
    }
}