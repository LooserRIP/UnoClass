using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria;
using Terraria.ModLoader;
using UnoClass.Common.GlobalItems;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace UnoClass {
    public class UnoSystem : ModSystem {


        private static HashSet<int> UnoDecks { get; set; }
        private static List<SpecialCardInfo> SpecialCards { get; set; }

        public override void OnModLoad() { //
            UnoDecks = new HashSet<int>();
            SpecialCards = new List<SpecialCardInfo>() {
                new SpecialCardInfo("Snowball", 1, "Freezes, inflicts frostburn,\nand knocks back nearby enemies.", Color.LightSkyBlue),
                new SpecialCardInfo("Gem Blitz", 3, "Spawns 5 homing gems\nto damage nearby enemies.", Color.LightGray)
            };
        }

        //just less messy than a .Contains
        public static bool IsDeck(int id) {
            return UnoDecks.Contains(id);
        }
        public static bool IsDeck(Item item) {
            return IsDeck(item.type); //see now this is like that chill type of syntax i love lowercase.lowercase it's so neat and pretty
        }
        public static bool IsDeck(ModItem moditem) {
            return IsDeck(moditem.Type); //WHY IS IT UPPERCASED HERE AND NOT THERE??????????????????
        }
        public static bool GetDeckGlobal(Item item, out GlobalDecks result) { //this shit is probably so badly coded i apologize if anyone has decompiled this to learn how the mod works, it's screws and bandaids
            result = null;
            if (!IsDeck(item.type)) return false;
            if (!item.TryGetGlobalItem(out result)) return false;
            return true;
        }
        public static bool GetDeckGlobal(int id, out GlobalDecks result) {
            return GetDeckGlobal(ModContent.GetModItem(id).Item, out result);
        }
        public static int GetDeckID(Item item) { //this shit is probably so badly coded i apologize if anyone has decompiled this to learn how the mod works, it's screws and bandaids
            GlobalDecks result;
            if (!GetDeckGlobal(item, out result)) return -1;
            return result.deckID;
        }

        //this is for caching all the uno deck ids, so that i can apply GlobalItems only to those
        public static void CacheDeck(ModItem deckItem) {
            int idofitem = deckItem.Item.type;
            if (!UnoDecks.Contains(idofitem)) {
                UnoDecks.Add(idofitem);
            }
        }

        public static int DeckPositionStatus = 0;
        public static int DeckPositionHeight = 1;

        public static int UpdatePositionStatus() {
            DeckPosition();
            return DeckPositionStatus;
        }
        public static Vector2 DeckPositionWorld() {
            return DeckPosition() + Main.screenPosition;
        }
        public static Vector2 DeckPosition() {
            Vector2 normal = DeckPosition(0);
            Vector2 player = DeckPosition(1);

            DeckPositionHeight = 1;
            if (!Main.playerInventory) {
                DeckPositionHeight = -1;
            }
            if (Vector2.Distance(normal, player) > 32) {
                DeckPositionStatus = 1;
                return player;
            }
            DeckPositionStatus = 0;
            return normal;
        }
        public static Vector2 DeckPosition(int status) {
            Vector2 normal =
                (new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f + (95f * DeckPositionHeight)));
            if (status == 0) return normal;
            Vector2 player =
                (Main.CurrentPlayer.Center + new Vector2(0, (95f * DeckPositionHeight)) - Main.screenPosition);
            return player;
        }

    }
}