using Chronicles.Content.Projectiles.Hostile;
using Chronicles.Core.ModLoader;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.NPCs.Vanilla;

public class FlyingSnake : VanillaNPC {
    private int timer;

    public override object NPCTypes => NPCID.FlyingSnake;

    public override void PostAI(NPC npc) {
        var target = Main.player[npc.target];

        timer = ++timer % 120;
        if (npc.Distance(target.Center) < (16 * 12) && Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height)) {
            if (timer > 100)
                npc.velocity *= .8f; //Slow down before firing
            if (timer == 0) {
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    var vel = npc.Top.DirectionTo(target.Center) * 10f;
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Top, vel, ModContent.ProjectileType<VenomSpit>(), npc.damage, 0);
                }
                SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = .6f, Pitch = .5f }, npc.Center);
            } //Fire
        }
    }
}