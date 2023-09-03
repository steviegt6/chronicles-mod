using Chronicles.Core.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace Chronicles.Content.NPCs.Vanilla;

public class FloatyGross : VanillaNPC {
    public override object NPCTypes => NPCID.FloatyGross;

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => false;

    public override void PostAI(NPC npc) {
        //Try to move into the player and suffocate them
        var target = Main.player[npc.target];
        if (npc.Distance(target.Center) < 30) {
            npc.Center = Vector2.Lerp(npc.Center, target.Center, .09f);
            npc.velocity = Vector2.Zero;
        } //Snap to position

        foreach (var player in Main.player.Where(x => x.active && !x.dead && x.Hitbox.Intersects(npc.Hitbox))) {
            player.AddBuff(BuffID.Weak, 300);
            player.AddBuff(BuffID.Suffocation, 30);
            player.lifeRegen -= npc.damage / 10; //'damage' the player dynamically based on what the NPC should normally
        }
    }
}