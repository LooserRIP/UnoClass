using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace UnoClass.Content.Items.Decks
{
	public class GemstoneDeck : DeckWeapon {

        public override void DeckDefaults() { 
            Item.damage = 50; //item damage
			Item.knockBack = 4f; //item knockback :D

            SetValues( //set globalitem variables
                DeckID: 4,
                DeckColor: new Color(48, 48, 57),
                DeckRange: 15,
                CardSlots: 2,
                ImpactRadius: 250,
                SpecialCard: 2
            );

			Item.rare = ItemRarityID.Orange; //Rarity
			Item.value = Item.sellPrice(silver: 75); //Value
        }
        public override void AddRecipes() {
            CreateRecipe(1)
                .AddIngredient(ItemID.StoneBlock, 15)
                .AddIngredient(ItemID.Ruby, 5)
                .AddIngredient(ItemID.Topaz, 5)
                .AddIngredient(ItemID.Emerald, 5)
                .AddIngredient(ItemID.Sapphire, 5)
                .AddIngredient(ItemID.Diamond, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}