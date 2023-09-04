using Chronicles.Content.Buffs;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.NPCs.Vanilla;

public class WalkingAntlion : VanillaNPC {
    private bool charging;

    public override object NPCTypes => new int[] { NPCID.WalkingAntlion, NPCID.GiantWalkingAntlion };

    public override bool PreAI(NPC npc) {
        npc.TargetClosest(npc.direction == 0);
        var target = Main.player[npc.target];

        if (npc.HasBuff(ModContent.BuffType<Stunned>())) {
            npc.velocity.X *= .95f;
            charging = false;

            return false;
        }

        if (npc.ai[0] == 1 && npc.Distance(target.Center) < (16 * 10) && Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height)) {
            npc.ai[0] = 0;
            npc.velocity.X *= .1f;
            npc.TargetClosest();

            SoundEngine.PlaySound(SoundID.Run with { Pitch = -1f, Volume = .5f }, npc.Center);
            charging = true;
        } //Begin to charge

        if (charging) {
            if (npc.velocity.X == 0 && npc.collideX) {
                npc.ai[0] = 0;
                npc.AddBuff(ModContent.BuffType<Stunned>(), 180); //Stun on wall collision

                SoundEngine.PlaySound(SoundID.NPCHit31, npc.Center);
                npc.velocity -= new Vector2(npc.direction * 2, 2);
            }

            if (npc.ai[0] >= 1) {
                charging = false;
                npc.ai[0] = 0;
            } //Stop charging
            else if (npc.ai[0] <= .4f)
                npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.direction * 20f, .05f);

            for (var i = 0; i < 2; i++) {
                var dustType = Main.rand.NextBool(2) ? DustID.SandstormInABottle : DustID.Smoke;
                Dust.NewDustDirect(npc.BottomLeft, npc.width, 6, dustType, 0, 0, 100, default, Main.rand.NextFloat(.5f, 2f))
                    .velocity = new Vector2(npc.velocity.X * -.1f, -Main.rand.NextFloat(0f, 1f));
            }

            npc.ai[0] = Math.Min(npc.ai[0] + (1f / 60), 1); //Continue to charge for 40 ticks
        }
        else {
            if (npc.velocity.X == 0 && npc.collideX)
                npc.direction = -npc.direction; //Turn around on wall collision
            if (npc.Distance(target.Center) > (16 * 10) && Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height))
                npc.TargetClosest(); //Chase the target when possible

            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.direction * 2.5f, .08f);

            npc.ai[0] = Math.Min(npc.ai[0] + (1f / 60), 1); //60 tick cooldown between charges
        }

        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        npc.spriteDirection = npc.direction;

        return false;
    }
}