using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace UnoClass.Content.Items.Decks
{
	public class BloodyDeck : DeckWeapon {

        public override void DeckDefaults() { 
            Item.damage = 23; //item damage
			Item.knockBack = 7.5f; //item knockback :D

            SetValues( //set globalitem variables
                DeckID: 3,
                DeckColor: new Color(53, 0, 28),
                DeckRange: 11,
                CardSlots: 1,
                ImpactRadius: 225
            );

			Item.rare = ItemRarityID.LightRed; //Rarity
			Item.value = Item.sellPrice(silver:85); //Value
        }
    }
}