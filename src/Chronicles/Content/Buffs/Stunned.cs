using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Chronicles.Content.Buffs;
public class Stunned : ModBuff {
    public override string Texture {
        get {
            var ns = (GetType().Namespace ?? "Chronicles").Split('.')[0];
            return $"{ns}/Assets/" + base.Texture[(ns.Length + 1)..].Replace("Content/", string.Empty);
        }
    }
}

public class StunnedGlobalNPC : GlobalNPC {
    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!npc.HasBuff(ModContent.BuffType<Stunned>()))
            return;

        var texture = Mod.Assets.Request<Texture2D>("Assets/Misc/StunStars").Value;
        var numFramesY = 6;
        var frame = texture.Frame(1, numFramesY, 0, (int)(Main.timeForVisualEffects / 4f % 5), 0, -2);
        var pos = npc.Top + new Vector2(0, -20 + npc.gfxOffY) - Main.screenPosition;

        spriteBatch.Draw(texture, pos, frame, npc.GetAlpha(drawColor), 0, frame.Size() / 2, 1, SpriteEffects.None, 0);
    }
}