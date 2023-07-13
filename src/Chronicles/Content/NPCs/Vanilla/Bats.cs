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
    private int aiState;
    private const int state_scanning = 0;
    private const int state_sleeping = 1;
    private const int state_normal = 2;

    private Vector2? sleepingPos;

    public override object NPCTypes => new int[] { NPCID.CaveBat, NPCID.IceBat, NPCID.JungleBat, NPCID.SporeBat, NPCID.GiantBat, NPCID.IlluminantBat, NPCID.GiantBat, NPCID.Hellbat };

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

        if (aiState == state_scanning) {
            if (sleepingPos.HasValue) {
                npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(sleepingPos.Value) * 5f, .1f);
                npc.noTileCollide = true;

                if (npc.Distance(sleepingPos.Value) < 20) {
                    npc.noTileCollide = false;
                    npc.Center = sleepingPos.Value;
                    aiState = state_sleeping;
                } //Snap to position
            }
            else {
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitY * -4f, .1f);
                scanForSleepingSpot(30);
            }
        }
        if (aiState == state_sleeping) {
            npc.TargetClosest();
            var target = Main.player[npc.target];

            npc.velocity = Vector2.Zero;

            if (npc.Distance(target.Center) < (16 * 5)) {
                SoundEngine.PlaySound(SoundID.NPCDeath4, npc.Center);
                aiState = state_normal;
            }
        }
        SetAlertness(npc, aiState == state_normal);
        npc.noGravity = aiState == (state_scanning | state_sleeping);

        return aiState == state_normal;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) => aiState = state_normal;

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => Alerted;

    public override void FindFrame(NPC npc, int frameHeight) {
        if (aiState == state_sleeping)
            npc.frame.Y = frameHeight * (Main.npcFrameCount[npc.type] - 1);
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (aiState != state_sleeping || npc.Distance(Main.LocalPlayer.Center) > (16 * 12))
            return;

        var elapsed = (int)((float)Main.timeForVisualEffects % 60);
        var pos = npc.position + (Vector2.UnitY.RotatedBy(Math.Sin(elapsed / 15f)) * elapsed);
        var color = Color.White * (float)(1f - (elapsed / 30f)) * .6f;

        spriteBatch.DrawString(FontAssets.MouseText.Value, "z", pos - screenPos, color);
    }
}