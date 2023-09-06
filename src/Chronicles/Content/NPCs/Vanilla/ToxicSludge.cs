using Chronicles.Content.Projectiles.Hostile;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.NPCs.Vanilla;

public class ToxicSludge : VanillaNPC {
    private static bool IsDying(NPC npc) => npc.life == 1;

    public override object NPCTypes => NPCID.ToxicSludge;

    public override bool PreAI(NPC npc) {
        if (IsDying(npc)) {
            npc.velocity.X *= .9f;
            npc.rotation = (float)Math.Sin(npc.ai[1] * 20) / 10;
            npc.scale = 1f + (npc.ai[1] / 4);

            if ((npc.ai[1] += 1f / 100) >= 1) {
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    npc.StrikeInstantKill();

                    for (var i = 0; i < 20; i++) {
                        var dust = Dust.NewDustPerfect(npc.Center, DustID.Poisoned, Main.rand.NextVector2Unit() * Main.rand.NextFloat() * 5f, 0, default, 3f);
                        dust.noGravity = true;
                        dust.fadeIn = 2f;

                        if (i < 5)
                            Projectile.NewProjectile(npc.GetSource_Death(), npc.Center, (Vector2.UnitY * -Main.rand.NextFloat(4f, 10f)).RotatedByRandom(2f), ModContent.ProjectileType<ToxicChunk>(), npc.damage, 0);
                    }
                }
            }
            return false;
        }
        return true;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) {
        if (npc.ai[1] < 1)
            npc.life = Math.Max(npc.life, 1);
    }

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item) => IsDying(npc) ? false : null;

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => IsDying(npc) ? false : null;

    public override bool CanBeHitByNPC(NPC npc, NPC attacker) => !IsDying(npc);

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => !IsDying(npc);

    public override bool CanHitNPC(NPC npc, NPC target) => !IsDying(npc);

    public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position) => IsDying(npc) ? false : null;

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (IsDying(npc)) {
            var texture = TextureAssets.Npc[npc.type].Value;
            var effects = (npc.spriteDirection == -1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            var pos = npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY);
            var lerp = (float)Math.Sin(npc.ai[1] * 50) * .2f;
            var scale = new Vector2(1 + lerp, 1 - lerp) * npc.scale;

            Main.EntitySpriteDraw(texture, pos, npc.frame, npc.GetAlpha(drawColor), npc.rotation, npc.frame.Size() / 2, scale, effects, 0);
            return false;
        }
        return true;
    }
}