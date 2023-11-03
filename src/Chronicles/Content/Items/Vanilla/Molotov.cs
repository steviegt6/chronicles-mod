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

public class Molotov : VanillaItem {
    public override object ItemTypes => ItemID.MolotovCocktail;

    public override void SetDefaults(Item item) {
        item.channel = true;
        item.useStyle = ItemUseStyleID.Swing;
        item.useTime = item.useAnimation = 14;
        item.shootSpeed = 10f;
        item.shoot = ModContent.ProjectileType<MolotovProj>();
    }
}

public class MolotovProj : ModProjectile {
    public bool released = false;

    public ref float Counter => ref Projectile.ai[0];

    public int MaxCounter => Player.itemTimeMax * 10;

    public Player Player => Main.player[Projectile.owner];

    public override string Texture => "Terraria/Images/Projectile_399";

    public override void SetDefaults() {
        Projectile.Size = new Vector2(14);
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (++Counter >= MaxCounter) {
            Projectile.Kill();
        } //Explode
        if (Counter > (MaxCounter - 18))
            SoundEngine.PlaySound(SoundID.Item76, Projectile.Center);
        for (var i = 0; i < 3; i++) {
            var pos = Projectile.Center - (Vector2.UnitY * Projectile.width / 2).RotatedBy(Projectile.rotation);
            Dust.NewDustPerfect(pos, Main.rand.NextBool() ? DustID.Torch : DustID.SolarFlare, Main.rand.NextVector2Unit() * Main.rand.NextFloat(), 0, default, Main.rand.NextFloat(.4f, 1.5f) * (Counter / MaxCounter)).noGravity = true;
        }

        if (!released) {
            if (!Player.channel) {
                released = true;
                Projectile.tileCollide = true;
            }
            if (Player.whoAmI == Main.myPlayer) {
                Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(Player.AngleTo(Main.MouseWorld));
                Projectile.netUpdate = true;
            }
            Player.ChangeDir(Math.Sign(Projectile.velocity.X));

            Projectile.rotation = Projectile.velocity.ToRotation() + ((Projectile.direction == -1) ? MathHelper.Pi : 0);
            Projectile.Center = Player.Center + (Vector2.Normalize(Projectile.velocity) * 10);
            Player.heldProj = Projectile.whoAmI;
            Player.itemAnimation = Player.itemTime = Player.itemTimeMax;

            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.velocity.ToRotation());
        }
        else {
            Projectile.rotation += Projectile.velocity.X / 15;
            Projectile.velocity.Y += .25f;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Item52 with { Volume = .1f, Pitch = -1f }, Projectile.Center);
        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => SoundEngine.PlaySound(SoundID.Item52 with { Volume = .1f, Pitch = -1f }, Projectile.Center);

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item50 with { Pitch = .5f }, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item76, Projectile.Center);

        for (var i = 0; i < 40; i++) {
            if (i < 6) {
                var type = (i / 2) switch {
                    1 => ProjectileID.MolotovFire2,
                    2 => ProjectileID.MolotovFire3,
                    _ => ProjectileID.MolotovFire
                };
                var fire = Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center - Projectile.velocity, Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 5f), type, Projectile.damage, Projectile.knockBack, Player.whoAmI);
                fire.hostile = true;
                fire.netUpdate = true;
            }
            Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f), 0, default, Main.rand.NextFloat(3f));
        }
        Projectile.hostile = true;
        var center = Projectile.Center;
        Projectile.width = Projectile.height = 100;
        Projectile.Center = center;
        Projectile.Damage();
    }

    public override bool ShouldUpdatePosition() => released;

    public override bool? CanDamage() => released;

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
