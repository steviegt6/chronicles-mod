using Chronicles.Core;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class SnowFlinx : VanillaNPC,
                     IPackNPC {
    private readonly int cooldownMax = 180;
    private int cooldown;

    private bool IsRolling { get => cooldown == cooldownMax; set => cooldown = value ? cooldownMax : Math.Min(cooldown, cooldownMax - 1); }

    public override object NPCTypes => NPCID.SnowFlinx;

    public int PackSize() => Main.rand.Next(3, 6);

    public override void SetStaticDefaults() => Main.npcFrameCount[NPCID.SnowFlinx] = 13;

    public override bool PreAI(NPC npc) {
        npc.TargetClosest(false);
        var target = Main.player[npc.target];

        var inRange = npc.Distance(target.Center) < (16 * 30);

        if (Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height) && inRange && cooldown == 0) {
            npc.direction = target.Center.X < npc.Center.X ? -1 : 1;
            npc.velocity.Y = -4;
            SoundEngine.PlaySound(SoundID.Run with { Pitch = -.8f, Volume = .5f }, npc.Center);

            IsRolling = true;
        }
        npc.knockBackResist = IsRolling ? 1.25f : .8f;

        if (IsRolling) {
            if (npc.velocity.X == 0 && npc.collideX) {
                npc.velocity = new Vector2(npc.direction * -2.5f, -4);
                SoundEngine.PlaySound(SoundID.NPCHit11, npc.Center);

                IsRolling = false;
            }
            if (!inRange)
                IsRolling = false;

            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.direction * 10f, .025f);

            npc.rotation += npc.velocity.X * .1f;
            npc.frameCounter = 0;

            return false;
        }
        else {
            cooldown = Math.Max(cooldown - 1, 0);
            return true;
        }
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) => IsRolling = false;

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) => spawnRate = 2;

    public override void FindFrame(NPC npc, int frameHeight) {
        var rollingFrame = frameHeight * (Main.npcFrameCount[npc.type] - 1);

        if (IsRolling)
            npc.frame.Y = rollingFrame;
        else if (npc.frame.Y == rollingFrame) {
            npc.frameCounter = 0;
            npc.frame.Y = 0;
        }
    }
}