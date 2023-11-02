using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using UnoClass.Common.Players;
using UnoClass.Content.Items;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.Localization;
using UnoClass.Content.DamageClasses;
using Terraria.Chat;
using System;
using Terraria.Utilities;
using ReLogic.Content;

namespace UnoClass.Common.UI.DeckWheelUI
{
	internal class DeckWheelUI : UIState
	{
        private UIText text;
		private UIElement area;
        private UIImage barFrame;
		private DeckWheel deckWheel;
        private UIImage[] cardFrame;
		private Vector2[] cardFramePositions;
		private float offsetLerp = 0;
        private Color gradientA;
		private Color gradientB;
		private int prevCardSlots = -1;
		private UnoModifiersPlayer modPlayer;


        public override void OnInitialize() {
            area = new UIElement();
			SetRectangle(area, left: -215, top: -210, width: 50, height: 50);
            area.HAlign = 0.5f;
            area.VAlign = 0.5f;

            //barFrame = new UIImage(ModContent.Request<Texture2D>("UnoClass/Common/UI/DeckWheelUI/Wheel"));
            //SetRectangle(barFrame, left: 0, top: 0, width: 75, height: 75);

            deckWheel = new DeckWheel();
            SetRectangle(deckWheel, left: 0, top: 0, width: 75, height: 75);

            //area.Append(barFrame);
            area.Append(deckWheel);

            cardFrame = new UIImage[10];
			cardFramePositions = new Vector2[10];
            for (int i = 0; i < 10; i++) {
				/*
				cardFrame[i] = new UIImage(ModContent.Request<Texture2D>("UnoClass/Content/Items/Cards/RedOne"));
                cardFrame[i].Left.Set(30f, 0f);
                cardFrame[i].Top.Set(30f, 0f);
                cardFrame[i].Width.Set(30, 0f);
                cardFrame[i].Height.Set(30, 0f);
				area.Append(cardFrame[i]);
				*/
				cardFramePositions[i] = new Vector2(30, 30);
            }

            gradientA = new Color(123, 25, 138); // A dark purple
			gradientB = new Color(187, 91, 201); // A light purple
            Append(area);
		}

        private void SetRectangle(UIElement uiElement, float left, float top, float width, float height) {
            uiElement.Left.Set(left, 0f);
            uiElement.Top.Set(top, 0f);
            uiElement.Width.Set(width, 0f);
            uiElement.Height.Set(height, 0f);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            modPlayer = Main.CurrentPlayer.GetModPlayer<UnoModifiersPlayer>();
            if (modPlayer != null) {
				if (modPlayer.scaleLerp == 0) {
					if (deckWheel == null) return;
                    deckWheel.ResetVariables();
                    return;
                }
				//barFrame.ImageScale = modPlayer.scaleLerp;
			}
			base.Draw(spriteBatch);
		}

		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);
        }

		public override void Update(GameTime gameTime) {

			if (!Main.CurrentPlayer.HeldItem.CountsAsClass(ModContent.GetInstance<UnoDamageClass>()))
				return;


            base.Update(gameTime);
		}
	}

	// This class will only be autoloaded/registered if we're not loading on a server
	[Autoload]
	internal class DeckWheelUISystem : ModSystem
	{
		private UserInterface DeckUserInterface;

		internal DeckWheelUI DeckWheelUI;

		public static LocalizedText NameOfInterfaceLocalized { get; private set; }

		public override void Load() {
			DeckWheelUI = new();
			DeckUserInterface = new();
			DeckUserInterface.SetState(DeckWheelUI);

			string category = "UI";
			NameOfInterfaceLocalized ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{category}.DeckWheelUI"));
		}

		public override void UpdateUI(GameTime gameTime) {
			DeckUserInterface?.Update(gameTime);
		}

		
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1) {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "UnoClass: Deck Wheel",
                    delegate {
                        DeckUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.Game)
                );
            }
		}
		
	}
}
