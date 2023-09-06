using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Crimera : VanillaNPC {
    private readonly float damageThreshold = .25f;

    private bool latched = false;

    public override object NPCTypes => NPCID.Crimera;

    public override bool PreAI(NPC npc) {
        var target = Main.player[npc.target];

        if (npc.Distance(target.Center) < (16 * 2.5f) && npc.ai[2] <= 0)
            latched = true;

        if (latched) {
            npc.TargetClosest(false);
            target = Main.player[npc.target];

            npc.Center = target.Center + (Vector2.UnitX * 35).RotatedBy(npc.rotation - MathHelper.PiOver2);
            npc.gfxOffY = target.gfxOffY;
            target.AddBuff(BuffID.Darkness, 30);
            target.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(npc.whoAmI), npc.damage / 3, 0, false, false, -1, false, 0, 0, 0);

            return false;
        }
        else npc.ai[2] = Math.Max(npc.ai[2] - ((npc.lifeMax * damageThreshold) / 30), 0); //Return ai[2] to zero over 30 ticks, this is effectively a cooldown for latching

        return true;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) {
        if (latched && (npc.ai[2] += hit.Damage) > (npc.lifeMax * damageThreshold))
            latched = false;
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => !latched;

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) => spawnRate = 2;
}