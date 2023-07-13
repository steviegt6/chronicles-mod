using Chronicles.Core;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class FireImp : VanillaNPC,
                       IPackNPC {

    private static readonly float[] scales = new float[] { .8f, 1f, 1.1f };

    public override object NPCTypes => NPCID.FireImp;

    public int PackSize() => Main.rand.Next(2, 5);

    public override void SetDefaults(NPC npc) {
        npc.scale = Main.rand.NextFromList(scales);
        npc.Size = new Vector2((int)(npc.width * npc.scale), (int)(npc.height * npc.scale));
    }

    public override bool PreAI(NPC npc) {
        npc.velocity.X *= .9f;

        if (Main.rand.NextBool(4)) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Torch);
            dust.velocity = Vector2.UnitY * -.5f;
            dust.noGravity = true;
            dust.fadeIn = Main.rand.NextFloat(1f, 1.2f);
        }
        return false;
    }
}