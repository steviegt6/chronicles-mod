using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Chronicles.Core.ModLoader;

public abstract class VanillaNPC : GlobalNPC,
                                   IChroniclesType<GlobalNPC> {
    public bool Alerted { get; private set; }

    /// <summary>
    /// Simply set to whos NPC types you want to change the behaviour of
    /// </summary>
    public abstract object NPCTypes { get; }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
        return NPCTypes switch {
            Array => ((int[])NPCTypes).Contains(entity.type),
            int => (int)NPCTypes == entity.type,
            short => (short)NPCTypes == entity.type,
            _ => false,
        };
    }

    public override void OnSpawn(NPC npc, IEntitySource source) {
        void spawnAsPack(Vector2 origin, int size, float distance) {
            for (var i = 0; i < size; i++) {
                for (var o = 0; o < 50; o++) {
                    var spawnPos = origin + (Main.rand.NextVector2Unit() * Main.rand.NextFloat() * distance);
                    spawnPos.Y = MathHelper.Clamp(spawnPos.Y, origin.Y - 16, origin.Y + 16); //As a pack, try to spawn on roughly the same elevation

                    if (!WorldGen.SolidTile(Framing.GetTileSafely((spawnPos / 16).ToPoint()))) {
                        var id = NPC.NewNPC(new EntitySource_Parent(npc), (int)spawnPos.X, (int)spawnPos.Y, npc.type);

                        if (Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(MessageID.SyncNPC, number: id);
                        break;
                    }
                }
            }
        }

        foreach (var type in VanillaNPCSystem.BEHAVIOUR_PACKS) {
            if (type is IPackNPC packNPC)
                if (((type.NPCTypes is Array && ((int[])type.NPCTypes).Contains(npc.type)) || (type.NPCTypes is int @int && @int == npc.type) || (type.NPCTypes is short @short && @short == npc.type)) && source is not EntitySource_Parent)
                    spawnAsPack(npc.Center, packNPC.PackSize() - 1, 16 * 7);
        }
    }

    public void SetAlertness(NPC npc, bool value, bool showBubble = true) {
        if (value && !Alerted && showBubble)
            EmoteBubble.NewBubble(EmoteID.EmotionAlert, new WorldUIAnchor(npc), 30);
        Alerted = value;
    }
}

public class VanillaNPCSystem : ModSystem {
    public static readonly List<VanillaNPC> BEHAVIOUR_PACKS = new();

    public override void Load() {
        var types = typeof(ChroniclesMod).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(VanillaNPC)));
        foreach (var type in types) {
            if (Activator.CreateInstance(type) is VanillaNPC item)
                BEHAVIOUR_PACKS.Add(item);
        } //Load all packs into a list, to easily find associated NPC types for external use

        for (var i = 0; i < NPCLoader.NPCCount; i++) {
            if (ModContent.RequestIfExists<Texture2D>($"Chronicles/Assets/NPCs/Vanilla/NPC_{i}", out var asset))
                TextureAssets.Npc[i] = asset;
        } //Load custom textures for vanilla NPCs
    }

    public override void Unload() {
        for (var i = 0; i < NPCLoader.NPCCount; i++) {
            if (ModContent.RequestIfExists<Texture2D>("Chronicles/Assets/NPCs/Vanilla/NPC_" + i, out var _))
                TextureAssets.Npc[i] = ModContent.Request<Texture2D>("Terraria/Images/NPC_" + i);
        } //Unload custom textures for vanilla NPCs
    }
}