using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using UnoClass;
using UnoClass.Common.GlobalItems;

namespace UnoClass.Content.Prefixes {
    // an abstract base class for all the prefixes i'll be adding :
    public abstract class DeckPrefix : ModPrefix {

        internal float damageMult = 1f;
        internal float knockbackMult = 1;
        internal float rangeMult = 1;
        internal float regenerationMult = 1;

        public override PrefixCategory Category => PrefixCategory.Custom;

        public DeckPrefix() { } //just for defaults

        public DeckPrefix(float damageMult, float rangeMult, float regenerationMult, float knockbackMult) {
            this.damageMult = damageMult;
            this.rangeMult = rangeMult;
            this.regenerationMult = regenerationMult;
            this.knockbackMult = knockbackMult;
        }
        public override void SetStaticDefaults() {
            DeckPrefixes.Add(Type);
            DeckPrefixDictionary.Add(Type, new Tuple<float,float,float,float>(damageMult, rangeMult, regenerationMult, knockbackMult));
        }

        // all the list stuff although i'd prefer this in UnoSystem

        internal static List<int> DeckPrefixes;
        internal static Dictionary<int,Tuple<float,float,float,float>> DeckPrefixDictionary;
        public override void Load() {
            DeckPrefixes = new List<int>();
            DeckPrefixDictionary = new Dictionary<int, Tuple<float, float, float, float>>() { };
        }

        public override void Unload() {
            DeckPrefixes = null;
        }

        public static bool DoConditionsApply(Item item) {
            return UnoSystem.IsDeck(item);
        }

        public override bool CanRoll(Item item) {
            return DoConditionsApply(item);
        }



        public override void ModifyValue(ref float valueMult) {
            valueMult *= 1 + (0.08f * rangeMult * damageMult * regenerationMult * knockbackMult);
        }

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
            damageMult = this.damageMult;
            knockbackMult = this.knockbackMult;
        }
        public override void Apply(Item item) {
            if (UnoSystem.GetDeckGlobal(item, out GlobalDecks deckItem)) {
                deckItem.deckRangeMultiplier = rangeMult;
                deckItem.deckRegenerationMultiplier = regenerationMult;
            }
        }
        

    }
}