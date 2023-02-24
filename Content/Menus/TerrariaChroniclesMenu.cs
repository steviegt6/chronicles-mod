using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Chronicles.Content.Menus; 

public sealed class TerrariaChroniclesMenu : ChroniclesMenu {
    public override string DisplayName => "";

    public override Asset<Texture2D> Logo => ChroniclesAssets.Menu.ChroniclesTitle;

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
        logoRotation = 0;
        logoScale = 1;
        drawColor = Color.White;
        return base.PreDrawLogo(spriteBatch, ref logoDrawCenter, ref logoRotation, ref logoScale, ref drawColor);
    }
}
