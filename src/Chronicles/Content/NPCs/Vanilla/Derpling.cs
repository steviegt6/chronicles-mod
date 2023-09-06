using Chronicles.Core.ModLoader;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Derpling : VanillaNPC {
    public override object NPCTypes => NPCID.Derpling;

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => npc.velocity.Y > 0;
}