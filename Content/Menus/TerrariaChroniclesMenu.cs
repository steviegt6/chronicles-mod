using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;

namespace Chronicles.Content.Menus;

public sealed class TerrariaChroniclesMenu : ChroniclesMenu {
    public override string DisplayName => Language.GetTextValue("Mods.Chronicles.Menu.TerrariaChronicles");

    public override Asset<Texture2D> Logo => ChroniclesAssets.Menu.ChroniclesTitle;

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
        logoDrawCenter -= new Vector2(0, 20f);
        drawColor = Color.White;
        base.PreDrawLogo(spriteBatch, ref logoDrawCenter, ref logoRotation, ref logoScale, ref drawColor);
        DrawVanillaLogo(spriteBatch, logoDrawCenter, drawColor);
        DrawModLogo(spriteBatch, logoDrawCenter);
        return false;
    }

    private void DrawVanillaLogo(SpriteBatch sb, Vector2 center, Color color) {
        var dayColor = new Color((byte)(color.R * (Main.LogoA / 255f)), (byte)(color.G * (Main.LogoA / 255f)), (byte)(color.B * (Main.LogoA / 255f)), (byte)(color.A * (Main.LogoA / 255f)));
        var nightColor = new Color((byte)(color.R * (Main.LogoB / 255f)), (byte)(color.G * (Main.LogoB / 255f)), (byte)(color.B * (Main.LogoB / 255f)), (byte)(color.A * (Main.LogoB / 255f)));
        
        sb.Draw(TextureAssets.Logo.Value, center, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), dayColor, 0f, TextureAssets.Logo.Size() / 2f, 1f, SpriteEffects.None, 0.0f);
        sb.Draw(TextureAssets.Logo2.Value, center, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), nightColor, 0f, TextureAssets.Logo.Size() / 2f, 1f, SpriteEffects.None, 0.0f);
    }

    private void DrawModLogo(SpriteBatch spriteBatch, Vector2 center) {
        spriteBatch.Draw(ChroniclesAssets.Menu.ChroniclesTitle.Value, center + new Vector2(0, 110f), null, Color.White, 0f, ChroniclesAssets.Menu.ChroniclesTitle.Size() / 2f, 1f, SpriteEffects.None, 0f);
    }
}
