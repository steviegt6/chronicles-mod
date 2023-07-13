using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Hostile;

public class Slimeling : ChroniclesNPC {
    private int frame;
    private bool airborne;

    public int SlimeType { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
    public const int COLOR_BLUE = NPCID.BlueSlime;
    public const int COLOR_GREEN = NPCID.GreenSlime;
    public const int COLOR_PURPLE = NPCID.PurpleSlime;
    public const int COLOR_ICE = NPCID.IceSlime;
    public const int COLOR_ILLUMANT = NPCID.IlluminantSlime;
    public const int COLOR_SAND = NPCID.SandSlime;

    private Color SlimeColor => SlimeType switch {
        COLOR_GREEN => Color.LightGreen,
        COLOR_PURPLE => Color.MediumPurple,
        COLOR_ICE => new Color(128, 244, 255),
        COLOR_ILLUMANT => Color.Magenta,
        COLOR_SAND => new Color(255, 242, 99),
        _ => Color.SkyBlue
    };

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 4;

    public override void SetDefaults() {
        NPC.friendly = false;
        NPC.lifeMax = 1;
        NPC.damage = 10;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1 with { Pitch = 1.5f };
        NPC.alpha = 50;
        NPC.scale = Main.rand.NextFloat(0.8f, 1.3f);
        NPC.Size = new Vector2((int)(18 * NPC.scale), (int)(10 * NPC.scale));
    }

    public override void AI() {
        NPC.TargetClosest();
        var target = Main.player[NPC.target];

        if (((NPC.localAI[0] = MathHelper.Max(NPC.localAI[0] - 1, 0)) <= 0) && airborne && (NPC.velocity.Y == 0)) {
            SoundEngine.PlaySound(SoundID.NPCHit1 with { PitchVariance = .4f, Volume = .25f }, NPC.Center);
            NPC.localAI[0] = 10; //Cooldown
        }

        var lungeRange = 16 * 3;
        if (NPC.Distance(target.Center) <= lungeRange && NPC.velocity.Y == 0 && Main.netMode != NetmodeID.MultiplayerClient) {
            NPC.velocity = new Vector2(NPC.direction * Main.rand.NextFloat(3.5f, 7.0f), -Main.rand.NextFloat(4.0f, 6.0f));
            NPC.netUpdate = true;
        }
        else {
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * 1.25f, .1f);

            float throwaway = 6;
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref throwaway, ref throwaway);

            if (Main.rand.NextBool(300) && Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.velocity = new Vector2(NPC.direction * Main.rand.NextFloat(3.5f, 5.0f), -Main.rand.NextFloat(2.0f, 4.0f));
                NPC.netUpdate = true;
            }
        }
        NPC.rotation = (NPC.velocity.Y != 0) ? NPC.velocity.X : 0;

        airborne = NPC.velocity.Y != 0;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        for (var i = 0; i < 5; i++) {
            var dustVel = (new Vector2(DustID.t_Slime, hit.HitDirection) * Main.rand.NextFloat(0.5f, 2.0f)).RotatedByRandom(1f);
            Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.t_Slime, hit.HitDirection, -1, 150, default, MathHelper.Min(dustVel.Length(), 1.3f)).color = SlimeColor with { A = 150 };
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter = (NPC.frameCounter + .2f) % Main.npcFrameCount[Type];
        NPC.frame.Y = (frame = (int)NPC.frameCounter) * frameHeight;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = TextureAssets.Npc[Type].Value;
        var drawFrame = texture.Frame(1, Main.npcFrameCount[Type], 0, frame, 0, -2);
        Main.EntitySpriteDraw(texture, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY + 2), drawFrame, NPC.GetAlpha(drawColor.MultiplyRGB(SlimeColor)), NPC.rotation, drawFrame.Size() / 2, NPC.scale, SpriteEffects.None, 0);
        return false;
    }
}