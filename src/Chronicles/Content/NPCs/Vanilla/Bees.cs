using Chronicles.Core.ModLoader;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Bees : VanillaNPC {
    public override object NPCTypes => new int[] { NPCID.Bee, NPCID.BeeSmall };

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) {
        npc.active = false;
        npc.netUpdate = true;
    } //Die on contact
}