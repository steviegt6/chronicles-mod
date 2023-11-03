using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using Terraria.DataStructures;
using Terraria.Audio;
using Chronicles.Core.ModLoader;

namespace Chronicles.Content.Items.Weapons.Ranged;

public class WoodenShortbow : ChroniclesItem {
    public override void SetDefaults() {
        Item.DamageType = DamageClass.Ranged;
        Item.damage = 4;
        Item.knockBack = 2;
        Item.shoot = ModContent.ProjectileType<WoodenShortbowProj>();
        Item.useAmmo = AmmoID.Arrow;
        Item.shootSpeed = 10f;
        Item.width = Item.height = 24;
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = false;
        Item.rare = ItemRarityID.White;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, player.whoAmI, type);
        return false;
    }

    public override void AddRecipes() {
        var modRecipe = CreateRecipe();
        modRecipe.AddIngredient(ItemID.Wood, 20);
        modRecipe.AddIngredient(ItemID.WhiteString);
        modRecipe.AddTile(TileID.WorkBenches);
        modRecipe.Register();
    }
}

public class WoodenShortbowProj : ChroniclesProjectile {
    private readonly int lingerTime = 20;

    private int ShotType {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private bool Released {
        get => (int)Projectile.ai[1] != 0;
        set => Projectile.ai[1] = value ? 1 : 0;
    }

    protected virtual int ParentItem => ModContent.ItemType<WoodenShortbow>();
    private Player Player => Main.player[Projectile.owner];

    public override void SetStaticDefaults() => Main.projFrames[Type] = 4;

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
        var quoteant = Math.Max(0, (float)Player.itemAnimation / Player.itemAnimationMax);

        if (Player.channel && !Released) {
            if (Main.LocalPlayer == Player) { //Aim towards the cursor
                Projectile.velocity = Player.DirectionTo(Main.MouseWorld);

                Projectile.netUpdate = true;
            }
            Player.ChangeDir(Math.Sign(Projectile.velocity.X));

            if ((Player.itemAnimation | Player.itemTime) < 2)
                Player.itemAnimation = Player.itemTime = 2;

            Projectile.frame = (int)((1f - quoteant) * (Main.projFrames[Type] - 1));
        }
        else {
            if (!Released) { //One-time fire effects
                var magnitude = Player.HeldItem.shootSpeed;
                Player.itemAnimation = Player.itemTime = lingerTime;

                Projectile.NewProjectileDirect(Entity.GetSource_FromAI(), Projectile.Center, Projectile.velocity * magnitude, ShotType, Projectile.damage, Projectile.knockBack, Player.whoAmI).CritChance = (int)((1f - quoteant) * 100f);
                SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
                
                Released = true;
            }

            if (Projectile.frame > 0)
                if (++Projectile.frameCounter > 3)
                    Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
        }

        var fullyCharged = Player.itemAnimation == 2;
        Projectile.rotation = Projectile.velocity.RotatedByRandom(fullyCharged ? 0.1f : 0f).ToRotation() + ((Player.direction == -1) ? MathHelper.Pi : 0);
        Projectile.Center = Player.Center + (Projectile.velocity * (24 + (quoteant * 2f)));

        var dynamicStr = Projectile.frame switch {
            0 => Player.CompositeArmStretchAmount.Full,
            1 => Player.CompositeArmStretchAmount.ThreeQuarters,
            2 => Player.CompositeArmStretchAmount.Quarter,
            _ => Player.CompositeArmStretchAmount.None
        };

        Player.SetCompositeArmFront(true, dynamicStr, Projectile.velocity.ToRotation() - 1.57f);
        Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - 1.57f);

        if (!Player.ItemAnimationEndingOrEnded && Player.HeldItem.type == ParentItem && Player.active && !Player.dead) //Active check
            Projectile.timeLeft = 2;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var effects = (Main.player[Projectile.owner].direction == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var drawFrame = new Rectangle(0, texture.Height / Main.projFrames[Type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Type]);
        var origin = new Vector2((effects == SpriteEffects.FlipHorizontally) ? 0 : drawFrame.Width, drawFrame.Height / 2);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), drawFrame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects);

        if (!Released) {
            var quoteant = (float)Player.itemAnimation / Player.itemAnimationMax;
            var offset = (Vector2.UnitX * (26 - (quoteant * 20))).RotatedBy(Projectile.velocity.ToRotation());
            var drawPos = Projectile.Center + new Vector2(0, Projectile.gfxOffY) - offset;

            var arrowTexture = TextureAssets.Projectile[ShotType].Value;

            Main.EntitySpriteDraw(arrowTexture, drawPos - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * (1f - quoteant), Projectile.velocity.ToRotation() + 1.57f, new Vector2(arrowTexture.Width / 2, arrowTexture.Height), Projectile.scale, effects);

            var sparkleOn = 10;
            if (Player.itemAnimation <= sparkleOn && Player.itemAnimation > 2) {
                var sparkle = Mod.Assets.Request<Texture2D>("Assets/Misc/Sparkle").Value;
                var scale = (float)Player.itemAnimation / sparkleOn;

                Main.EntitySpriteDraw(sparkle, drawPos + (Vector2.UnitX * arrowTexture.Height).RotatedBy(Projectile.velocity.ToRotation()) - Main.screenPosition, null, Color.White with { A = 0 } * (Player.itemAnimation * (float)(255f / sparkleOn)), 0f, sparkle.Size() / 2, scale, effects);
            }
        }
        return false;
    }

    public override bool? CanDamage() => false;
}