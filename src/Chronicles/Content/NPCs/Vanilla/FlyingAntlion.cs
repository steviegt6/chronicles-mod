using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class FlyingAntlion : VanillaNPC {
    private bool charging = false;

    public override object NPCTypes => new int[] { NPCID.FlyingAntlion, NPCID.GiantFlyingAntlion };

    public override bool PreAI(NPC npc) {
        var target = Main.player[npc.target];

        var inDashRange = target.Distance(npc.Center) < (16 * 14);

        if (inDashRange || charging) {
            npc.ai[2] = ++npc.ai[2] % 225;

            if (charging = npc.ai[2] >= 200) {
                npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(target.Center) * 20f, .05f);
                npc.rotation = npc.velocity.ToRotation() + ((npc.spriteDirection == -1) ? MathHelper.Pi : 0);

                return false;
            } //Charge for 25 ticks
        }
        if (!charging && target.Distance(npc.Center) < (16 * 10))
            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionFrom(target.Center) * 3, .1f);

        return true;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) {
        if (!charging)
            return;

        npc.ai[2] = 0;
        charging = false;
    }
}