using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Harpy : VanillaNPC {
    private Vector2? sleepingPos;

    private bool IsSleeping(NPC npc) => npc.ai[0] == 1 && !Alerted;

    public override object NPCTypes => NPCID.Harpy;

    public override bool PreAI(NPC npc) {
        void scanForSleepingSpot(int dist) {
            var origin = (npc.Center / 16).ToPoint();

            for (var i = origin.X - dist; i <= origin.X + dist; i++) {
                for (var j = origin.Y - dist; j <= origin.Y + dist; j++) {
                    if (Collision.CanHitLine(npc.position, npc.width, npc.height, new Vector2(i, j) * 16, 16, 16) && WorldGen.InWorld(i, j) && !Framing.GetTileSafely(i, j).HasTile && Framing.GetTileSafely(i, j + 1).HasTile) {
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

    public override void HitEffect(NPC npc, NPC.HitInfo hit) {
        SetAlertness(npc, true);

        foreach (var otherNPC in Main.npc.Where(x => x.active && x.whoAmI != npc.whoAmI && x.Distance(npc.Center) < (16 * 20) && x.type == NPCID.Harpy))
            otherNPC.GetGlobalNPC<Harpy>().SetAlertness(npc, true);
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => Alerted;

    public override void FindFrame(NPC npc, int frameHeight) {
        if (IsSleeping(npc))
            npc.frame.Y = frameHeight * (Main.npcFrameCount[npc.type] - 1);
    }
}