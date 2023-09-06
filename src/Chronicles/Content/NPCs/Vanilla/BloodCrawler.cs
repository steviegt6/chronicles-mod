using Chronicles.Core.ModLoader;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class BloodCrawler : VanillaNPC {
    public override object NPCTypes => new int[] { NPCID.BloodCrawler, NPCID.BloodCrawlerWall };

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) => target.AddBuff(BuffID.Bleeding, 180);
}