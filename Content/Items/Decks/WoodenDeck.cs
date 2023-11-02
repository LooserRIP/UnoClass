using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace UnoClass.Content.Items.Decks
{
	public class WoodenDeck : DeckWeapon {

        public override void DeckDefaults() { 
            Item.damage = 18; //item damage
			Item.knockBack = 3; //item knockback :D

            SetValues( //set globalitem variables
                DeckID: 0,
                DeckColor: new Color(42, 42, 42),
                DeckRange: 7,
                CardSlots: 1,
                ImpactRadius: 190
            );

			Item.rare = 2; //Rarity
			Item.value = Item.sellPrice(silver:1); //Values
        }
        public override void AddRecipes() {
            CreateRecipe(1)
                .AddRecipeGroup(RecipeGroupID.Wood, 30)
                .AddIngredient(ItemID.BambooBlock, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}