using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using UnoClass.Content.Items.Materials;
using UnoClass.Content.Items.Decks;

namespace UnoClass.Common.GlobalNPCs
{
	public class GlobalNPCLoot : GlobalNPC
	{
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
            //Ice slime loot, give the frozen gel
            if (npc.type == NPCID.IceBat || npc.type == NPCID.IceSlime || npc.type == NPCID.SpikedIceSlime) {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrozenGel>(), npc.type == NPCID.IceSlime ? 2 : 3, 1, Main.rand.Next(2, 3)));
            }
            if (npc.type == NPCID.BloodZombie) {
                npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<BloodyDeck>(),150,75));
            }
        }
	}
}
