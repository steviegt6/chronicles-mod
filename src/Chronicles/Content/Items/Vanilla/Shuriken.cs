using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Audio;

namespace Chronicles.Content.Items.Vanilla;

public class Shuriken : VanillaItem {
    public override object ItemTypes => ItemID.Shuriken;

    public override void SetDefaults(Item item) {
        item.shoot = ModContent.ProjectileType<ShurikenHeld>();
        item.useStyle = ItemUseStyleID.Swing;
        item.useTime = item.useAnimation = 25;
        item.shootSpeed = 8f;
        item.UseSound = null;
        item.noUseGraphic = true;
        item.channel = true;
        item.useTurn = false;
    }
}

public class ShurikenHeld : ChroniclesProjectile {
    public readonly int numShots = 5;
    public bool released = false;

    public int Charge {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public int ChargeMax => (int)(Player.itemTimeMax * 3.5f);

    public Player Player => Main.player[Projectile.owner];

    public override string Texture => "Terraria/Images/Projectile_3";

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
            if (!Player.channel)
                released = true;
            if (Player.whoAmI == Main.myPlayer) {
                Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(Player.AngleTo(Main.MouseWorld));
                Projectile.netUpdate = true;
            }
            Player.ChangeDir(Math.Sign(Projectile.velocity.X));

            Player.heldProj = Projectile.whoAmI;
            Player.itemAnimation = Player.itemTime = timePerThrow * (int)((float)Charge / ChargeMax * 5);
            Projectile.Center = Player.Center - (Vector2.Normalize(Projectile.velocity) * 20);
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Player.AngleTo(Projectile.Center));

            if ((Charge + 1) % (ChargeMax / numShots) == 0)
                SoundEngine.PlaySound(SoundID.MaxMana);
            Charge = Math.Min(Charge + 1, ChargeMax);
        }
        else {
            Projectile.Center = Player.Center + (Vector2.Normalize(Projectile.velocity) * 10);
            Projectile.alpha = 255;

            if (((Player.itemAnimation + 1) % timePerThrow) == 0 && Player.whoAmI == Main.myPlayer) {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity, ProjectileID.Shuriken, Projectile.damage, Projectile.knockBack, Player.whoAmI);

                for (var i = 0; i < 8; i++) {
                    var vel = (Projectile.velocity * Main.rand.NextFloat()).RotatedByRandom(.1f);
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Main.rand.NextBool() ? DustID.SilverFlame : DustID.Smoke, vel.X, vel.Y, 180, default, Main.rand.NextFloat(.25f, 1.2f)).noGravity = true;
                }
            }
        }
        if (((Player.itemAnimation > 2) || !released) && Player.active && !Player.dead)
            Projectile.timeLeft = 2;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanDamage() => false;

    public override bool? CanCutTiles() => false;

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null,
            Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        DrawSparkle();

        return false;
    }

    public void DrawSparkle() {
        var texture = TextureAssets.Projectile[79].Value;
        var scalar = 1f - (Charge % (ChargeMax / numShots)) * .4f;
        var pos = Projectile.Center;

        Main.EntitySpriteDraw(texture, pos - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, Color.White with { A = 0 }, 0, texture.Size() / 2, scalar * .1f, SpriteEffects.None, 0);
    }
}

public class ShurikenProj : VanillaProjectile {
    public override object ProjectileTypes => ProjectileID.Shuriken;

    public override void SetDefaults(Projectile projectile) => projectile.extraUpdates = 1;

    public override bool PreAI(Projectile projectile) {
        projectile.rotation += .1f * projectile.direction;
        return false;
    }
}