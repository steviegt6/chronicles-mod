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
        if (player.ownedProjectileCounts[Item.shoot] < 1 && Item.GetGlobalItem<CrossbowGItem>().Loaded)
            Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, Item.shoot, 0, 0, player.whoAmI, Item.GetGlobalItem<CrossbowGItem>().projType, IronCrossbowProj.LOADED);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (!Item.GetGlobalItem<CrossbowGItem>().Loaded) { 
            player.itemTime = player.itemAnimation = player.itemTimeMax = player.itemAnimationMax = loadTime;

            if (player.ownedProjectileCounts[Item.shoot] < 1)
                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, Item.shoot, 0, 0, player.whoAmI, type);
            return false;
        } //The player is loading a bolt; we don't want to fire a projectile here
        return true;
    }

    public override bool? CanChooseAmmo(Item ammo, Player player) => Item.GetGlobalItem<CrossbowGItem>().Loaded ? (ammo.shoot == Item.GetGlobalItem<CrossbowGItem>().projType) : null;

    public override bool CanConsumeAmmo(Item ammo, Player player) => !Item.GetGlobalItem<CrossbowGItem>().Loaded;

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!Item.GetGlobalItem<CrossbowGItem>().Loaded)
            return;

        var sparkle = Mod.Assets.Request<Texture2D>("Assets/Misc/Sparkle").Value;
        spriteBatch.Draw(sparkle, position + new Vector2(20, 0) * scale, null, Color.White with { A = 0 }, 
            (float)Main.timeForVisualEffects / 50f, sparkle.Size() / 2, scale * 1.5f, SpriteEffects.None, 0);
    }
}

public class IronCrossbowProj : ChroniclesProjectile {
    private Item? parent;

    public int ShotIndex { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }

    public ref float AIState => ref Projectile.ai[1];
    internal const int LOADING = 0;
    internal const int LOADED = 1;
    internal const int FIRING = 2;

    public Player Player => Main.player[Projectile.owner];

    private static Item HeldItem(Player player) => (Main.mouseItem == null || Main.mouseItem.IsAir) ? player.HeldItem : Main.mouseItem;

    public override void SetStaticDefaults() => Main.projFrames[Type] = 5;

    public override void SetDefaults() {
        Projectile.width = Projectile.height = 10;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.hostile = false;
    }

    public override void AI() {
        if (Projectile.timeLeft > 2) //The projectile has just spawned in
            parent = Player.HeldItem;

        if (AIState != FIRING) { //Aim towards the cursor
            if (Player.whoAmI == Main.myPlayer) {
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
        Player.heldProj = Projectile.whoAmI;

        if (Player.ItemAnimationActive && Player.ItemAnimationEndingOrEnded) {
            HeldItem(Player).GetGlobalItem<CrossbowGItem>().projType = (HeldItem(Player).GetGlobalItem<CrossbowGItem>().projType == -1) ? ShotIndex : -1;

            if (!HeldItem(Player).GetGlobalItem<CrossbowGItem>().Loaded)
                Projectile.active = false;
        }

        if (Player.HeldItem == parent && Player.active && !Player.dead) //Active check
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
            var boltTexture = TextureAssets.Projectile[ShotIndex].Value;

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

public class CrossbowGItem : GlobalItem {
    public int projType = -1;

    public bool Loaded => projType != -1;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) 
        => (entity.ModItem != null) && (entity.ModItem.Mod == Mod) && entity.useAmmo != AmmoID.None;
}