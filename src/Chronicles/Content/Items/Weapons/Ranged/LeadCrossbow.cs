using Terraria.ModLoader;
using Terraria.ID;
using Terraria;

namespace Chronicles.Content.Items.Weapons.Ranged;

public class LeadCrossbow : IronCrossbow {
    public override string Texture => "Chronicles/Assets/Items/Weapons/Ranged/LeadCrossbow";

    public override void SetDefaults() {
        Item.DamageType = DamageClass.Ranged;
        Item.damage = 20;
        Item.crit = 50;
        Item.knockBack = 7;
        Item.shoot = ModContent.ProjectileType<LeadCrossbowProj>();
        Item.useAmmo = AmmoID.Arrow;
        Item.shootSpeed = 12f;
        Item.width = 24;
        Item.height = 24;
        Item.useTime = Item.useAnimation = 40;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = false;
        Item.rare = ItemRarityID.Blue;
    }
}

public class LeadCrossbowProj : IronCrossbowProj {
    protected override int ParentItem => ModContent.ItemType<LeadCrossbow>();
    public override string Texture => "Chronicles/Assets/Items/Weapons/Ranged/LeadCrossbowProj";
}