using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Items.Weapons.Melee;

public class GoldenGreatsword : ChroniclesItem {
    private bool reverseSwing;

    public override void SetDefaults() {
        Item.DamageType = DamageClass.Melee;
        Item.damage = 120;
        Item.crit = 11;
        Item.knockBack = 4.5f;
        Item.shoot = ModContent.ProjectileType<GoldenGreatswordProj>();
        Item.shootSpeed = 2f;
        Item.width = Item.height = 24;
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.channel = true;
        Item.rare = ItemRarityID.Green;
    }

    public override ModItem Clone(Item newEntity) {
        var clone = (GoldenGreatsword)NewInstance(newEntity);
        clone.reverseSwing = reverseSwing;

        return clone;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, reverseSwing ? -1 : 1);
        reverseSwing = !reverseSwing;

        return false;
    }
}

public class GoldenGreatswordProj : ChroniclesProjectile {
    public readonly int swingRange = 300;
    public readonly int holdoutDistance = 110;

    public bool released = false;

    public int DirUnit {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public ref float SwingCounter => ref Projectile.ai[1];

    public ref float Charge => ref Projectile.ai[2];

    public int MaxCharge => Player.itemAnimationMax;

    public Player Player => Main.player[Projectile.owner];

    public override string Texture => "Chronicles/Assets/Items/Weapons/Melee/GoldenGreatsword";

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

        var degrees = (float)(SwingCounter / Player.itemAnimationMax) * (swingRange * DirUnit);
        var rotation = MathHelper.ToRadians(degrees - ((swingRange / 2) * DirUnit));

        Projectile.Center = Player.Center + (Vector2.Normalize(Projectile.velocity) * (float)(holdoutDistance * Projectile.scale)).RotatedBy(rotation);
        Projectile.rotation = Player.AngleTo(Projectile.Center);

        Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.rotation);
        Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.rotation);

        if (!released) {
            Player.itemTime = Player.itemTimeMax;
            Player.itemAnimation = Player.itemAnimationMax;

            if ((++Charge + 1) == MaxCharge && Player.whoAmI == Main.myPlayer)
                SoundEngine.PlaySound(SoundID.MaxMana);
            if (Charge < MaxCharge)
                SwingCounter -= .05f;

            if (!Player.channel) {
                if (Player.whoAmI == Main.myPlayer) {
                    Projectile.velocity = Player.DirectionTo(Main.MouseWorld);
                    Projectile.netUpdate = true;
                }
                Player.ChangeDir(Math.Sign(Projectile.velocity.X));

                SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown with { Pitch = .5f}, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                SwingCounter = 0;
                Projectile.damage = (int)(Projectile.damage * (float)MathHelper.Clamp((float)Charge / MaxCharge, .1f, 1f));

                released = true;
            }
        }
        else {
            SwingCounter = MathHelper.Lerp(SwingCounter, Player.itemAnimationMax, Player.GetTotalAttackSpeed(DamageClass.Melee) / 12f);

            if (SwingCounter >= (Player.itemAnimationMax - 2))
                Projectile.scale -= .005f;
            else {
                var dist = Main.rand.NextFloat();
                var pos = Vector2.Lerp(Player.Center, Projectile.Center, dist) + (Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f));
                var scale = dist * 2f * ((float)Player.itemAnimation / Player.itemAnimationMax);

                var dust = Dust.NewDustPerfect(pos, DustID.GoldFlame, Vector2.Zero, 100, default, scale);
                dust.velocity = Vector2.UnitY.RotatedBy(Projectile.rotation);
                dust.noLightEmittence = true;
                dust.noGravity = true;
            }

            if (Charge >= MaxCharge)
                TryDeflect();
        }

        if ((Player.itemAnimation > 2) && Player.active && !Player.dead) //Active check
            Projectile.timeLeft = 2;
    }

    private void TryDeflect() {
        var collisionPoint = 0f;
        bool colliding(Projectile x) => Collision.CheckAABBvLineCollision(x.getRect().TopLeft(), x.getRect().Size(), Player.Center, Projectile.Center, 30, ref collisionPoint);

        var projectiles = Main.projectile.Where(x => x.active && x.whoAmI != Projectile.whoAmI && x.hostile && colliding(x));
        var playedSound = false;

        foreach (var proj in projectiles) {
            proj.velocity = Player.DirectionTo(proj.Center) * proj.velocity.Length();
            proj.friendly = true;
            proj.hostile = false;

            if (!playedSound) {
                SoundEngine.PlaySound(SoundID.DrumFloorTom with { Pitch = .5f, PitchVariance = .5f }, Projectile.Center);
                playedSound = true;
            }
        }
    }

    public override bool? CanDamage() => released ? null : false;

    public override bool ShouldUpdatePosition() => false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        var collisionPoint = 0f;
        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Player.Center, Projectile.Center, 30, ref collisionPoint))
            return true;
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, target.Center);

    public override bool PreDraw(ref Color lightColor) {
        lightColor = Lighting.GetColor((int)(Player.Center.X / 16), (int)(Player.Center.Y / 16));

        var texture = TextureAssets.Projectile[Type].Value;
        var effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
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
        if (released)
            DrawSmear();
        else if (Charge >= MaxCharge)
            DrawSparkle();

        return false;
    }

    public virtual void DrawSmear() {
        Main.instance.LoadProjectile(985);
        var texture = TextureAssets.Projectile[985].Value;
        var motionUnit = MathHelper.Max(((float)Player.itemAnimation / Player.itemAnimationMax) - .75f, 0);
        var frame = texture.Frame(1, 4, 0, 3 - (int)(motionUnit * 16f));
        var effects = (Projectile.spriteDirection == -1 || DirUnit == -1) ? SpriteEffects.FlipVertically : SpriteEffects.None;

        var degrees = (float)(SwingCounter / Player.itemAnimationMax) * (swingRange * DirUnit);
        var rotUnit = Projectile.velocity.ToRotation() + MathHelper.ToRadians(degrees - ((swingRange / 2) * DirUnit));

        float opacity = 1;
        for (var i = 0; i < 3; i++) {
            var rotation = rotUnit - ((i * .3f) * (effects == SpriteEffects.FlipVertically ? -1 : 1));
            var color = Projectile.GetAlpha(Color.Goldenrod with { A = 0 }) * opacity * motionUnit;
            opacity -= .3f;

            Main.EntitySpriteDraw(texture, Player.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), 
                frame, color, rotation, frame.Size() / 2, 1.6f, effects, 0);
        }
    }

    public virtual void DrawSparkle() {
        var texture = TextureAssets.Projectile[79].Value;
        var scalar = MathHelper.Max((10f - Math.Abs(MaxCharge - Charge)) * .5f, 0);
        var pos = Projectile.Center + (Player.DirectionTo(Projectile.Center) * 20f);

        Main.EntitySpriteDraw(texture, pos - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, 
            Projectile.GetAlpha(Color.White with { A = 0 }), 0, texture.Size() / 2, scalar * .1f, SpriteEffects.None, 0);
    }
}