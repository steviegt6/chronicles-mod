using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class Shelled : VanillaNPC {
    private int turnCounter = 0;

    private static bool IsTortoise(NPC npc) => new int[] { NPCID.GiantTortoise, NPCID.IceTortoise }.Contains(npc.type);

    private static bool Hiding(NPC npc) {
        npc.TargetClosest();
        var target = Main.player[npc.target];
        float detectRange = IsTortoise(npc) ? (16 * 20) : (16 * 10);

        return npc.Distance(target.Center) < detectRange || npc.velocity.Length() > 4;
    }

    public override object NPCTypes => new int[] { NPCID.GiantShelly, NPCID.GiantShelly2, NPCID.CochinealBeetle, NPCID.CyanBeetle, NPCID.LacBeetle, NPCID.GiantTortoise, NPCID.IceTortoise };

    public override void SetDefaults(NPC npc) => npc.damage = 0;

    public override bool PreAI(NPC npc) {
        npc.ai[0] = Hiding(npc) ? 2 : 0;

        if (Hiding(npc)) {
            npc.frameCounter = 0;
            npc.knockBackResist = 1.75f;
            npc.defense = 120;
            npc.rotation += npc.velocity.X / 12f;

            if (npc.velocity.Y == 0)
                npc.velocity.X *= .95f;

            return false;
        }
        else {
            npc.knockBackResist = .1f;
            npc.defense = npc.defDefense;

            if (npc.velocity == Vector2.Zero && npc.collideX) {
                if (npc.ai[1] == 0) {
                    npc.velocity.Y = -5f; //Jump
                    npc.ai[1] = 1;
                }
                else npc.ai[2] = -npc.ai[2]; //Turn around
            } //On horizontal collision, try jumping once, then turn around if still colliding
            if (npc.velocity.Y == 0 && npc.ai[1] == 1)
                npc.ai[1] = 0;

            const float idle_speed = 1f;

            if (((++turnCounter % (60 * 10) == 0 && Main.rand.NextBool(2)) || Math.Abs(npc.velocity.X) > idle_speed || npc.ai[2] == 0) && Main.netMode != NetmodeID.MultiplayerClient) {
                npc.ai[2] = Main.rand.NextFloat(-idle_speed, idle_speed);
                npc.netUpdate = true;
            }
            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.ai[2], .02f);
        }
        npc.direction = npc.spriteDirection = (npc.velocity.X < 0) ? -1 : 1;
        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

        return false;
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        if (Hiding(npc) && IsTortoise(npc) && projectile.CanBeReflected()) {
            projectile.penetrate++;
            projectile.velocity = -projectile.velocity;
            projectile.friendly = false;
            projectile.hostile = true;

            modifiers.Knockback *= 
                (projectile.DamageType == Terraria.ModLoader.DamageClass.Melee 
                || projectile.DamageType == Terraria.ModLoader.DamageClass.MeleeNoSpeed) 
                ? 1 : 0;
            //Take no knockback from any damage type except for melee

            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, npc.Center);
        } //Reflect the projectile
    }
}