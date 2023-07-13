using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Projectiles.Hostile;

public class SporeGas : ModProjectile
{
    public ref float Counter => ref Projectile.ai[0];
    private readonly int timeLeftMax = 120;

    public override string Texture => "Chronicles/Assets/Projectiles/Hostile/SporeGas";

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 5;

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.alpha = 255;
        Projectile.timeLeft = timeLeftMax;
    }

    public override void AI()
    {
        if (Projectile.timeLeft == timeLeftMax) {
            Projectile.scale = Main.rand.NextFloat(0.8f, 2.0f);
            Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
        }

        var fadeTime = timeLeftMax / 2;
        const int alpha_min = 150;

        if (Projectile.timeLeft > fadeTime)
            Projectile.alpha = (int)MathHelper.Max(Projectile.alpha - (255 / fadeTime), alpha_min);
        else
            Projectile.alpha = (int)MathHelper.Min(Projectile.alpha + (255 / fadeTime), 255);
        Projectile.scale += .001f;
        Projectile.rotation += .01f;

        //Player damage
        foreach (var player in Main.player.Where(x => x.active && !x.dead && x.Hitbox.Intersects(Projectile.Hitbox))) {
            player.AddBuff(BuffID.Suffocation, 30);
            player.AddBuff(BuffID.Poisoned, 300);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Projectile.type].Value;
        var frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor),
            Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor) * .5f,
            -Projectile.rotation, frame.Size() * 0.5f, Projectile.scale * 1.05f, SpriteEffects.None, 0);
        return false;
    }
}