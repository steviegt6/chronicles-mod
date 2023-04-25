using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Items.Weapons.Melee;

public class CopperClub : ModItem {
    private bool reverseSwing;

    public override string Texture => "Chronicles/Assets/Items/Weapons/Melee/CopperClub";

    public override void SetDefaults() {
        Item.DamageType = DamageClass.Melee;
        Item.damage = 18;
        Item.crit = 11;
        Item.knockBack = 7f;
        Item.shoot = ModContent.ProjectileType<CopperClubProj>();
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
        var clone = (CopperClub)NewInstance(newEntity);
        clone.reverseSwing = reverseSwing;

        return clone;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, reverseSwing ? -1 : 1);
        reverseSwing = !reverseSwing;

        return false;
    }
}

public class CopperClubProj : ModProjectile {
    private readonly int swingRange = 240;
    private readonly int holdoutDistance = 48;

    private int DirUnit {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private float SwingCounter {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    protected virtual int SwingDusts => DustID.Copper;
    private Player Player => Main.player[Projectile.owner];

    public override string Texture => "Chronicles/Assets/Items/Weapons/Melee/CopperClub";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 4;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Player.heldProj = Projectile.whoAmI;

        var degrees = (float)(SwingCounter / Player.itemAnimationMax) * (swingRange * DirUnit);
        var startBias = 10;
        var rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(degrees - (((swingRange / 2) + startBias) * DirUnit));

        Projectile.Center = Player.Center - Projectile.velocity + (Vector2.UnitX * (float)(holdoutDistance * Projectile.scale)).RotatedBy(rotation);
        Projectile.rotation = Player.AngleTo(Projectile.Center);

        Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.rotation);
        Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.rotation);

        SwingCounter = MathHelper.Lerp(SwingCounter, Player.itemAnimationMax, 0.07f);

        if (SwingCounter >= (Player.itemAnimationMax - 2))
            Projectile.scale -= 0.003f;
        else {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, SwingDusts, 0f, 0f, 100, default, .7f);
            dust.velocity = Vector2.UnitY.RotatedBy(Projectile.rotation);
            dust.noGravity = true;
        }

        if ((Player.itemAnimation > 2) && Player.active && !Player.dead) //Active check
            Projectile.timeLeft = 2;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;
        var effects = (Projectile.direction == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var rotation = Projectile.rotation + ((effects == SpriteEffects.None) ? 0.785f : 2.355f);
        var origin = (effects == SpriteEffects.FlipHorizontally) ? Projectile.Size / 2 : new Vector2(texture.Width - (Projectile.width / 2), Projectile.height / 2);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, effects, 0);

        for (var i = 0; i < Projectile.oldPos.Length; i++) {
            var drawPos = Projectile.oldPos[i] - Main.screenPosition + origin + (Vector2.UnitY * Projectile.gfxOffY);

            if (effects == SpriteEffects.None)
                drawPos.X -= texture.Width - Projectile.width;

            var color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length) * .5f;
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[i] + (rotation - Projectile.rotation), origin, Projectile.scale, effects, 0);
        }
        return false;
    }
}