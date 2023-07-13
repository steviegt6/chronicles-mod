using Chronicles.Content.Projectiles.Hostile;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.NPCs.Vanilla;

public class SporeTrail : VanillaNPC {
    private int sporeTimer;

    public override object NPCTypes => new int[] { NPCID.SporeBat, NPCID.SporeSkeleton, NPCID.ZombieMushroom, NPCID.ZombieMushroomHat };

    public override void PostAI(NPC npc) {
        var sporeRate = 10;
        if (npc.velocity.Length() > .5f && (sporeTimer = ++sporeTimer % sporeRate) == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + (Main.rand.NextVector2Unit() * Main.rand.NextFloat() * 20f), Vector2.Zero, ModContent.ProjectileType<SporeGas>(), npc.damage / 4, 0);
    }
}