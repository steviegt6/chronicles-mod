using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Items.Weapons.Melee;

public class WoodenStaff : ModItem {
    public override void SetDefaults() {
        Item.DamageType = DamageClass.Melee;
        Item.damage = 8;
        Item.knockBack = 4f;
        Item.shoot = ModContent.ProjectileType<WoodenStaffProj>();
        Item.shootSpeed = 2f;
        Item.width = Item.height = 24;
        Item.useTime = Item.useAnimation = 22;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item7 with { Volume = 1.5f };
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        => player.ownedProjectileCounts[Item.shoot] < 1;
}

public class WoodenStaffProj : ModProjectile {
    private bool Released {
        get => (int)Projectile.ai[0] != 0;
        set => Projectile.ai[0] = value ? 1 : 0;
    }
    private readonly int staffLength = 100;

    private Player Player => Main.player[Projectile.owner];

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
        Projectile.scale = 0f;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.scale += Math.Sign(1 - Projectile.scale) * .05f;

        if (!Released) {
            if (Main.rand.NextBool(2)) {
                for (var i = 0; i < 2; i++) {
                    var dustPos = Projectile.Center + (Vector2.UnitX * ((staffLength * .5f) * Projectile.scale)).RotatedBy(-.785f + (MathHelper.Pi * i) + Projectile.rotation);
                    Dust.NewDustPerfect(dustPos, DustID.WoodFurniture, Vector2.UnitY.RotatedBy(Projectile.rotation - .785f), 50, default, .7f).noGravity = true;
                }
            }

            if (!Player.channel) {
                Player.itemAnimation = Player.itemTime = Player.itemTimeMax;
                Projectile.scale = 1.2f;
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.position);

                Released = true;
            }
        }

        Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, .785f + Projectile.rotation + ((Player.direction == 1) ? MathHelper.Pi : 0));

        Player.heldProj = Projectile.whoAmI;
        Projectile.Center = Player.Center;
        Projectile.rotation = Released ? 
            .785f + ((float)MathHelper.Min(1, (float)Player.itemAnimation / (Player.itemAnimationMax / 2)) * .3f) * Player.direction : 
            (Projectile.rotation + (Player.GetAttackSpeed(DamageClass.Melee) * 0.15f) * Player.direction);

        if ((Player.itemAnimation > 2 || !Released) && Player.active && !Player.dead) //Active check
            Projectile.timeLeft = 2;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        var rotationUnit = -.785f + Projectile.rotation;
        var lineOffset = Vector2.UnitX * (staffLength / 2);

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + lineOffset.RotatedBy(rotationUnit + MathHelper.Pi), Projectile.Center + lineOffset.RotatedBy(rotationUnit));
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;
        var origin = texture.Size() / 2;

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

        for (var i = 0; i < Projectile.oldPos.Length; i++) {
            var drawPos = Projectile.oldPos[i] - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);

            var color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length) * .5f;
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[i], origin, Projectile.scale, SpriteEffects.None, 0);
        }
        return false;
    }
}