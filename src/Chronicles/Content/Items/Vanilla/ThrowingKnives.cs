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

namespace Chronicles.Content.Items.Vanilla;

public class ThrowingKnives : VanillaItem {
    public override object ItemTypes => new int[] { ItemID.ThrowingKnife, ItemID.PoisonedKnife, ItemID.BoneDagger, ItemID.FrostDaggerfish };

    public override void SetDefaults(Item item) {
        item.useStyle = ItemUseStyleID.Swing;
        item.UseSound = null;
        item.noUseGraphic = true;
        item.channel = true;
        item.useTurn = false;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ThrowingKnifeHeld>(), damage, knockback, player.whoAmI, 0, type);
        return false;
    }
}

public class ThrowingKnifeHeld : ChroniclesProjectile {
    public readonly int numShots = 4;
    public bool released = false;

    public int Charge {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public int ShotIndex {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public int ChargeMax => (int)(Player.itemTimeMax * 4.5f);

    public Player Player => Main.player[Projectile.owner];

    public override string Texture => "Terraria/Images/Projectile_48";

    public override void SetDefaults() {
        Projectile.width = Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 2;
    }

    public override void AI() {
        var timePerThrow = Player.itemAnimationMax / numShots;

        if (!released) {
            if (!Player.channel) {
                released = true;
                SoundEngine.PlaySound(SoundID.Item1);
                Projectile.alpha = 255;
            }
            if (Player.whoAmI == Main.myPlayer) {
                Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(Player.AngleTo(Main.MouseWorld));
                Projectile.netUpdate = true;
            }
            Player.ChangeDir(Math.Sign(Projectile.velocity.X));

            Player.heldProj = Projectile.whoAmI;
            Player.itemAnimation = Player.itemTime = timePerThrow * ((int)((float)Charge / ChargeMax * numShots) + 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + ((Projectile.direction == 1) ? MathHelper.Pi : 0);
            Projectile.Center = Player.Center - (Vector2.Normalize(Projectile.velocity) * 16);
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Player.AngleTo(Projectile.Center));
            Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -1.57f + Projectile.velocity.ToRotation());

            Charge = Math.Min(Charge + 1, ChargeMax);
        }
        else {
            Projectile.Center = Player.Center + (Vector2.Normalize(Projectile.velocity) * 10);

            if (((Player.itemAnimation + 1) % timePerThrow) == 0 && Player.whoAmI == Main.myPlayer) {
                var shots = Charge / ((float)ChargeMax / numShots);
                var velocity = Projectile.velocity.RotatedBy((((Player.itemAnimation - 1) / timePerThrow) - (shots / 2f)) * .15f * -Projectile.direction);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ShotIndex, Projectile.damage, Projectile.knockBack, Player.whoAmI);
            }
        }
        if (((Player.itemAnimation > 2) || !released) && Player.active && !Player.dead)
            Projectile.timeLeft = 2;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanDamage() => false;

    public override bool? CanCutTiles() => false;

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[ShotIndex].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null,
            Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

        if (Charge < ChargeMax)
            DrawSparkle();
        return false;
    }

    public void DrawSparkle() {
        var texture = TextureAssets.Projectile[79].Value;
        var scalar = Math.Max(1f - (Charge % (ChargeMax / numShots)) * .2f, 0) * .5f;
        var rotation = (float)Main.timeForVisualEffects / 30f;

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null,
            Projectile.GetAlpha(Color.White with { A = 0 }), rotation, texture.Size() / 2, scalar, SpriteEffects.None, 0);
    }
}