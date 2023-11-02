using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using UnoClass.Content.DamageClasses;
using UnoClass.Common.Players;
using Terraria.DataStructures;
using Terraria.Utilities;
using UnoClass;
using UnoClass.Content.Prefixes;
using Terraria.Localization;
using UnoClass.Utilities;
using System;

namespace UnoClass.Common.GlobalItems
{
	public class GlobalDecks : GlobalItem
	{
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
            return UnoSystem.IsDeck(entity);
        }

        public int deckID;
        public Color deckColor;
        public int deckRange;
        public int cardSlots;
        public int impactRadius;
        public int specialCard;
        public float deckRangeMultiplier = 1;
        public float deckRegenerationMultiplier = 1;



        public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand) {
            if (pre == -1 && rand.Next(1, 8) <= 2) return false; 
            return true;
        }
        public override int ChoosePrefix(Item item, UnifiedRandom rand) {
            if (DeckPrefix.DoConditionsApply(item)) {
                return rand.Next(DeckPrefix.DeckPrefixes);
            }
            return base.ChoosePrefix(item, rand);
        }
        public int Percentageify(float val) {
            return (int)Math.Round((val - 1f) * 100f);
        }
        public float RoundToOneDecimal(float val) {
            return MathF.Round(val, 1);
        }
        public float GetDeckRange(Item item) {
            GlobalDecks deckItem;
            float prefixMult = 1;
            float playerMult = Main.CurrentPlayer.GetModPlayer<UnoModifiersPlayer>().AccessRangeMult;
            float deckMult = 1;
            if (DeckPrefix.DeckPrefixes.Contains(item.prefix)) prefixMult = (DeckPrefix.DeckPrefixDictionary[item.prefix].Item2 - 1);
            if (UnoSystem.GetDeckGlobal(Main.CurrentPlayer.HeldItem, out deckItem)) deckMult = deckItem.deckRangeMultiplier;
            return RoundToOneDecimal(deckRange * 0.5f * (playerMult - (deckMult - 1) + (prefixMult - 1)));
        }
        public float GetDeckRegeneration(Item item) {
            GlobalDecks deckItem;
            float prefixMult = 1;
            float playerMult = Main.CurrentPlayer.GetModPlayer<UnoModifiersPlayer>().RegenerationMult;
            float deckMult = 1;
            if (DeckPrefix.DeckPrefixes.Contains(item.prefix)) prefixMult = (DeckPrefix.DeckPrefixDictionary[item.prefix].Item3 - 1);
            if (UnoSystem.GetDeckGlobal(Main.CurrentPlayer.HeldItem, out deckItem)) deckMult = deckItem.deckRegenerationMultiplier;
            return RoundToOneDecimal(1f / (playerMult - (deckMult - 1) + (prefixMult - 1)));
        }
        public float GetImpactRadiusName() {
            return RoundToOneDecimal((impactRadius * Main.CurrentPlayer.GetModPlayer<UnoModifiersPlayer>().ImpactRangeMult) / 64f);
        }


        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {


            int ttindexs = tooltips.FindIndex(t => (t.Mod == "Terraria" || t.Mod == Mod.Name) && (t.IsModifier || t.Name.EndsWith("Knockback") || t.Name.Equals("Material")));
            TooltipLine tt3 = new TooltipLine(Mod, "ImpactRadiusItemInfo", GetImpactRadiusName() + " tile impact radius.") { };
            tooltips.Insert(ttindexs, tt3);
            if (Main.keyState.PressingShift()) {
                TooltipLine tt2 = new TooltipLine(Mod, "ItemRegenerationInfo", GetDeckRegeneration(item) + "s card regeneration rate") { };
                tooltips.Insert(ttindexs, tt2);
                TooltipLine tt4 = new TooltipLine(Mod, "PickupItemInfo", GetDeckRange(item) + " tile pickup range") { };
                tooltips.Insert(ttindexs, tt4);
            }
            TooltipLine tt1 = new TooltipLine(Mod, "CardSlotsItemInfo", cardSlots + " card slot" + (cardSlots == 1 ? "" : "s")) { };
            tooltips.Insert(ttindexs, tt1);


            //prefix tooltips, stole this shamelessly from clicker class - so good and clean.
            if (item.prefix < PrefixID.Count || !DeckPrefix.DeckPrefixes.Contains(item.prefix)) {
                return;
            }


            int ttindexm = tooltips.FindLastIndex(t => (t.Mod == "Terraria" || t.Mod == Mod.Name) && (t.IsModifier || t.Name.StartsWith("Tooltip") || t.Name.Equals("Material")));
            if (ttindexm != -1) {
                if (deckRangeMultiplier != 1) {
                    TooltipLine ttm = new TooltipLine(Mod, "PrefixDeckRange", (deckRangeMultiplier > 1 ? "+" : "") + Percentageify(deckRangeMultiplier) + "% pickup range") {
                        IsModifier = true,
                        IsModifierBad = deckRangeMultiplier < 1
                    };
                    tooltips.Insert(++ttindexm, ttm);
                }
                if (deckRegenerationMultiplier != 1) {
                    TooltipLine ttm = new TooltipLine(Mod, "PrefixDeckRegeneration", (deckRegenerationMultiplier > 1 ? "+" : "") + Percentageify(deckRegenerationMultiplier) + "% card regeneration rate") {
                        IsModifier = true,
                        IsModifierBad = deckRegenerationMultiplier < 1
                    };
                    tooltips.Insert(++ttindexm, ttm);
                }
            }
        }



    }
}
