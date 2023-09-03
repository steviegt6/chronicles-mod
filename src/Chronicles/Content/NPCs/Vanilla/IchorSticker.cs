using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class IchorSticker : VanillaNPC {
    public override object NPCTypes => NPCID.IchorSticker;

    public override bool PreAI(NPC npc) {
        npc.TargetClosest(false);
        var target = Main.player[npc.target];
        var flyToPos = target.Center - (Vector2.UnitY * 200);

        if (Math.Abs(npc.Center.X - target.Center.X) < 20) {
            if ((npc.ai[0] = ++npc.ai[0] % 20) == 0)
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + (Vector2.UnitY * (npc.height / 2)), Vector2.UnitY * 10f, ProjectileID.GoldenShowerHostile, npc.damage, 0);
        } //Attack the player in range
        npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(flyToPos) * 4f, .04f); //Try to fly above the target
        npc.rotation = npc.velocity.X / 12;

        npc.noGravity = true;
        return false;
    }
}