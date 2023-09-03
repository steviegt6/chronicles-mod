using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using Terraria.Audio;
using Chronicles.Core.ModLoader;

namespace Chronicles.Content.Items.Weapons.Ranged;

public class IronCrossbow : ChroniclesItem {
    private readonly int loadTime = 75;

    public override void SetDefaults() {
        Item.DamageType = DamageClass.Ranged;
        Item.damage = 20;
        Item.crit = 46;
        Item.knockBack = 7;
        Item.shoot = ModContent.ProjectileType<IronCrossbowProj>();
        Item.useAmmo = AmmoID.Arrow;
        Item.shootSpeed = 12f;
        Item.width = Item.height = 24;
        Item.useTime = Item.useAnimation = 40;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = false;
        Item.rare = ItemRarityID.Blue;
    }

    public override void HoldItem(Player player) {
        if (player.ownedProjectileCounts[Item.shoot] < 1 && player.GetModPlayer<CrossbowPlayer>().loaded)
            Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, Item.shoot, 0, 0, player.whoAmI, IronCrossbowProj.LOADED);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (!player.GetModPlayer<CrossbowPlayer>().loaded) { //The player is loading a bolt; we don't want to fire a projectile here
            player.itemTime = player.itemAnimation = loadTime;
            player.itemTimeMax = player.itemAnimationMax = loadTime;

            if (player.ownedProjectileCounts[Item.shoot] < 1)
                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, Item.shoot, 0, 0, player.whoAmI);

            return false;
        }

        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }

    public override bool CanConsumeAmmo(Item ammo, Player player) => !player.GetModPlayer<CrossbowPlayer>().loaded;
}

public class IronCrossbowProj : ChroniclesProjectile {
    private int AIState {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    internal const int LOADING = 0;
    internal const int LOADED = 1;
    internal const int FIRING = 2;

    protected virtual int ParentItem => ModContent.ItemType<IronCrossbow>();
    private Player Player => Main.player[Projectile.owner];

    public override void SetStaticDefaults() => Main.projFrames[Type] = 5;

    public override void SetDefaults() {
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.width = Projectile.height = 10;
    }

    public override void AI() {
        Player.heldProj = Projectile.whoAmI;

        if (AIState != FIRING) { //Aim towards the cursor
            if (Main.LocalPlayer == Player) {
                Projectile.velocity = Player.DirectionTo(Main.MouseWorld);

                Projectile.netUpdate = true;
            }
            Player.ChangeDir(Math.Sign(Projectile.velocity.X));
        }

        float offset;
        if (AIState == LOADING) {
            offset = (float)((float)Player.itemAnimation / Player.itemAnimationMax) * 6f;

            var quoteant = (float)Player.itemAnimation / Player.itemAnimationMax;
            Projectile.frame = (int)((1f - quoteant) * Main.projFrames[Type]);

            if (Player.ItemAnimationEndingOrEnded) {
                SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack, Projectile.position);
                AIState = LOADED;
            }
        }
        else {
            offset = MathHelper.Max(0, Player.itemAnimation - (Player.itemAnimationMax - 10)) * 2.5f;

            if (AIState != FIRING)
                Projectile.frame = Main.projFrames[Type] - 1;

            if (Player.ItemAnimationJustStarted) {
                Projectile.frame = 0;

                for (var i = 0; i < 20; i++) {
                    if (i < 8)
                        Dust.NewDustPerfect(Projectile.Center, DustID.TreasureSparkle, (Projectile.velocity * Main.rand.NextFloat(0.5f, 7.0f)).RotatedByRandom(.22f), 80, default, Main.rand.NextFloat(0.5f, 1.8f)).noGravity = true;
                    else
                        Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-40.0f, 0.0f), Main.rand.NextFloat(-15.0f, 15.0f)).RotatedBy(Projectile.velocity.ToRotation()), DustID.Smoke, -(Projectile.velocity * Main.rand.NextFloat(0.0f, 1.5f)), 180, default, Main.rand.NextFloat(0.5f, 1.5f)).noGravity = true;
                }

                SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, Projectile.position);
                AIState = FIRING;
            }
        }

        Projectile.rotation = Projectile.velocity.ToRotation() + ((Player.direction == -1) ? MathHelper.Pi : 0);
        Projectile.Center = Player.Center + (Projectile.velocity * (46 - offset)) - (Vector2.UnitY * 4).RotatedBy(MathHelper.WrapAngle(Projectile.rotation));

        Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - 1.57f);
        Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - 1.57f);

        if (Player.ItemAnimationActive && Player.ItemAnimationEndingOrEnded)
            if ((Player.GetModPlayer<CrossbowPlayer>().loaded = !Player.GetModPlayer<CrossbowPlayer>().loaded) == false)
                Projectile.active = false;

        if (Player.HeldItem.type == ParentItem && Player.active && !Player.dead) //Active check
            Projectile.timeLeft = 2;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var effects = (Main.player[Projectile.owner].direction == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var drawFrame = new Rectangle(0, texture.Height / Main.projFrames[Type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Type]);
        var origin = new Vector2((effects == SpriteEffects.FlipHorizontally) ? 0 : drawFrame.Width, drawFrame.Height / 2);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), drawFrame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects);

        if (AIState != FIRING) {
            var quoteant = (float)Player.itemAnimation / Player.itemAnimationMax;
            var offset = new Vector2(46 - (quoteant * 20), 4 * Player.direction).RotatedBy(Projectile.velocity.ToRotation());
            var drawPos = Projectile.Center + new Vector2(0, Projectile.gfxOffY) - offset;

            Player.PickAmmo(Player.HeldItem, out var projType, out _, out _, out _, out _, true);
            var boltTexture = TextureAssets.Projectile[projType].Value;

            Main.EntitySpriteDraw(boltTexture, drawPos - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * (1f - quoteant), Projectile.velocity.ToRotation() + 1.57f, new Vector2(boltTexture.Width / 2, boltTexture.Height), Projectile.scale, effects);

            if (AIState == LOADED) {
                var sparkle = Mod.Assets.Request<Texture2D>("Assets/Misc/Sparkle").Value;
                var num = (float)((Main.timeForVisualEffects / 30f) % 3.14);
                var scale = Math.Max(0, num.ToRotationVector2().Y);

                Main.EntitySpriteDraw(sparkle, drawPos + (Vector2.UnitX * boltTexture.Height).RotatedBy(Projectile.velocity.ToRotation()) - Main.screenPosition, null, Color.White with { A = 0 }, (float)num, sparkle.Size() / 2, scale, effects);
            }
        }
        return false;
    }

    public override bool? CanDamage() => false;
}

public class CrossbowPlayer : ModPlayer {
    public bool loaded = false; //This should eventually be made instanced between crossbows
}