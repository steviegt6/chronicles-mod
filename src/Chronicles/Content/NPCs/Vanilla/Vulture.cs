using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Vulture : VanillaNPC {
    public override object NPCTypes => NPCID.Vulture;

    public override bool PreAI(NPC npc) {
        if (!Alerted) {
            npc.damage = 0;

            npc.TargetClosest();
            var target = Main.player[npc.target];

            if (target.Distance(npc.Center) < (16 * 3) && (npc.ai[2] = Math.Max(npc.ai[2] - 1, 0)) == 0) {
                SoundEngine.PlaySound(SoundID.NPCHit11, npc.Center);

                npc.ai[2] = 20;
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    npc.velocity = new Vector2(12 * Main.rand.NextFloat(-1f, 1f), -6f);
                    npc.netUpdate = true;
                }
            }

            npc.velocity.X *= .95f;
            npc.velocity.Y = Math.Min(npc.velocity.Y, .5f);
            if (Math.Abs(npc.velocity.X) < .2f)
                npc.velocity.X = 0;

            return false;
        }
        return true;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) {
        SetAlertness(npc, true);
        npc.damage = npc.defDamage;

        foreach (var otherNPC in Main.npc.Where(x => x.active && x.whoAmI != npc.whoAmI && x.Distance(npc.Center) < (16 * 20) && x.type == NPCID.Vulture)) {
            otherNPC.GetGlobalNPC<Vulture>().SetAlertness(npc, true);
            otherNPC.damage = otherNPC.defDamage;
        }
    }
}