using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class GraniteElemental : VanillaNPC {
    private readonly int overshoot = 20;

    private const int STATE_FLY = 0;
    private const int STATE_SLAM = 1;
    private const int STATE_STUN = 2;

    public override object NPCTypes => NPCID.GraniteFlyer;

    public override void SetDefaults(NPC npc) => npc.noTileCollide = false;

    public override bool PreAI(NPC npc) {
        npc.TargetClosest(false);
        var target = Main.player[npc.target];
        var flyToPos = target.Center - (Vector2.UnitY * 200);

        npc.noGravity = true;
        npc.knockBackResist = .2f;

        //Slowly fly towards the target and slam down when in range, stunning for a duration
        if (npc.ai[1] > 0) {
            if (npc.ai[2] < overshoot && (npc.ai[1] != STATE_STUN)) {
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitY * -2f, .05f);
                SoundEngine.PlaySound(SoundID.DD2_BookStaffCast with { Pitch = -.5f, Volume = .7f }, npc.Center);
            } //Overshoot
            else {
                if (npc.ai[1] == STATE_STUN) {
                    if (npc.ai[2] > 200) {
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                    } //Remove from stunned state

                    npc.knockBackResist = 1.8f;
                    npc.velocity.X *= .95f;
                    npc.velocity.Y = MathHelper.Min(npc.velocity.Y + .8f, 10);
                } //Stunned in place
                else if (npc.collideY || npc.velocity.Y == 0) {
                    for (var i = 0; i < 30; i++) {
                        var dust = Dust.NewDustPerfect(npc.getRect().Bottom(), DustID.Electric);
                            
                        if (i < 10) {
                            dust.noGravity = true;
                            dust.fadeIn = 1.2f;
                            dust.velocity = Vector2.UnitX * Main.rand.NextFloatDirection() * 5;
                        }
                        else {
                            dust.velocity = (Vector2.UnitY * Main.rand.NextFloat(-2f, -4f)).RotatedByRandom(1.5f);
                            if (Main.rand.NextBool(3)) {
                                dust.noGravity = true;
                                dust.fadeIn = 1.2f;
                            }
                        }
                    }
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 1f, PitchVariance = .4f }, npc.Center);
                    SoundEngine.PlaySound(SoundID.NPCHit15, npc.Center);

                    npc.ai[1] = 2;
                    npc.ai[2] = 0;
                } //Just collided
                else npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitY * 15f, .5f);
            }
            npc.ai[0] = -1; //This puts the NPC in their defensive animation
        }
        else if (npc.Distance(flyToPos) < 30) {
            npc.velocity.X *= .25f;

            npc.ai[1] = 1;
            npc.ai[2] = 0;
        } //Start slam
        else {
            if (npc.ai[2] > 25)
                npc.ai[0] = 0; //Keep the NPC in their defensive animation briefly after landing

            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(flyToPos) * 4f, .08f); //Try to fly above the target
        }
        npc.ai[2]++;

        return false;
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => npc.ai[1] == STATE_SLAM;

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (npc.ai[1] != STATE_SLAM)
            return;

        var texture = TextureAssets.Npc[npc.type].Value;
        var trailLength = 5;
        for (var i = 0; i < trailLength; i++) {
            var color = (Color.Cyan * (npc.ai[2] / overshoot)) with { A = 0 } * (1f - (i / (float)trailLength));
            spriteBatch.Draw(texture, npc.Center - screenPos + new Vector2(0, npc.gfxOffY - (npc.velocity.Y * i)), npc.frame, color, npc.rotation, npc.frame.Size() / 2, npc.scale, SpriteEffects.None, 0);
        }
    }
}