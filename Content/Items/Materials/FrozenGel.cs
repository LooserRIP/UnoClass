using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace UnoClass.Content.Items.Materials
{
	public class FrozenGel : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
			ItemID.Sets.ExtractinatorMode[Item.type] = Item.type;
		}

		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 14;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(silver: 1);
            /*
			Item.useAnimation = 5;
			Item.useTime = 5;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Swing;
			*/
		}
        public override void AddRecipes() {
			Recipe.Create(ItemID.Gel, 2)
				.AddIngredient<FrozenGel>(1)
				.AddTile(TileID.Furnaces)
				.Register();
        }


        /*
		//adding some extractination for ice and gel
        public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) {
            Item.consumable = true;
            Main.NewText("using!! " + resultType + ", " + resultStack);
			resultType = Main.rand.NextFromList(ItemID.Gel, ItemID.Gel, ItemID.IceBlock, ItemID.SnowBlock, ItemID.CopperCoin, ItemID.SilverCoin);
			resultStack = Main.rand.Next(1, Main.rand.Next(1, 4));
			int check = Main.rand.Next(0, 25);
			if (check == 0) {
				resultType = Main.rand.Next(0,25) == 0 ? ItemID.PlatinumCoin : ItemID.GoldCoin;
				resultStack = 1;
			}
			base.ExtractinatorUse(extractinatorBlockType, ref resultType, ref resultStack);
        }
        public override bool ConsumeItem(Player player) {
			return true;
        }
        public override void OnConsumeItem(Player player) {
            base.OnConsumeItem(player);
        }*/
    }
    }
