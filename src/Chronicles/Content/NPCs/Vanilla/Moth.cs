using Chronicles.Core.ModLoader;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Moth : VanillaNPC {
    public override object NPCTypes => NPCID.Moth;

    public override void SetDefaults(NPC npc) {
        npc.aiStyle = NPCAIStyleID.Firefly;
        npc.damage = 0;
    }

    public override void AI(NPC npc) {
        var target = Main.player[npc.target];

        if (target.Distance(npc.Center) < (16 * 10))
            npc.velocity = npc.DirectionFrom(target.Center);
        npc.direction = npc.spriteDirection = (npc.velocity.X < 0) ? -1 : 1;
    }
}