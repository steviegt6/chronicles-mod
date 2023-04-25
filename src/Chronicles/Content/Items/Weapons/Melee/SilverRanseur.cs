using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chronicles.Content.Items.Weapons.Melee;

public class SilverRanseur : ModItem {
    public override string Texture => "Chronicles/Assets/Items/Weapons/Melee/SilverRanseur";

    public override void SetDefaults() {
        Item.DamageType = DamageClass.Melee;
        Item.damage = 15;
        Item.crit = 4;
        Item.knockBack = 1.8f;
        Item.shoot = ModContent.ProjectileType<SilverRanseurProj>();
        Item.shootSpeed = 2f;
        Item.width = Item.height = 24;
        Item.useTime = Item.useAnimation = 22;
        Item.useStyle = ItemUseStyleID.Rapier;
        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item1;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        => velocity = velocity.RotatedByRandom(.5f);
}

public class SilverRanseurProj : ModProjectile {
    private int HalfTime => Player.itemAnimationMax / 2;
    private Player Player => Main.player[Projectile.owner];

    public override string Texture => "Chronicles/Assets/Items/Weapons/Melee/SilverRanseurProj";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults() {
        Projectile.width = Projectile.height = 18;
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
        Player.ChangeDir(Projectile.direction);
        Projectile.rotation = Projectile.velocity.ToRotation();

        Player.heldProj = Projectile.whoAmI;
        Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - 1.57f);

        if (Player.itemAnimation <= HalfTime) {
            Projectile.alpha += 15;
            Projectile.scale -= 0.025f;
        }
        else if (Player.itemAnimation > (HalfTime * 1.5f)) {
            for (var i = 0; i < 2; i++) {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SilverFlame, 0f, 0f, 160, default, 1.1f);
                dust.velocity = Vector2.Normalize(Projectile.velocity) * 2f;
                dust.noGravity = true;
            }
        }

        var lungeLength = 54;
        var desiredVel = Vector2.Normalize(Projectile.velocity) * (lungeLength * ((Player.itemAnimation < HalfTime) ? 0.5f : 1f));
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.15f);
        Projectile.Center = Player.Center + Projectile.velocity + (Player.HeldItem.ModItem.HoldoutOffset() ?? Vector2.Zero);

        if (Player.itemAnimation > 2 && Player.active && !Player.dead) //Active check
            Projectile.timeLeft = 2;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var effects = (Projectile.direction == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var rotation = Projectile.rotation + ((effects == SpriteEffects.None) ? 0.785f : 2.355f);
        var origin = (effects == SpriteEffects.FlipHorizontally) ? Projectile.Size / 2 : new Vector2(texture.Width - (Projectile.width / 2), Projectile.height / 2);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, effects, 0);

        for (var i = 0; i < Projectile.oldPos.Length; i++) {
            var drawPos = Projectile.oldPos[i] - Main.screenPosition + origin + (Vector2.UnitY * Projectile.gfxOffY);

            if (effects == SpriteEffects.None)
                drawPos.X -= texture.Width - Projectile.width;

            var alphaMod = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length) * .5f;
            var color = Color.Lerp(alphaMod, alphaMod with { A = 0 }, (Player.itemAnimation - HalfTime) / HalfTime);

            Main.EntitySpriteDraw(texture, drawPos, null, color, rotation, origin, Projectile.scale, effects, 0);
        }

        var maxTime = HalfTime * 1.5f;
        if (Player.itemAnimation < maxTime) {
            var extraTexture = TextureAssets.Extra[89].Value;
            var scale = new Vector2(MathHelper.Max(0, (float)Player.itemAnimation / maxTime - .5f), 1) * Projectile.scale;

            Main.EntitySpriteDraw(extraTexture, Player.Center + (Vector2.Normalize(Projectile.velocity) * 100) - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.velocity.ToRotation() + 1.57f, extraTexture.Size() / 2, scale, effects, 0);
        }

        return false;
    }
}