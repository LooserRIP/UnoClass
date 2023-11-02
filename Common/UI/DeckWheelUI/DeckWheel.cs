using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using UnoClass.Common.Players;
using Terraria.GameContent.ObjectInteractions;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using UnoClass.Common.GlobalItems;

namespace UnoClass.Common.UI.DeckWheelUI {
    public class DeckWheel : UIElement {
        private UIImage[] cardFrame;
        private Vector2[] cardFramePositions;
        private float[] cardFrameScales;
        private float[] cardFrameRotationLerps;
        private float offsetLerpPrevious;
        private float offsetLerp = 0;
        private float offsetDeckRotation = 0;
        private Color gradientA;
        private Color gradientB;
        private int prevCardSlots = -1;
        private UnoModifiersPlayer modPlayer;
        //private Asset<Texture2D> EmptyCard;
        //private Asset<Texture2D>[][] CardTextures;
        private Asset<Texture2D> CardSpritesheet;
        private Asset<Texture2D> DeckSpritesheet;
        private Asset<Texture2D> DeckOutline;
        private UnoCard[] CardCache;
        private FastNoiseLite FNLRotation;
        private float fnlx;
        private GlobalDecks cachedDeckItem;

        private float innX;
        private float innY;
        private float innYLerp;
        private float lerpDeckPositionStatus;

        public DeckWheel() {
            //Loading in card textures
            /*
            CardTextures = new Asset<Texture2D>[4][];
            for (int color = 0; color < 4; color++) {
                CardTextures[color] = new Asset<Texture2D>[10];
                for (int value = 0; value < 10; value++) {
                    CardTextures[color][value] = ModContent.Request<Texture2D>(
                            "UnoClass/Content/Items/Cards/" + ColorName(color) + "_" + value);
                }
            }
            EmptyCard = ModContent.Request<Texture2D>("UnoClass/Content/Items/Cards/Empty");
            */
            //Done loading card textures. hope this doesn't crash the game or something

            CardSpritesheet = ModContent.Request<Texture2D>("UnoClass/Content/Items/Spritesheets/Cards");
            DeckSpritesheet = ModContent.Request<Texture2D>("UnoClass/Content/Items/Spritesheets/Decks");
            DeckOutline = ModContent.Request<Texture2D>("UnoClass/Common/UI/DeckWheelUI/Wheel");

            //initializing some boring lists
            cardFramePositions = new Vector2[10];
            cardFrameScales = new float[10];
            cardFrameRotationLerps = new float[10];
            CardCache = new UnoCard[10];
            for (int i = 0; i < 10; i++) {
                cardFramePositions[i] = new Vector2(30, 30);
                cardFrameScales[i] = 0;
                cardFrameRotationLerps[i] = 0;
            }

            FNLRotation = new FastNoiseLite(Main.rand.Next(-10000,10000));
            FNLRotation.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            fnlx = 0;
        }
        public void ResetVariables() {
            if (modPlayer == null) return;
            offsetLerp = modPlayer.CardSlots - 1;
            offsetDeckRotation = 1;
            innYLerp = 0;

        }
        public string ColorName(int color) {
            if (color == 0) return "Red";
            if (color == 1) return "Blue";
            if (color == 2) return "Yellow";
            return "Green";
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            CalculatedStyle innerDimensions = GetInnerDimensions();
            fnlx += 33;
            Vector2 innset = UnoSystem.DeckPosition();
            lerpDeckPositionStatus = 0.9f * lerpDeckPositionStatus + (0.1f * UnoSystem.DeckPositionStatus);
            if (lerpDeckPositionStatus <= 0.001f) lerpDeckPositionStatus = 0;
            if (lerpDeckPositionStatus >= 0.999f) lerpDeckPositionStatus = 1;
            Vector2 inn0 = UnoSystem.DeckPosition(0);
            Vector2 inn1 = UnoSystem.DeckPosition(1);
            Vector2 inn = Vector2.Lerp(inn0, inn1, lerpDeckPositionStatus);
            if (innYLerp == 0) innYLerp = inn.Y;
            innYLerp = MathHelper.Lerp(innYLerp, inn.Y, lerpDeckPositionStatus > 0.9f ? 1f : 0.05f);
            inn.Y = innYLerp;
            innX = inn.X - 50;
            innY = inn.Y - 50;
            float totalAlpha = 1;
            Color alphaColor = Color.White;
            if (MathF.Abs(inn.Y + Main.screenPosition.Y - Main.CurrentPlayer.Center.Y) < 65f) {
                totalAlpha = (MathF.Max(MathF.Abs(inn.Y + Main.screenPosition.Y - Main.CurrentPlayer.Center.Y) - 40,0)) / 25f * 0.9f + 0.1f;
                alphaColor = new Color(totalAlpha, totalAlpha, totalAlpha, totalAlpha);
            }
            

            modPlayer = Main.CurrentPlayer.GetModPlayer<UnoModifiersPlayer>();

            GlobalDecks deckItem;
            bool gotDeckItem = UnoSystem.GetDeckGlobal(Main.CurrentPlayer.HeldItem, out deckItem);
            if (gotDeckItem) {
                cachedDeckItem = deckItem;
            }
            if (cachedDeckItem == null) return;
            /*
            if (modPlayer.scaleLerp < 0.01f) {
                for (int i = 0; i < 10; i++) {
                    cardFramePositions[i] = new Vector2(30, 30);
                    cardFrameScales[i] = 1;
                }
            }
            */
            float radius = 23f;
            float radiusMultiplier = 1;
            float newOffsetLerp = offsetLerp * 0.9f + (modPlayer.offsetDeck * 0.1f);
            offsetDeckRotation = offsetDeckRotation * 0.9f + (modPlayer.deckRotation * 0.1f);
            float distance = modPlayer.DeckWheelDistance();
            offsetLerp = newOffsetLerp;
            if (offsetLerpPrevious == 0) offsetLerpPrevious = offsetLerp;
            radiusMultiplier = 1 + (MathF.Abs(offsetLerpPrevious - offsetLerp) * 3f);
            offsetLerpPrevious = offsetLerp;
            List<CardDrawCall> cardDrawCall = new List<CardDrawCall>() { };
            //Main.NewText("Card Slots: " + modPlayer.CardSlots.ToString(), Color.LightBlue);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);

            //draw the fUCKING WHEEL
            float deckScale = modPlayer.scaleLerp;
            float deckRotation = offsetDeckRotation;
            int deckPositionX = (int)(innX+(50f*deckScale)+(50f*(1-deckScale)));
            int deckPositionY = (int)(innY+(50f*deckScale)+(50f*(1-deckScale)));

            spriteBatch.Draw(
                (Texture2D)DeckSpritesheet,
                new Rectangle((int)deckPositionX, (int)deckPositionY, (int)(100*deckScale), (int)(100*deckScale)),
                new Rectangle(50 * cachedDeckItem.deckID, 0, 50, 50), // piece we're cutting off the spritesheet
                    Color.White.MultiplyRGBA(alphaColor), // color
                    deckRotation * MathF.PI * 2, //rotation value, i think it's like per pi values
                    new Vector2((int)(25), (int)(25)), //origin. this shit gonna SUCK to code
                    SpriteEffects.None, //sprite effect
                    0);
            spriteBatch.Draw(
                (Texture2D)DeckOutline,
                new Rectangle((int)deckPositionX, (int)deckPositionY, (int)(100*deckScale), (int)(100*deckScale)),
                null, // we're not cutting anything since this is the outline
                    cachedDeckItem.deckColor.MultiplyRGBA(alphaColor), // color
                    deckRotation * MathF.PI * 2, //rotation value, i think it's like per pi values
                    new Vector2((int)(25), (int)(25)), //origin. this shit gonna SUCK to code
                    SpriteEffects.None, //sprite effect
                    0);            //draw wheel cards



            for (int i = 0; i < 10; i++) {
                if (modPlayer.CardSlots > i) {
                    cardFrameScales[i] = cardFrameScales[i] * 0.9f + 0.1f;
                    if (cardFrameScales[i] > 0.99f) cardFrameScales[i] = 1f;
                } else {
                    cardFrameScales[i] = cardFrameScales[i] * 0.9f;
                    if (cardFrameScales[i] < 0.01f) cardFrameScales[i] = 0f;
                }
                if (cardFrameScales[i] > 0) {
                    float angle = (i + newOffsetLerp) * (2 * MathF.PI / modPlayer.CardSlots);

                    float x = MathF.Sin(angle) * radius * radiusMultiplier;
                    float y = MathF.Cos(angle) * radius * radiusMultiplier;


                    int realID = (i + modPlayer.offsetDeck) % modPlayer.CardSlots;
                    Vector2 offset = new Vector2(x, y);
                    if (modPlayer.CardSlots < 2) offset = Vector2.Zero; 
                    // ^^ for animation, we make sure that we don't move cards while they're not at full scale.
                    float strongerLerp = modPlayer.scaleLerpStrong;
                    float setCardFrameX = (cardFramePositions[i].X * strongerLerp + (30 * (1 - strongerLerp))) * 0.9f + (30f + offset.X) * 0.1f;
                    float setCardFrameY = (cardFramePositions[i].Y * strongerLerp + (30 * (1 - strongerLerp))) * 0.9f + (30f - offset.Y) * 0.1f;
                    if (modPlayer.CardSlots <= i) { //when it's below the slots, we just imagine as if the offset doesn't exist
                        setCardFrameX = (cardFramePositions[i].X * strongerLerp + (30 * (1 - strongerLerp)));
                        setCardFrameY = (cardFramePositions[i].Y * strongerLerp + (30 * (1 - strongerLerp)));
                    }
                    Vector2 cardFramePosition = new Vector2(setCardFrameX, setCardFrameY);
                    float totalScale = modPlayer.scaleLerp * cardFrameScales[i];
                    if (i < modPlayer.Deck.Count) {
                        CardCache[i] = modPlayer.Deck[i];
                    } // for future me: what i'm doing here is caching the card,
                      //   because when you remove a card slot, it gets removed from the deck but then
                      //   you can't use the deck variable for the fade out of the card - therefore,
                      //   a simple cache suffices.

                    if (realID == 0) {
                        // GameInterfaceLayer gil = new GameInterfaceLayer("UI", InterfaceScaleType.UI);
                        // GameInterfaceLayer
                        if (distance <= modPlayer.GetAccessRange()) {
                            cardFrameRotationLerps[i] = cardFrameRotationLerps[i] * 0.9f + 0.1f;
                        } else {
                            cardFrameRotationLerps[i] = cardFrameRotationLerps[i] * 0.8f;
                        }
                    } else {
                        cardFrameRotationLerps[i] = cardFrameRotationLerps[i] * 0.8f;
                    }
                    float rotation = (FNLRotation.GetNoise(fnlx, 0) * 0.15f) * cardFrameRotationLerps[i];

                    int positionX = (int)(setCardFrameX + innX - (20f * totalScale - 20f) + (totalScale * 20f));
                    int positionY = (int)(setCardFrameY + innY - (20f * totalScale - 20f) + (totalScale * 40f));

                    cardDrawCall.Add(
                        new CardDrawCall(
                            i, setCardFrameY,
                            new Rectangle( //position
                                (int)(setCardFrameX + innX - (20f * totalScale - 20f) + (totalScale * 20f)),
                                (int)(setCardFrameY + innY - (20f * totalScale - 20f) + (totalScale * 40f)),
                                (int)(40 * totalScale),
                                (int)(40 * totalScale)),
                            CardCache[i].Rect(), // piece we're cutting off the spritesheet
                            rotation,
                            CardCache[i].Empty)); //rotation value, i think it's like per pi values


                    if (realID == 0) {
                        modPlayer.SpawnCardPosition = new Vector2(positionX, positionY);
                    }

                    /*
                https://discord.com/channels/103110554649894912/534215632795729922/1165795339316494406
                here, is the message link for projectile texture...

                        also, do a flip x effect for the card. you can do it by having some sort
                        of variable change the X values of the width and position, width to 0 and position
                        to match up with the width (you'll figure it out)

                    */
                    cardFramePositions[i] = cardFramePosition;
                }
                //cardFrame[i].SetImage = new UIImage(ModContent.Request<Texture2D>("UnoClass/Content/Items/Cards/RedOne")); // Frame of our resource bar
            }
            // Drawing the currently spawned-in card because i'm terrible at layering so i'ma just create a duplicate

            cardDrawCall = cardDrawCall.OrderByDescending(card => card.y).ToList();
            foreach (CardDrawCall carddraw in cardDrawCall) {
                spriteBatch.Draw(
                    (Texture2D)CardSpritesheet, //cached spritesheet texture
                    carddraw.rectangle, //position,
                    carddraw.source, // piece we're cutting off the spritesheet
                    (carddraw.empty ? cachedDeckItem.deckColor : Color.White).MultiplyRGBA(alphaColor), // color
                    carddraw.rotation, //rotation value, i think it's like per pi values
                    new Vector2(10f, 20f), //origin. this shit gonna SUCK to code
                    SpriteEffects.None, //sprite effect
                    0 //layer depth, honestly no idea what this shit does.. i guess it's layering in that specific spritebatch
                    );
            }

            if (modPlayer.CurrentCardProjectile != null) {
                if (modPlayer.CurrentCardProjectile.active) {
                    Vector2 visualProjectileLocation = modPlayer.CurrentCardProjectile.position - Main.screenPosition;
                    float totalScale = modPlayer.CurrentCardProjectileMod.placeScaleOffset;
                    Main.spriteBatch.Draw(
                        (Texture2D)CardSpritesheet,
                        new Rectangle(
                        (int)(visualProjectileLocation.X - 5f + (20 * (1 - totalScale))),
                        (int)(visualProjectileLocation.Y + (20 * (1 - totalScale))),
                        (int)(40 * totalScale),
                        (int)(40 * totalScale)),
                        modPlayer.CurrentCardData.Rect(),
                        Color.White * (1 / ((totalScale - 1) * 0.33f + 1)),
                        0,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0
                    );
                }
            }

            // Drawing first line of coins (current collected coins)
            // CoinsSplit converts the number of copper coins into an array of all types of coins
            //DrawCoins(spriteBatch, innX, innY, Utils.CoinsSplit(collectedCoins));
            // Drawing second line of coins (coins per minute) and text "CPM"
            //DrawCoins(spriteBatch, innX, innY, Utils.CoinsSplit(GetCoinsPerMinute()), 0, 25);
            //Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, "CPM", innX + (float)(24 * 4), innY + 25f, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
        }

        struct CardDrawCall {
            public int id;
            public float y;
            public Rectangle rectangle;
            public Rectangle source;
            public float rotation;
            public bool empty;

            public CardDrawCall( int id, float y, Rectangle rectangle, Rectangle source, float rotation, bool empty ) {
                this.y = y;
                this.id = id;
                this.rectangle = rectangle;
                this.source = source;
                this.rotation = rotation;
                this.empty = empty;
            }
        }

    }
}