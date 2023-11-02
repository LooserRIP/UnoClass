using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using UnoClass.Common.GlobalItems;
using UnoClass.Content.DamageClasses;
using UnoClass.Content.Items.Decks;

namespace UnoClass.Common.PlayerLayers {
    public class DeckPlayerLayer : PlayerDrawLayer {
        private Asset<Texture2D> heldDecksSpritesheet;

        public override bool IsHeadLayer => true;

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
            if (drawInfo.drawPlayer.HeldItem == null) return false;
            return UnoSystem.IsDeck(drawInfo.drawPlayer.HeldItem.type);
        }

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            GlobalDecks deckItem;
            bool canDeck = drawInfo.drawPlayer.HeldItem.TryGetGlobalItem<GlobalDecks>(out deckItem);
            if (!canDeck) return;
            if (heldDecksSpritesheet == null) {
                heldDecksSpritesheet = ModContent.Request<Texture2D>("UnoClass/Content/Items/Spritesheets/HeldDecks");
            }

            var position = drawInfo.Center + new Vector2(drawInfo.drawPlayer.direction * 12 + 0f, 2f) - Main.screenPosition;
            position = new Vector2((int)position.X, (int)position.Y);

            drawInfo.DrawDataCache.Add(new DrawData(
                (Texture2D)heldDecksSpritesheet,
                new Rectangle((int)position.X,
                             (int)position.Y,
                            20,
                            20
                    ),
                new Rectangle(deckItem.deckID * 10, 0, 10, 10),
                drawInfo.itemColor,
                0,
                new Vector2(5,5),
                drawInfo.drawPlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            ));
        }
    }
}