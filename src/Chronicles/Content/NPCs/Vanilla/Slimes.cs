using Chronicles.Content.NPCs.Hostile;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.NPCs.Vanilla;

public class Slimes : VanillaNPC {
    public bool setScale = false;

    //This is not a fine range so as to visually represent the number of slimelings that will be spawned on death
    private static readonly float[] scales = new float[] { .8f, 1f, 1.25f };

    public override object NPCTypes => new int[] { NPCID.BlueSlime, NPCID.IlluminantSlime, NPCID.IceSlime, NPCID.SandSlime };

    public override void AI(NPC npc) {
        if (!setScale) {
            npc.scale = Main.rand.NextFromList(scales);
            npc.Size = new Vector2((int)(npc.width * npc.scale), (int)(npc.height * npc.scale));

            setScale = true;
        }
    }

    public override void OnKill(NPC npc) {
        //Split into smaller slimelings on death
        var numSlimes = 2 + Array.FindIndex(scales, x => x == npc.scale);

        for (var i = 0; i < numSlimes; i++) {
            var slimeling = NPC.NewNPCDirect(new EntitySource_Death(npc), (int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<Slimeling>());
            slimeling.ai[0] = npc.netID;
            slimeling.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat() * 10f;

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendData(MessageID.SyncNPC, number: slimeling.whoAmI);
        }
    }
}