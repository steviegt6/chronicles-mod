using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class DungeonSpirit : VanillaNPC  {
    public override object NPCTypes => NPCID.DungeonSpirit;

    public override bool PreAI(NPC npc) {
        var homePos = new Vector2(Main.dungeonX, Main.dungeonY) * 16;
        npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(homePos) * 4f, .01f);

        if (npc.Distance(homePos) < (16 * 2)) {
            npc.active = false;

            for (var i = 0; i < 10; i++)
                Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.DungeonSpirit, Scale: 2f).noGravity = true;
        } //Die after reaching the dungeon entrance

        npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;
        return false;
    }
}