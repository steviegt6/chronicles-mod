using Chronicles.Core.ModLoader;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class FungiSpore : VanillaNPC {
    public override object NPCTypes => NPCID.FungiSpore;

    public override void SetDefaults(NPC npc) => npc.noTileCollide = false;
}