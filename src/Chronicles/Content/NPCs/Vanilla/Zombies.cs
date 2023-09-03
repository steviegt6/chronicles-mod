using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Zombies : VanillaNPC {
    public override object NPCTypes => new int[] { NPCID.Zombie, NPCID.ZombieMushroom, NPCID.ZombieMushroomHat, NPCID.ZombieEskimo, NPCID.ZombiePixie, NPCID.ZombieRaincoat, NPCID.ZombieSuperman, NPCID.ZombieSweater, NPCID.ZombieXmas, NPCID.ArmedTorchZombie, NPCID.ArmedZombie, NPCID.ArmedZombieCenx, 
        NPCID.ArmedZombieEskimo, NPCID.ArmedZombiePincussion, NPCID.ArmedZombieSlimed, NPCID.ArmedZombieSwamp, NPCID.ArmedZombieTwiggy, NPCID.BaldZombie, NPCID.FemaleZombie, NPCID.MaggotZombie, NPCID.PincushionZombie, NPCID.SlimedZombie, NPCID.SwampZombie, NPCID.TorchZombie, NPCID.TwiggyZombie };

    public override bool PreAI(NPC npc) {
        npc.TargetClosest(Alerted);
        var target = Main.player[npc.target];

        if (Main.IsItDay()) {
            ref var fleeDir = ref npc.ai[0];

            if (fleeDir == 0)
                fleeDir = (target.Center.X < npc.Center.X) ? 1 : -1; //Set an initial fleeing direction based on target position
            if (npc.velocity.X == 0 && npc.collideX)
                fleeDir = -fleeDir; //Switch directions upon hitting a stop

            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.ai[0] * 1.6f, .04f);
            npc.direction = npc.spriteDirection = (int)npc.ai[0];
        }
        else if (Alerted) {
            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.direction * 3f, .05f);

            if (npc.Distance(target.Center) > (16 * 28))
                SetAlertness(npc, false);
        }
        else {
            const float idle_speed = .3f;
            //Stumble around slowly and aimlessly until a player comes close
            if ((Main.rand.NextBool(250) || npc.velocity.X == 0 || Math.Abs(npc.velocity.X) > idle_speed) && Main.netMode != NetmodeID.MultiplayerClient) {
                npc.velocity.X = Main.rand.NextBool(3) ? Main.rand.NextFloat(-idle_speed, idle_speed) : 0;
                npc.netUpdate = true;
            }
            if (npc.Distance(target.Center) < (16 * 18) && Collision.CanHit(npc, target)) {
                SoundEngine.PlaySound(SoundID.ZombieMoan with { Pitch = 1.3f }, npc.Center);
                SetAlertness(npc, true);
            }

            npc.spriteDirection = npc.direction = (npc.velocity.X > 0) ? 1 : -1;
            npc.ai[0] = 0;
        }

        if ((Main.timeForVisualEffects % (600 + Main.rand.Next(300))) == 0)
            SoundEngine.PlaySound(SoundID.ZombieMoan, npc.Center); //Periodically moan

        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        return false;
    }
}