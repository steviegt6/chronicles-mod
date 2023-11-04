using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace Chronicles.Content.Items.Vanilla;

public class Grenades : VanillaItem {
    public override object ItemTypes => new int[] { ItemID.Grenade, ItemID.BouncyGrenade, ItemID.Beenade, ItemID.PartyGirlGrenade };

    public override void SetDefaults(Item item) {
        item.channel = true;
        item.UseSound = null;
        item.useStyle = ItemUseStyleID.Swing;
        item.useTime = item.useAnimation = 12;
        item.UseSound = null;
        item.shootSpeed = 10f;
    }
}

public class GrenadeProjs : VanillaProjectile {
    private readonly int explosionSize = 100;
    public bool released = false;

    public static int MaxCounter(Player player) => player.itemTimeMax * 10;

    public override object ProjectileTypes => new int[] { ProjectileID.Grenade, ProjectileID.BouncyGrenade, ProjectileID.Beenade, ProjectileID.PartyGirlGrenade };

    public override void SetDefaults(Projectile projectile) {
        projectile.penetrate = -1;
        projectile.friendly = projectile.hostile = true;
    }

    public override bool PreAI(Projectile projectile) {
        var player = Main.player[projectile.owner];
        if (++projectile.ai[0] >= MaxCounter(player))
            projectile.Kill();

        if (!released) {
            if (!player.channel) {
                released = true;
                SoundEngine.PlaySound(SoundID.Item1, projectile.Center);
                projectile.tileCollide = true;
            }
            if (player.whoAmI == Main.myPlayer) {
                projectile.velocity = (Vector2.UnitX * projectile.velocity.Length()).RotatedBy(player.AngleTo(Main.MouseWorld));
                projectile.netUpdate = true;
            }
            player.ChangeDir(Math.Sign(projectile.velocity.X));

            player.heldProj = projectile.whoAmI;
            player.itemAnimation = player.itemTime = player.itemTimeMax;
            projectile.rotation = projectile.velocity.ToRotation() + ((projectile.direction == -1) ? MathHelper.Pi : 0);
            projectile.Center = player.Center - (Vector2.Normalize(projectile.velocity) * 16);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + player.AngleTo(projectile.Center));
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -1.57f + projectile.velocity.ToRotation());
        }
        else {
            projectile.rotation += projectile.velocity.X / 15;
            projectile.velocity.Y += .25f;
        }
        return false;
    }

    public override void OnKill(Projectile projectile, int timeLeft) {
        var center = projectile.Center;
        projectile.width = projectile.height = explosionSize;
        projectile.Center = center;
        projectile.Damage();
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) => modifiers.FinalDamage *= 1f + (1f - ((float)target.Distance(projectile.Center) / explosionSize));

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers) => modifiers.FinalDamage *= 1f + (1f - ((float)target.Distance(projectile.Center) / explosionSize));

    public override bool ShouldUpdatePosition(Projectile projectile) => released;

    public override bool? CanDamage(Projectile projectile) => projectile.ai[0] >= MaxCounter(Main.player[projectile.owner]);

    public override bool? CanCutTiles(Projectile projectile) => projectile.ai[0] >= MaxCounter(Main.player[projectile.owner]);

    public override bool PreDraw(Projectile projectile, ref Color lightColor) {
        var texture = TextureAssets.Projectile[projectile.type].Value;
        Main.EntitySpriteDraw(texture, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), null,
            projectile.GetAlpha(lightColor), projectile.rotation, texture.Size() / 2, projectile.scale, SpriteEffects.None, 0);

        var glowColor = (Color.Red with { A = 0 }) * (projectile.ai[0] / MaxCounter(Main.player[projectile.owner]));
        for (var i = 0; i < 3; i++)
            Main.EntitySpriteDraw(texture, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), null,
                projectile.GetAlpha(glowColor), projectile.rotation, texture.Size() / 2, projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}