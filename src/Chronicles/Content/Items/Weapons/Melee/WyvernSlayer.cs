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

namespace Chronicles.Content.Items.Weapons.Melee;

public class WyvernSlayer : ChroniclesItem {
    private bool reverseSwing;

    public override void SetDefaults() {
        Item.DamageType = DamageClass.Melee;
        Item.damage = 180;
        Item.crit = 11;
        Item.knockBack = 11f;
        Item.shoot = ModContent.ProjectileType<WyvernSlayerProj>();
        Item.shootSpeed = 2f;
        Item.width = Item.height = 24;
        Item.useTime = Item.useAnimation = 42;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.channel = true;
        Item.rare = ItemRarityID.Green;
    }

    public override ModItem Clone(Item newEntity) {
        var clone = (WyvernSlayer)NewInstance(newEntity);
        clone.reverseSwing = reverseSwing;

        return clone;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, reverseSwing ? -1 : 1);
        reverseSwing = !reverseSwing;

        return false;
    }
}

public class WyvernSlayerProj : GoldenGreatswordProj {
    public override string Texture => "Chronicles/Assets/Items/Weapons/Melee/WyvernSlayer";

    public override void AI() {
        Player.heldProj = Projectile.whoAmI;

        var degrees = (float)(SwingCounter / Player.itemAnimationMax) * (swingRange * DirUnit);
        var rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(degrees - ((swingRange / 2) * DirUnit));

        Projectile.Center = Player.Center + (Vector2.UnitX * (float)(holdoutDistance * Projectile.scale)).RotatedBy(rotation);
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

                SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                SwingCounter = 0;
                Projectile.damage = (int)(Projectile.damage * (float)MathHelper.Clamp((float)Charge / MaxCharge, .1f, 1f));

                released = true;
            }
        }
        else {
            SwingCounter = MathHelper.Lerp(SwingCounter, Player.itemAnimationMax, Player.GetTotalAttackSpeed(DamageClass.Melee) / 18f);

            if (SwingCounter >= (Player.itemAnimationMax - 2))
                Projectile.scale -= .0025f;
            else {
                var dist = Main.rand.NextFloat();
                var pos = Vector2.Lerp(Player.Center, Projectile.Center, dist) + (Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f));
                var scale = dist * 2f * ((float)Player.itemAnimation / Player.itemAnimationMax);

                var dust = Dust.NewDustPerfect(pos, DustID.SilverFlame, Vector2.Zero, 100, default, scale);
                dust.velocity = Vector2.UnitY.RotatedBy(Projectile.rotation);
                dust.noLightEmittence = true;
                dust.noGravity = true;
            }
        }

        if ((Player.itemAnimation > 2) && Player.active && !Player.dead) //Active check
            Projectile.timeLeft = 2;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, target.Center);

        if (Charge >= MaxCharge && target.knockBackResist > 0f) {
            //Attach this projectile to target, which handles launch logic
            Projectile.NewProjectile(target.GetSource_OnHurt(Projectile), target.Center, Vector2.Zero, ModContent.ProjectileType<Fling>(), (int)(Projectile.damage * .5f), 0, Player.whoAmI, target.whoAmI, target.rotation);
            target.velocity *= 2f;

            for (var i = 0; i < 25; i++)
                Dust.NewDustPerfect(target.Center + (Main.rand.NextVector2Unit() * Main.rand.NextFloat(10)), Main.rand.NextBool() ? DustID.Smoke : DustID.SilverFlame, (target.velocity * Main.rand.NextFloat(.25f, .5f)).RotatedByRandom(.5f), 150, default, Main.rand.NextFloat(1f, 3f)).noGravity = true;
        }
    }

    public override void DrawSmear() {
        var texture = TextureAssets.Projectile[985].Value;
        var motionUnit = MathHelper.Max(((float)Player.itemAnimation / Player.itemAnimationMax) - .75f, 0);
        var frame = texture.Frame(1, 4, 0, 3 - (int)(motionUnit * 16f));
        var effects = (Projectile.spriteDirection == -1 || DirUnit == -1) ? SpriteEffects.FlipVertically : SpriteEffects.None;

        var degrees = (float)(SwingCounter / Player.itemAnimationMax) * (swingRange * DirUnit);
        var rotUnit = Projectile.velocity.ToRotation() + MathHelper.ToRadians(degrees - ((swingRange / 2) * DirUnit));

        float opacity = 1;
        for (var i = 0; i < 3; i++) {
            var rotation = rotUnit - ((i * .3f) * (effects == SpriteEffects.FlipVertically ? -1 : 1));
            var color = Projectile.GetAlpha(Color.LightSteelBlue with { A = 0 }) * opacity * motionUnit;
            opacity -= .3f;

            Main.EntitySpriteDraw(texture, Player.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), frame,
                color, rotation, frame.Size() / 2, 1.6f, effects, 0);
        }
    }
}

public class Fling : ChroniclesProjectile {
    public int ParentIndex {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public ref float OldRotation => ref Projectile.ai[1];

    public bool DealDamage { get => Projectile.ai[2] == 1; set => Projectile.ai[2] = value ? 1 : 0; }

    public override string Texture => "Chronicles/Assets/Items/Weapons/Melee/WyvernSlayer";

    public override void SetDefaults() {
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.alpha = 255;
        Projectile.timeLeft = 120;
        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (Main.npc[ParentIndex] is NPC npc && npc.active) {
            Projectile.Center = npc.Center;
            Projectile.Size = npc.Size;
            npc.rotation += npc.velocity.X * .04f;

            var tileCollided = (npc.collideX && (int)npc.velocity.X == 0) || (npc.collideY && (int)npc.velocity.Y == 0);
            if (tileCollided) {
                DealDamage = true;
                Projectile.timeLeft = Math.Min(Projectile.timeLeft, 2);
            }
        }
        else Projectile.Kill();
    }

    public override void OnKill(int timeLeft) {
        if (Main.npc[ParentIndex] is NPC npc)
            npc.rotation = OldRotation;
    }

    public override bool? CanHitNPC(NPC target) {
        if (ParentIndex == target.whoAmI && !DealDamage)
            return false;
        return null;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        DealDamage = true;
        Projectile.timeLeft = Math.Min(Projectile.timeLeft, 2);
    }

    public override bool? CanCutTiles() => false;
}