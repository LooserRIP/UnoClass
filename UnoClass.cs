using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using UnoClass.Common.UI;
using UnoClass.Networking;

namespace UnoClass
{
	public class UnoClass : Mod {
        internal static UnoClass mod;
		public static UnoClass Instance = ModContent.GetInstance<UnoClass>();

        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            ModNetHandler.HandlePacket(reader, whoAmI);
        }
        public override void Load() {
            mod = this;
        }
    }

    public struct SpecialCardInfo {
        public string Name;
        public int ItemID;
        public string Explanation;
        public Color Color;
        public SpecialCardInfo(string Name, int ItemID, string Explanation, Color Color) {
            this.Name = Name;
            this.ItemID = ItemID;
            this.Explanation = Explanation;
            this.Color = Color;
        }
    }
	public struct UnoCard {
		public bool Empty;
        public int SpecialCard;
		public int Color;
		public int Value;
        //Special Values:
        // 10 - Stop
        // 11 - Flip
        // 12 - +2
        public UnoCard(bool _Empty, int _Color, int _Value) {
            Empty = _Empty;
            Color = _Color;
            Value = _Value;
            SpecialCard = 0;
        }
        public UnoCard(bool _Empty, int _Color, int _Value, int _SpecialCard) {
            Empty = _Empty;
            Color = _Color;
            Value = _Value;
            SpecialCard = _SpecialCard;
        }
        public UnoCard(bool _Empty) {
			Empty = _Empty;
			Color = 0;
			Value = 0;
            SpecialCard = 0;
		}
		public UnoCard EmptyCard() {
			return new UnoCard(true, 0, 0);
        }
        public static Rectangle Rect(int x, int y) {
            return new Rectangle(x * 24 + 2, y * 24 + 2, 20, 20);
        }
        public Rectangle Rect() {
            if (Empty) return Rect(0, 4);
            if (SpecialCard > 0) {
                switch (SpecialCard) {
                    case 1:
                        return Rect(0, 5);
                    case 2:
                        return Rect(1, 5);
                }
            }
            return Rect(Value, Color);
        }
        //SPECIAL CARD AMOUNT: 1
        static public UnoCard FromPacket(byte input) {
			if (input.Equals(0)) return new UnoCard(true, 0, 0); //guys if it's empty i mean if it's 0 then it's uh empty
            if (input > 0 && input <= 2) {
                return new UnoCard(false, 0, 0, input);
            }
            int inp = (int)(input - 1) - 2;
            int val = inp % 10;
            int color = (inp - val) / 10; //fuck you emerald
            return new UnoCard(false, color, val);
        }
        public byte ToPacket() {
			if (Empty) return 0; //if it's empty nothing else matters :)
            if (SpecialCard > 0) return (byte)SpecialCard;
			return (byte)(((Color * 10) + Value + 1) + 2);
        }
		public Color ToColor() {
            if (Color == 0)
                return new Microsoft.Xna.Framework.Color(232, 59, 59);
            if (Color == 1)
                return new Microsoft.Xna.Framework.Color(77, 155, 230);
            if (Color == 2)
                return new Microsoft.Xna.Framework.Color(249, 194, 43);
            if (Color == 3)
                return new Microsoft.Xna.Framework.Color(30, 188, 115);
            return Microsoft.Xna.Framework.Color.White;
		}
        public UnoCard Clone() {
			return new UnoCard(Empty, Color, Value, SpecialCard);
		}
	}

    public class MyStupidCustomDust {
        public byte type;
        public Vector2 position;
        public float size;
        public int frame;
        public Vector2 velocity;
        public Color color;
        public bool active;
        public float rotation;
        private float value;
        private Color originalColor;

        public MyStupidCustomDust(byte type, Vector2 position, float size, int frame, Vector2 velocity, Color color) {
            this.type = type;
            this.position = position - (Vector2.One * GetFrameSize() * 0.5f);
            this.size = size;
            this.frame = frame;
            this.velocity = velocity;
            this.color = color;
            originalColor = color;
            this.active = true;
            this.rotation = 0;
            this.value = 0;
            OnSpawn();
        }
        public static Color Alpha(float alpha) {
            return new Color(alpha, alpha, alpha, alpha);
        }

        public int GetFrameSize() {
            switch (type) {
                case 1: //ice shards
                    return 13;
            }
            return 10; //default
        }
        public Rectangle GetRect() {
            int frameSize = GetFrameSize();
            return new Rectangle(0, frameSize * frame, frameSize, frameSize);
        }
        public void OnSpawn() {
            switch (type) {
                case 0: //smoke
                    rotation = MathF.PI * 2 * Main.rand.NextFloat();
                    value = Main.rand.NextFloat(0, 1f);
                    break;
                case 1: //ice shards
                    rotation = MathF.PI * 2 * Main.rand.NextFloat();
                    velocity = new Vector2(MathF.Sin(rotation), MathF.Cos(rotation)) * velocity.Length();
                    break;
                case 2: //snow mist
                    value = Main.rand.NextFloat(0.8f, 1.2f);
                    value /= size;
                    color = originalColor.MultiplyRGBA(Alpha(value));
                    break;
            }
        }
        
        public void AI() {
            switch (type) {
                case 0: //smoke
                    rotation += velocity.Length() * 0.1f * value;
                    position += velocity;
                    velocity *= 0.94f;
                    if (velocity.LengthSquared() <= 0.05f) {
                        size *= 0.95f;
                        if (size <= 0.01f) size = 0;
                    }
                    break;
                case 1: //ice shards
                    position += velocity;
                    velocity *= 0.94f;
                    if (velocity.LengthSquared() <= 0.05f) {
                        size *= 0.95f;
                        if (size <= 0.01f) size = 0;
                        color = originalColor.MultiplyRGBA(Alpha(size));
                    }
                    break;
                case 2: //snow mist
                    position += velocity;
                    velocity *= 0.94f;
                    if (velocity.LengthSquared() <= 0.05f) {
                        value *= 0.95f;
                    }
                    color = originalColor.MultiplyRGBA(Alpha(value));
                    break;
            }
        }

        public void Render(SpriteBatch spriteBatch, Asset<Texture2D> texture) {
            if (!active) return;
            Vector2 visualPosition = position - Main.screenPosition;
            int frameSize = GetFrameSize();
            spriteBatch.Draw(
                (Texture2D)texture,
                new Rectangle(
                    (int)(visualPosition.X + (frameSize * 0.5f * (1 - size)) + (frameSize * 0.5f * size)),
                    (int)(visualPosition.Y + (frameSize * 0.5f * (1 - size)) + (frameSize * 0.5f * size)),
                    (int)(frameSize * size),
                    (int)(frameSize * size)),
                GetRect(),
                color,
                rotation,
                new Vector2(frameSize / 2f, frameSize / 2f),
                SpriteEffects.None,
                0
                );
        }

    }

}