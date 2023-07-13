using Chronicles.Core;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Wolf : VanillaNPC,
                    IPackNPC {
    private readonly List<int> packWhoAmIs = new();
    private int? lastTarget;

    public override object NPCTypes => NPCID.Wolf;

    public int PackSize() => Main.rand.Next(2, 5) + 1;

    public override void SetDefaults(NPC npc) {
        npc.scale = Main.rand.NextFloat(0.8f, 1.1f);
        npc.Size = new Vector2((int)(npc.width * npc.scale), (int)(npc.height * npc.scale));
    }

    public override void OnSpawn(NPC npc, IEntitySource source) {
        base.OnSpawn(npc, source);

        foreach (var packNPC in Main.npc.Where(x => x.active && x.type == npc.type && x.whoAmI != npc.whoAmI && x.Distance(npc.Center) < 150))
            packWhoAmIs.Add(packNPC.whoAmI);
    }

    public override bool PreAI(NPC npc) {
        if (!packWhoAmIs.Any()) {
            if (!lastTarget.HasValue) {
                lastTarget = npc.target;
            }
            else {
                npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.DirectionFrom(Main.player[lastTarget.Value].Center).X * 8f, .1f);

                float throwaway = 6;
                Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref throwaway, ref throwaway);
                if (npc.collideX && npc.collideY) //Jump
                    npc.velocity.Y = -5;
            } //Run from the last target if no pack members remain
        }
        else {
            foreach (var packNPCIndex in packWhoAmIs) {
                if (!Main.npc[packNPCIndex].active && Main.npc[packNPCIndex].type == npc.type) {
                    packWhoAmIs.Remove(packNPCIndex); //Remove pack members who are inactive
                    break;
                }
            }
        }
        return !lastTarget.HasValue;
    }
}