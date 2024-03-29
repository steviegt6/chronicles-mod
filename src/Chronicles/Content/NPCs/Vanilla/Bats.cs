using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Bats : VanillaNPC {
    private Vector2? sleepingPos;

    private bool IsSleeping(NPC npc) => npc.ai[0] == 1 && !Alerted;

    public override object NPCTypes => new int[] { NPCID.CaveBat, NPCID.IceBat, NPCID.Lavabat, NPCID.JungleBat, NPCID.SporeBat, NPCID.GiantFlyingFox, NPCID.IlluminantBat, NPCID.GiantBat, NPCID.Hellbat };

    public override void SetStaticDefaults() {
        //Add additional frames of animation
        Main.npcFrameCount[NPCID.IlluminantBat] = 5;
        Main.npcFrameCount[NPCID.IceBat] = 5;
        Main.npcFrameCount[NPCID.Lavabat] = 5;
        Main.npcFrameCount[NPCID.GiantFlyingFox] = 5;
    }

    public override bool PreAI(NPC npc) {
        void scanForSleepingSpot(int dist) {
            var origin = (npc.Center / 16).ToPoint();

            for (var i = origin.X - dist; i <= origin.X + dist; i++) {
                for (var j = origin.Y - dist; j <= origin.Y + dist; j++) {
                    if (Collision.CanHitLine(npc.position, npc.width, npc.height, new Vector2(i, j) * 16, 16, 16) && WorldGen.InWorld(i, j) && !Framing.GetTileSafely(i, j).HasTile && Framing.GetTileSafely(i, j - 1).HasTile) {
                        sleepingPos = (new Vector2(i, j) * 16) + new Vector2(8);
                    }
                }
            }
        }
        npc.noGravity = !Alerted;

        if (Alerted)
            return true;

        if (!IsSleeping(npc)) {
            if (sleepingPos.HasValue) {
                npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(sleepingPos.Value) * 5f, .1f);
                npc.noTileCollide = true;

                if (npc.Distance(sleepingPos.Value) < 20) {
                    npc.noTileCollide = false;
                    npc.Center = sleepingPos.Value;

                    npc.ai[0] = 1; //Begin to sleep
                } //Snap to position
            }
            else {
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitY * -4f, .1f);
                scanForSleepingSpot(30);
            }
        }
        else {
            npc.TargetClosest();
            var target = Main.player[npc.target];

            npc.velocity = Vector2.Zero;

            if (npc.Distance(target.Center) < (16 * 5)) {
                SoundEngine.PlaySound(SoundID.NPCDeath4, npc.Center);
                SetAlertness(npc, true);
            }
        }
        return false;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) => SetAlertness(npc, true);

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => Alerted;

    public override void FindFrame(NPC npc, int frameHeight) {
        var restingFrame = frameHeight * (Main.npcFrameCount[npc.type] - 1);

        if (IsSleeping(npc))
            npc.frame.Y = restingFrame;
        else if (npc.frame.Y == restingFrame) {
            npc.frameCounter = 0;
            npc.frame.Y = 0;
        }
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!IsSleeping(npc) || npc.Distance(Main.LocalPlayer.Center) > (16 * 12))
            return;

        var elapsed = (int)((float)Main.timeForVisualEffects % 60);
        var pos = npc.position + (Vector2.UnitY.RotatedBy(Math.Sin(elapsed / 15f)) * elapsed);
        var color = Color.White * (float)(1f - (elapsed / 30f)) * .6f;

        spriteBatch.DrawString(FontAssets.MouseText.Value, "z", pos - screenPos, color);
    }
}