using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using UnoClass.Content.DamageClasses;
using UnoClass.Common.Players;
using UnoClass.Common.GlobalItems;
using Terraria.Utilities;
using Terraria.Localization;
using System;

namespace UnoClass.Content.Items.Decks
{
	public class LivingDeck : DeckWeapon {

        public override void DeckDefaults() { //
            Item.damage = 22; //item damage
			Item.knockBack = 4.5f; //item knockback :D

            SetValues( //set globalitem variables
                DeckID: 1,
                DeckColor: new Color(42, 42, 42),
                DeckRange: 12,
                CardSlots: 1,
                ImpactRadius: 200
            );

			Item.rare = ItemRarityID.Orange; //Rarity
			Item.value = Item.sellPrice(silver:17, copper:50); //Values


        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            int acornAmount = 0;
            foreach (Item acornCheck in player.inventory) {
                if (acornCheck.type == ItemID.Acorn) {
                    acornAmount += acornCheck.stack;
                }
            }
            damage.Base += MathF.Min(MathF.Max(22f * ((MathF.Log(acornAmount * 10) / 10f)),0),20);
        }
    }
}