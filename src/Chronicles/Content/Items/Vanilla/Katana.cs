using Chronicles.Content.Items.Weapons.Melee;
using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Items.Vanilla;

public class Katana : VanillaItem {
    public override object ItemTypes => ItemID.Katana;

    private byte combo;

    public override void SetDefaults(Item item) {
        item.damage = 18;
        item.crit = 16;
        item.knockBack = 2f;
        item.shoot = ModContent.ProjectileType<KatanaProj>();
        item.shootSpeed = 1f;
        item.useTime = item.useAnimation = 21;
        item.useStyle = ItemUseStyleID.Swing;
        item.UseSound = SoundID.Item1;
        item.noMelee = true;
        item.noUseGraphic = true;
        item.autoReuse = true;
        item.useTurn = false;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (player.altFunctionUse == 2) {
            player.GetModPlayer<KatanaPlayer>().cooldown = 60 + player.itemTimeMax;
            damage *= 3;
        }
        else velocity = velocity.RotatedBy(((combo % 2) * .5f) - .25f);
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, (player.altFunctionUse == 2) ? KatanaProj.LUNGE : combo);
        combo = (byte)(++combo % 4);

        return false;
    }

    public override bool AltFunctionUse(Item item, Player player) => player.GetModPlayer<KatanaPlayer>().cooldown == 0;
}

public class KatanaProj : ChroniclesProjectile {
    public readonly int holdoutDistance = 60;
    public const int LUNGE = 4;

    public int Combo {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public ref float SwingCounter => ref Projectile.ai[1];

    public int DirUnit => (Combo % 2 == 0) ? -1 : 1;

    public int SwingRange => (int)(140 * ((Combo * .4f) + 1));

    public Player Player => Main.player[Projectile.owner];

    public override string Texture => "Terraria/Images/Item_2273";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 4;
        ProjectileID.Sets.TrailingMode[Type] = 4;
    }

    public override void SetDefaults() {
        Projectile.width = Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        Player.heldProj = Projectile.whoAmI;

        if ((Player.itemAnimation > 2) && Player.active && !Player.dead) //Active check
            Projectile.timeLeft = 2;
        if (Combo == LUNGE) {
            LungeAI();
            return;
        }

        var degrees = (float)(SwingCounter / Player.itemAnimationMax) * (SwingRange * DirUnit);
        var startBias = -60;
        var rotation = MathHelper.ToRadians(degrees - (((SwingRange / 2) + startBias) * DirUnit));

        Projectile.Center = Player.Center + (Vector2.Normalize(Projectile.velocity) * (float)(holdoutDistance * Projectile.scale)).RotatedBy(rotation);
        Projectile.rotation = Player.AngleTo(Projectile.Center);

        Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.rotation);
        Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.rotation);

        SwingCounter = MathHelper.Lerp(SwingCounter, Player.itemAnimationMax, Player.GetTotalAttackSpeed(DamageClass.Melee) / 5f);

        if (SwingCounter >= (Player.itemAnimationMax - 2))
            Projectile.scale -= .0025f;
        else {
            for (var i = 0; i < 3; i++) {
                var dist = Main.rand.NextFloat();
                var pos = Vector2.Lerp(Player.Center, Projectile.Center, dist) + (Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f));
                var scale = dist * ((float)Player.itemAnimation / Player.itemAnimationMax);

                var dust = Dust.NewDustPerfect(pos, Main.rand.NextBool() ? DustID.Smoke : DustID.SilverFlame, Vector2.Zero, 180, default, scale);
                dust.scale *= (dust.type == DustID.Smoke) ? 3f : 1.5f;
                dust.velocity = Vector2.UnitY.RotatedBy(Projectile.rotation);
                dust.noLightEmittence = true;
                dust.noGravity = true;
            }
        }
    }

    private void LungeAI() {
        if (Projectile.numUpdates == 0 && ++SwingCounter < 6) {
            if (SwingCounter == 1)
                SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, Projectile.Center);

            Player.GetModPlayer<KatanaPlayer>().freeDodge = true;
            Player.velocity = Vector2.Normalize(Projectile.velocity) * 45;
            var endPos = Player.Center + Player.velocity;

            for (var i = 0; i < (int)(Player.Center.Distance(endPos) / 5); i++) {
                var dustPos = Vector2.Lerp(Player.Center, endPos, Main.rand.NextFloat());
                var dust = Dust.NewDustPerfect(dustPos + (Main.rand.NextVector2Unit() * Main.rand.NextFloat(30)), Main.rand.NextBool() ? DustID.Smoke : DustID.SilverFlame, Vector2.Normalize(Projectile.velocity) * Main.rand.NextFloat(1f, 3f), 200, default);
                dust.noGravity = true;
                dust.scale = (dust.type == DustID.Smoke) ? Main.rand.NextFloat(2f, 3f) : .75f;
                dust.noLightEmittence = true;
            }
        }
        else if (SwingCounter == 6) {
            Player.GetModPlayer<KatanaPlayer>().freeDodge = false;
            Player.velocity *= .05f;
        }

        Projectile.Center = Player.Center + (Vector2.Normalize(Projectile.velocity) * (float)((holdoutDistance - SwingCounter / 2) * Projectile.scale));
        Projectile.rotation = Player.AngleTo(Projectile.Center);

        Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.rotation);
        Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.rotation);
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        var collisionPoint = 0f;
        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Player.Center, Projectile.Center, 30, ref collisionPoint))
            return true;
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Combo == LUNGE) {
            SoundEngine.PlaySound(SoundID.NPCHit25 with { Pitch = 1f }, target.Center);
            if (target.life <= 0) {
                Player.GetModPlayer<KatanaPlayer>().cooldown = 15;
                SoundEngine.PlaySound(SoundID.NPCDeath11, target.Center);
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        lightColor = Lighting.GetColor((int)(Player.Center.X / 16), (int)(Player.Center.Y / 16));

        var texture = TextureAssets.Projectile[Type].Value;
        var effects = ((Projectile.spriteDirection == -1) || (DirUnit == -1)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var rotation = Projectile.rotation + ((effects == SpriteEffects.None) ? 0.785f : 2.355f);
        var origin = (effects == SpriteEffects.FlipHorizontally) ? Projectile.Size / 2 : new Vector2(texture.Width - (Projectile.width / 2), Projectile.height / 2);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, effects, 0);

        for (var i = 0; i < Projectile.oldPos.Length; i++) {
            var drawPos = Projectile.oldPos[i] - Main.screenPosition + origin + new Vector2(0, Projectile.gfxOffY);

            if (effects == SpriteEffects.None)
                drawPos.X -= texture.Width - Projectile.width;

            var color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length) * .5f;
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[i] + (rotation - Projectile.rotation), origin, Projectile.scale, effects, 0);
        }
        if (Combo != LUNGE)
            DrawSmear();

        return false;
    }

    public virtual void DrawSmear() {
        var texture = TextureAssets.Projectile[985].Value;
        var motionUnit = MathHelper.Max(((float)Player.itemAnimation / Player.itemAnimationMax) - .7f, 0);
        var frame = texture.Frame(1, 4, 0, 3 - (int)(motionUnit * 8f));
        var effects = (Projectile.spriteDirection == -1 || DirUnit == -1) ? SpriteEffects.FlipVertically : SpriteEffects.None;

        var degrees = (float)(SwingCounter / Player.itemAnimationMax) * (SwingRange * DirUnit);
        var rotUnit = Projectile.velocity.ToRotation() + MathHelper.ToRadians(degrees - ((SwingRange / 2) * DirUnit));
        var drawpos = Player.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);

        var opacity = .5f;
        for (var i = 0; i < 3; i++) {
            var rotation = rotUnit - ((i * .15f) * (effects == SpriteEffects.FlipVertically ? -1 : 1));
            var color = Projectile.GetAlpha(Color.LightGray with { A = 0 }) * opacity * motionUnit;
            opacity -= .15f;

            Main.EntitySpriteDraw(texture, drawpos, frame,
                color, rotation, frame.Size() / 2, new Vector2(.75f, 1f) * 1.1f, effects, 0);
        }

        var sparkle = TextureAssets.Projectile[79].Value;
        Main.EntitySpriteDraw(sparkle, drawpos + (Vector2.Normalize(Projectile.velocity) * 70f),
            null, Projectile.GetAlpha((Color.White with { A = 0 }) * .2f), 0, sparkle.Size() / 2, motionUnit * 2f, SpriteEffects.None, 0);
    }
}

public class KatanaPlayer : ModPlayer {
    public int cooldown;
    public bool freeDodge;

    public override void ResetEffects() => cooldown = Math.Max(cooldown - 1, 0);

    public override bool FreeDodge(Player.HurtInfo info) => freeDodge;
}