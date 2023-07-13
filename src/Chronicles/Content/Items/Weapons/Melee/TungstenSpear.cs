using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Items.Weapons.Melee;

public class TungstenSpear : SilverRanseur {
    public override void SetDefaults() {
        Item.DamageType = DamageClass.Melee;
        Item.damage = 15;
        Item.crit = 4;
        Item.knockBack = 1.8f;
        Item.shoot = ModContent.ProjectileType<TungstenSpearProj>();
        Item.shootSpeed = 2f;
        Item.width = Item.height = 24;
        Item.useTime = Item.useAnimation = 22;
        Item.useStyle = ItemUseStyleID.Rapier;
        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item1;
    }
}

public class TungstenSpearProj : SilverRanseurProj {
    public override string Texture => "Chronicles/Assets/Items/Weapons/Melee/TungstenSpearProj";
}