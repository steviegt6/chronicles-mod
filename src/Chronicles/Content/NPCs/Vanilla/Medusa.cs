using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Medusa : VanillaNPC {
    public override object NPCTypes => NPCID.Medusa;

    public override bool PreAI(NPC npc) {
        npc.TargetClosest(npc.direction == 0);
        var target = Main.player[npc.target];
        npc.ai[2] = 0;

        if (!Alerted && Collision.CanHitLine(target.position, target.width, target.height, npc.position, npc.width, npc.height) && ++npc.ai[1] >= 240) {
            npc.ai[1] = 0;
            SetAlertness(npc, true);
        }
        if (Alerted) {
            npc.velocity.X = 0;

            if (++npc.ai[1] >= 30) {
                npc.direction = target.Center.X < npc.Center.X ? -1 : 1;
                npc.ai[2] = -1;

                if (target.direction == -npc.direction) {
                    if (!target.HasBuff(BuffID.Stoned))
                        target.AddBuff(BuffID.Stoned, 180); //Stone the target when in range

                    if (Main.rand.NextBool(2))
                        Dust.NewDustPerfect(npc.Center, DustID.TreasureSparkle, Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 3f));
                }
            } //Turn to the target
            if (npc.ai[1] >= 90) {
                npc.direction = target.Center.X < npc.Center.X ? 1 : -1;
                SetAlertness(npc, false);
                npc.ai[1] = 0;
            } //Turn away from the target
        }
        else {
            if (npc.velocity == Vector2.Zero && npc.collideX) {
                if (npc.ai[0] == 0) {
                    npc.velocity.Y = -5; //Jump
                    npc.ai[0] = 1;
                }
                else npc.direction = -npc.direction; //Turn around
            } //On horizontal collision, try jumping once, then turn around if still colliding
            if (npc.Distance(target.Center) > (16 * 60))
                npc.direction = target.Center.X < npc.Center.X ? -1 : 1;
            if (npc.velocity.Y == 0 && npc.ai[0] == 1)
                npc.ai[0] = 0;

            npc.velocity.X = npc.direction * .5f;
            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        }
        return false;
    }
}
