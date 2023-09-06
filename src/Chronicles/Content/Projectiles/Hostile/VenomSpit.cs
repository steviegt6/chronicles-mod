using Microsoft.Xna.Framework;
using Terraria;
using Chronicles.Core.ModLoader;
using System;
using Terraria.ID;
using Terraria.Audio;

namespace Chronicles.Content.Projectiles.Hostile;

public class VenomSpit : ChroniclesProjectile {
    public override string Texture => "Terraria/Images/Projectile_444";

    public override void SetDefaults() {
        Projectile.Size = new Vector2(16);
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.alpha = 255;
    }

    public override void AI() {
        Projectile.alpha = Math.Max(Projectile.alpha - 10, 0);
        Projectile.rotation += .1f * Projectile.direction;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, new Vector2(0, 7), .02f);

        for (var i = 0; i < 2; i++)
            Dust.NewDustPerfect(Projectile.Center, DustID.Venom, Projectile.velocity.RotatedByRandom(.25f), 0, default, 1f).noGravity = true;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Venom, 180);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Venom, 180);

    public override void Kill(int timeLeft) {
        for (var i = 0; i < 15; i++) {
            var dust = Dust.NewDustPerfect(Projectile.Center, DustID.Venom, -(Projectile.velocity * Main.rand.NextFloat()).RotatedByRandom(2f));
            if (i < 10) {
                dust.noGravity = true;
                dust.scale = 3f;
                dust.fadeIn = 2f;
            }
        }
        SoundEngine.PlaySound(SoundID.NPCDeath19, Projectile.Center);
    }
}
