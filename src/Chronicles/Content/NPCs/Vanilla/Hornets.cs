using Chronicles.Core;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Hornets : VanillaNPC,
                       IPackNPC {
    public override object NPCTypes => new int[] { NPCID.Hornet, NPCID.HornetHoney, NPCID.HornetFatty, NPCID.HornetLeafy, NPCID.HornetSpikey, NPCID.HornetStingy, NPCID.MossHornet };

    public int PackSize() => Main.rand.Next(2, 6);

    public override void SetDefaults(NPC npc) {
        npc.scale = Main.rand.NextFloat(0.8f, 1.25f);
        npc.Size = new Vector2((int)(npc.width * npc.scale), (int)(npc.height * npc.scale));
    }

    public override bool PreAI(NPC npc) {
        npc.TargetClosest(Alerted);
        var target = Main.player[npc.target];

        bool canStayAlert() {
            if (target == null)
                return false;
            return Collision.CanHit(npc, target) && npc.Distance(target.Center) < (16 * 40);
        }

        if (Alerted) {
            if (canStayAlert()) {
                npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(target.Center) * 3.8f, .1f);
                npc.ai[0] = 0;
            }
            else if (++npc.ai[0] > 120) //Wait 2 seconds before forgetting about an out-of-sight target
                SetAlertness(npc, false);
        }
        else {
            if ((((npc.ai[0] = ++npc.ai[0] % 60) == 0 && Main.rand.NextBool(3)) || (npc.velocity == Vector2.Zero)) && Main.netMode != NetmodeID.MultiplayerClient) {
                npc.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat() * 2f;
                npc.netUpdate = true;
            }

            npc.direction = (npc.velocity.X > 0) ? 1 : -1;

            if (npc.Distance(target.Center) < (16 * 10) && canStayAlert())
                SetAlertness(npc, true);
        }

        //Swivel rapidly in place
        npc.gfxOffY += (float)Math.Sin((npc.ai[2] = ++npc.ai[2] % 360) / 2.5f);
        npc.rotation = Utils.AngleLerp(npc.rotation, npc.velocity.X / 5, .1f);

        npc.spriteDirection = npc.direction;

        return false;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) => SetAlertness(npc, true);

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) => target.AddBuff(BuffID.Poisoned, 180);

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) => spawnRate = 4;
}