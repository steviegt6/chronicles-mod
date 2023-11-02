using Chronicles.Core.ModLoader;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Chronicles.Content.Projectiles.Hostile;

public class ToxicChunk : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_523";

    public override void SetDefaults() {
        Projectile.Size = new Vector2(40);
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.alpha = 255;
        Projectile.scale = .5f;
    }

    public override void AI() {
        Projectile.alpha = Math.Max(Projectile.alpha - 10, 0);
        Projectile.rotation += .1f * Projectile.direction;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, new Vector2(0, 10), .02f);

        for (var i = 0; i < 2; i++)
            Dust.NewDustPerfect(Projectile.Center, DustID.Poisoned, Projectile.velocity.RotatedByRandom(.25f), 0, default, 1f).noGravity = true;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Poisoned, 180);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Poisoned, 180);

    public override void Kill(int timeLeft) {
        for (var i = 0; i < 15; i++) {
            var dust = Dust.NewDustPerfect(Projectile.Center, DustID.Poisoned, -(Projectile.velocity * Main.rand.NextFloat()).RotatedByRandom(2f));
            if (i < 10) {
                dust.noGravity = true;
                dust.scale = 3f;
                dust.fadeIn = 2f;
            }
        }
        SoundEngine.PlaySound(SoundID.NPCDeath19, Projectile.Center);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, 
            Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}
