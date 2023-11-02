using UnoClass.Common.Players;
using UnoClass.Content.DamageClasses;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Diagnostics;
using Terraria.ID;

namespace UnoClass.Content.Items.Accessories {
    //[AutoloadEquip(EquipType.HandsOff)]

    public class PurpleHandWarmer : ModItem
	{
        public static readonly int AdditiveDamageBonus = 0;
        public static readonly int AdditiveRegenerationRate = 7;
        public static readonly int AdditivePickupRange = 35;
        public static readonly int AdditiveImpactRange = 15;
        public static readonly int ExtraCardSlots = 0;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            AdditiveRegenerationRate, AdditiveImpactRange);

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
            Item.defense = 4;
            Item.rare = ItemRarityID.Blue;
        }

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetDamage(ModContent.GetInstance<UnoDamageClass>()) += AdditiveDamageBonus / 100f;
            player.GetModPlayer<UnoModifiersPlayer>().CardSlots += ExtraCardSlots;
            player.GetModPlayer<UnoModifiersPlayer>().RegenerationMult += AdditiveRegenerationRate / 100f;
            player.GetModPlayer<UnoModifiersPlayer>().AccessRangeMult += AdditivePickupRange / 100f;
            player.GetModPlayer<UnoModifiersPlayer>().ImpactRangeMult += AdditiveImpactRange / 100f;
        }

        public override void AddRecipes() {
            CreateRecipe(1)
                .AddIngredient(ItemID.DemoniteBar, 7)
                .AddIngredient(ItemID.ShadowScale, 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}