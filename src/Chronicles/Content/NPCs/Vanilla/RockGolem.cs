using Chronicles.Core.ModLoader;
using System;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class RockGolem : VanillaNPC {

    public override object NPCTypes => NPCID.RockGolem;

    public override void SetDefaults(NPC npc) => npc.damage = 0;

    public override bool PreAI(NPC npc) {
        const float idle_speed = .3f;

        //Stumble around slowly and aimlessly
        if ((Main.rand.NextBool(250) || npc.velocity.X == 0 || Math.Abs(npc.velocity.X) > idle_speed) && Main.netMode != NetmodeID.MultiplayerClient) {
            npc.velocity.X = Main.rand.NextBool(3) ? Main.rand.NextFloat(-idle_speed, idle_speed) : 0;
            npc.netUpdate = true;
        }
        npc.spriteDirection = npc.direction = (npc.velocity.X > 0) ? 1 : -1;

        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        return false;
    }
}