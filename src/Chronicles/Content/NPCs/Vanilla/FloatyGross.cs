using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class FloatyGross : VanillaNPC {
    public override object NPCTypes => NPCID.FloatyGross;

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => false;

    public override void PostAI(NPC npc) {
        var target = Main.player[npc.target];

        if (npc.Distance(target.Center) < 30) {
            npc.Center = Vector2.Lerp(npc.Center, target.Center, .09f);
            npc.gfxOffY = target.gfxOffY;
            npc.velocity = Vector2.Zero;

            target.AddBuff(BuffID.Weak, 300);
            target.AddBuff(BuffID.Suffocation, 30);
            target.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(npc.whoAmI), npc.damage / 3, 0, false, false, -1, false, 0, 0, 0);
        }
    }
}