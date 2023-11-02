using Chronicles.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Items.Weapons.Melee;

public class TinMace : CopperClub {
    private bool reverseSwing;

    public override void SetDefaults() {
        Item.DamageType = DamageClass.Melee;
        Item.damage = 18;
        Item.crit = 11;
        Item.knockBack = 7f;
        Item.shoot = ModContent.ProjectileType<TinMaceProj>();
        Item.shootSpeed = 2f;
        Item.width = Item.height = 24;
        Item.useTime = Item.useAnimation = 32;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item1;
    }

    public override ModItem Clone(Item newEntity) {
        var clone = (TinMace)NewInstance(newEntity);
        clone.reverseSwing = reverseSwing;

        return clone;
    }
}

public class TinMaceProj : CopperClubProj {
    protected override int SwingDusts => DustID.Tin;

    public override string Texture => Assets.Textures.Items.Weapons.Melee.TinMace_Name;
}