using Chronicles.Core;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Pixie : VanillaNPC,
                     IPackNPC {
    public override object NPCTypes => NPCID.Pixie;

    public int PackSize() => Main.rand.Next(2, 5);

    public override void SetDefaults(NPC npc) {
        npc.scale = Main.rand.NextFloat(0.7f, 1.25f);
        npc.Size = new Vector2((int)(npc.width * npc.scale), (int)(npc.height * npc.scale));
    }

    public override bool PreAI(NPC npc) {
        void changeDir(int bias = 0) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            npc.ai[1] = (bias == 0) ? Main.rand.NextFloat(-1.5f, 1.5f) : bias * Main.rand.NextFloat(1.5f);
            if (Math.Abs(npc.ai[1]) < .5f)
                npc.ai[1] = .5f * ((npc.ai[1] < 0) ? -1 : 1);

            npc.netUpdate = true;
        }

        if (!Alerted) {
            if (npc.ai[1] == 0) {
                npc.TargetClosest(false);
                var target = Main.player[npc.target];

                changeDir((target.Center.X < npc.Center.X) ? -1 : 1);
            } //This should only be called when the NPC first spawns and a vector has not yet been initialized
            if (++npc.ai[0] % (60 * 5) == 0 && Main.rand.NextBool(2))
                changeDir();

            if (npc.collideX && npc.velocity.X == 0)
                npc.direction = -npc.direction;

            npc.direction = (int)Math.Sin(npc.ai[1]);
            //Handle NPC direction

            var tilePos = npc.Bottom + new Vector2(npc.width + 16, 16 * 3);
            float velY = WorldGen.SolidOrSlopedTile(Framing.GetTileSafely(tilePos)) ? -1 : 1;
            var hoverSin = (float)Math.Sin(npc.ai[0] / 30) * .75f;

            npc.velocity = Vector2.Lerp(npc.velocity, new Vector2(npc.ai[1], velY).RotatedBy(hoverSin), .04f);
            npc.damage = 0;

            Lighting.AddLight(npc.Center, TorchID.Torch);
            if (Main.rand.NextBool(12))
                Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Pixie, Alpha: 150).velocity = Vector2.Zero;

            return false;
        }
        else return true;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) {
        SetAlertness(npc, true);
        npc.damage = npc.defDamage;

        foreach (var otherNPC in Main.npc.Where(x => x.active && x.whoAmI != npc.whoAmI && x.Distance(npc.Center) < (16 * 15) && x.type == NPCID.Pixie)) {
            otherNPC.GetGlobalNPC<Pixie>().SetAlertness(npc, true);
            otherNPC.damage = otherNPC.defDamage;
        }
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) => target.AddBuff(BuffID.Darkness, 200);

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) => spawnRate = 2;
}