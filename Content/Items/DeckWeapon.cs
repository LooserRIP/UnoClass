using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using UnoClass.Common.GlobalItems;
using UnoClass.Common.Players;
using UnoClass.Content.DamageClasses;

namespace UnoClass.Content.Items {
    public abstract class DeckWeapon : ModItem {

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            UnoSystem.CacheDeck(this);
        }

        public sealed override void SetDefaults() {
            Item.DamageType = ModContent.GetInstance<UnoDamageClass>();
            Item.width = 34;
            Item.height = 42;
            Item.scale = 2;

            Item.useTime = 1;
            Item.useAnimation = 1;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.holdStyle = ItemHoldStyleID.HoldUp;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = null;
            Item.ResearchUnlockCount = 1;
            Item.maxStack = 1;
            Item.crit = 0;
            Item.AllowReforgeForStackableItem = true;

            Item.damage = 20;
            Item.knockBack = 6;

            Item.rare = 2;
            Item.value = Item.sellPrice(silver: 1);

            DeckDefaults();
        }

        public abstract void DeckDefaults(); // Declare an abstract method that must be implemented in the derived classes
        public override void HoldItemFrame(Player player) {
            float rotation = -1.2f;
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, player.direction * rotation);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction * rotation);
        }
        public override void UseAnimation(Player player) { //just the use shit
            if (player.altFunctionUse == 2) {
                player.GetModPlayer<UnoModifiersPlayer>().SkipDeck(Item);
            } else { //I know I can do this shit without the else, but this lets me sleep at night
                player.GetModPlayer<UnoModifiersPlayer>().UseDeck(Item);
            }
        }
        public override bool? CanHitNPC(Player player, NPC target) {
            return false;
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }

        /// <summary>
        /// Sets the common deck-related attributes without error.
        /// </summary>
        /// <param name="DeckID">The identifier for the deck</param>
        /// <param name="DeckColor">The color of the deck's border and empty cards</param>
        /// <param name="DeckRange">The range in tiles, wooden deck is 7 for reference.</param>
        /// <param name="CardSlots">The number of card slots in the deck item.</param>
        /// <param name="ImpactRadius">The impact radius of the card explosion. Wooden deck is 190.</param>
        /// <param name="SpecialCard">Special card, 0 for none.</param>
        /// <param name="DeckRangeMultiplier">Reserved for prefixes, usually not altered.</param>
        /// <param name="DeckRegenerationMultiplier">Reserved for prefixes, usually not altered.</param>
        public void SetValues(int? DeckID = null, Color? DeckColor = null, int? DeckRange = null,
            int? CardSlots = null, int? ImpactRadius = null, int? SpecialCard = null,
            float? DeckRangeMultiplier = null, float? DeckRegenerationMultiplier = null) {

            if (UnoSystem.GetDeckGlobal(Item, out GlobalDecks deckItem)) { //would prefer a return here, i'm an ifless person yk
                if (DeckID != null) deckItem.deckID = (int)DeckID;
                if (DeckColor != null) deckItem.deckColor = (Color)DeckColor;
                if (DeckRange != null) deckItem.deckRange = (int)DeckRange;
                if (CardSlots != null) deckItem.cardSlots = (int)CardSlots;
                if (ImpactRadius != null) deckItem.impactRadius = (int)ImpactRadius;
                if (DeckRangeMultiplier != null) deckItem.deckRangeMultiplier = (float)DeckRangeMultiplier;
                if (DeckRegenerationMultiplier != null) deckItem.deckRangeMultiplier = (float)DeckRegenerationMultiplier;
            }
        }

    }
}