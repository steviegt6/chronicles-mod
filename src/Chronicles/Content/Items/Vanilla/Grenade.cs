using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Items.Vanilla;

public class Grenade : VanillaItem {
    public override object ItemTypes => ItemID.Grenade;

    public override void SetDefaults(Item item) {
        item.channel = true;
        item.UseSound = null;
        item.useStyle = ItemUseStyleID.Swing;
        item.useTime = item.useAnimation = 12;
        item.UseSound = null;
        item.shootSpeed = 10f;
        item.shoot = ModContent.ProjectileType<GrenadeProj>();
    }
}

public class GrenadeProj : ModProjectile {
    private readonly int explosionSize = 100;
    public bool released = false;

    public ref float Counter => ref Projectile.ai[0];

    public int MaxCounter => Player.itemTimeMax * 10;

    public Player Player => Main.player[Projectile.owner];

    public override string Texture => "Terraria/Images/Projectile_30";

    public override void SetDefaults() {
        Projectile.Size = new Vector2(10);
        Projectile.penetrate = -1;
        Projectile.friendly = Projectile.hostile = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (++Counter >= MaxCounter)
            Projectile.Kill();

        if (!released) {
            if (!Player.channel) {
                released = true;
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                Projectile.tileCollide = true;
            }
            if (Player.whoAmI == Main.myPlayer) {
                Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(Player.AngleTo(Main.MouseWorld));
                Projectile.netUpdate = true;
            }
            Player.ChangeDir(Math.Sign(Projectile.velocity.X));

            Player.heldProj = Projectile.whoAmI;
            Player.itemAnimation = Player.itemTime = Player.itemTimeMax;
            Projectile.rotation = Projectile.velocity.ToRotation() + ((Projectile.direction == -1) ? MathHelper.Pi : 0);
            Projectile.Center = Player.Center - (Vector2.Normalize(Projectile.velocity) * 16);

            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Player.AngleTo(Projectile.Center));
            Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -1.57f + Projectile.velocity.ToRotation());
        }
        else {
            Projectile.rotation += Projectile.velocity.X / 15;
            Projectile.velocity.Y += .25f;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.velocity.X *= .96f;
        return false;
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item14 with { PitchVariance = .5f }, Projectile.Center);

        for (var i = 0; i < 50; i++) {
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0, 0, 200, default, Main.rand.NextFloat(1f, 3f));
            if (i < 35) {
                var dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f), 0, default, Main.rand.NextFloat(2f));
                if (Main.rand.NextBool(4)) {
                    dust.noGravity = true;
                    dust.scale = 4f;
                    dust.fadeIn = 1.5f;
                }
            }
            if (i < 5)
                Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Unit(), GoreID.Smoke1, Main.rand.NextFloat(.5f, 1.5f)).alpha = 150;
        }
        var center = Projectile.Center;
        Projectile.width = Projectile.height = explosionSize;
        Projectile.Center = center;
        Projectile.Damage();
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.FinalDamage *= 1f + (1f - ((float)target.Distance(Projectile.Center) / explosionSize));

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) => modifiers.FinalDamage *= 1f + (1f - ((float)target.Distance(Projectile.Center) / explosionSize));

    public override bool ShouldUpdatePosition() => released;

    public override bool? CanDamage() => Counter >= MaxCounter;

    public override bool? CanCutTiles() => Counter >= MaxCounter;

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, 
            Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

        var glowColor = (Color.Red with { A = 0 }) * (Counter / MaxCounter);
        for (var i = 0; i < 3; i++)
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null,
                Projectile.GetAlpha(glowColor), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}
