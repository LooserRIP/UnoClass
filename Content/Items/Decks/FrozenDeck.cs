using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace UnoClass.Content.Items.Decks
{
	public class FrozenDeck : DeckWeapon {

        public override void DeckDefaults() { 
            Item.damage = 55; //item damage
			Item.knockBack = 6f; //item knockback :D

            SetValues( //set globalitem variables
                DeckID: 2,
                DeckColor: new Color(59, 48, 112),
                DeckRange: 12,
                CardSlots: 1,
                ImpactRadius: 250,
                SpecialCard: 1

            );

			Item.rare = ItemRarityID.Orange; //Rarity
			Item.value = Item.sellPrice(silver:35); //Value
        }
        public override void AddRecipes() {
            CreateRecipe(1)
                .AddIngredient<Materials.FrozenGel>(15)
                .AddIngredient(ItemID.PlatinumBar, 10)
                .AddIngredient(ItemID.IceBlock, 25)
                .AddIngredient(ItemID.SnowBlock, 25)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}