using Chronicles.Core;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Pixie : VanillaNPC,
                     IPackNPC {
    public override object NPCTypes => NPCID.Pixie;

    public int PackSize() => Main.rand.Next(1, 4);

    public override void SetDefaults(NPC npc) {
        npc.scale = Main.rand.NextFloat(0.7f, 1.25f);
        npc.Size = new Vector2((int)(npc.width * npc.scale), (int)(npc.height * npc.scale));
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) => spawnRate = 2;
}