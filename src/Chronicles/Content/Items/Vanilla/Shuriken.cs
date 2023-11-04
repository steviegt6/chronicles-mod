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
    public readonly int numShots = 4;
    public bool released = false;

    public int Charge {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public int ChargeMax => (int)(Player.itemTimeMax * 3f);

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
            Player.itemAnimation = Player.itemTime = timePerThrow * ((int)((float)Charge / ChargeMax * numShots) + 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Center = Player.Center - (Vector2.Normalize(Projectile.velocity) * 16);

            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Player.AngleTo(Projectile.Center));
            Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -1.57f + Projectile.velocity.ToRotation());
            Charge = Math.Min(Charge + 1, ChargeMax);
        }
        else {
            Projectile.Center = Player.Center + (Vector2.Normalize(Projectile.velocity) * 10);
            Projectile.alpha = 255;

            if (((Player.itemAnimation + 1) % timePerThrow) == 0) {
                if (Player.whoAmI == Main.myPlayer)
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity, ProjectileID.Shuriken, Projectile.damage, Projectile.knockBack, Player.whoAmI);

                for (var i = 0; i < 10; i++) {
                    var vel = (Projectile.velocity * Main.rand.NextFloat(.5f, 1.5f)).RotatedByRandom(.1f);
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Main.rand.NextBool() ? DustID.SilverFlame : DustID.Smoke, vel.X, vel.Y, 180, default, Main.rand.NextFloat(.25f, 1.4f)).noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item1);
            }
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.velocity.ToRotation() - (Player.itemAnimation % timePerThrow) * .5f * Projectile.direction);
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

public class ShurikenProj : VanillaProjectile {
    private bool collided = false;

    public override object ProjectileTypes => ProjectileID.Shuriken;

    public override void SetDefaults(Projectile projectile) => projectile.extraUpdates = 1;

    public override bool PreAI(Projectile projectile) {
        projectile.ai[0] = Math.Min(projectile.ai[0] + (1 / 15f), 1f);
        if (!collided)
            projectile.rotation += .2f * projectile.direction;
        else {
            if (projectile.scale > 1f)
                projectile.scale = Math.Max(projectile.scale - .1f, 1f);

            var fadeTime = 15;
            if (projectile.timeLeft <= fadeTime)
                projectile.alpha += 255 / fadeTime;
        }

        return false;
    }

    public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) {
        if (!collided) {
            collided = true;

            projectile.position += projectile.velocity;
            projectile.velocity = Vector2.Zero;
            projectile.timeLeft = 180;
            projectile.scale = 1.75f;

            Collision.HitTiles(projectile.Center, projectile.velocity, 16, 16);
            SoundEngine.PlaySound(SoundID.Dig, projectile.Center);
        }
        return false;
    }

    public override bool PreKill(Projectile projectile, int timeLeft) => timeLeft > 0;

    public override bool? CanDamage(Projectile projectile) => !collided ? null : false;

    public override bool? CanCutTiles(Projectile projectile) => !collided;

    public override bool PreDrawExtras(Projectile projectile) {
        if (!collided) {
            Main.instance.LoadProjectile(607);
            var texture = TextureAssets.Projectile[607].Value;
            var scale = new Vector2(1f - projectile.ai[0], 1f) * projectile.scale * .75f;
            Main.EntitySpriteDraw(texture, projectile.Center - Main.screenPosition, null, Color.Gray with { A = 0 } * .5f, projectile.velocity.ToRotation() - 1.57f, new Vector2(texture.Width / 2, texture.Height), scale, SpriteEffects.None);
        }
        return true;
    }
}