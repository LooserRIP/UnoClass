using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using System.Globalization;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using UnoClass;
using ReLogic.Content;
using System;
using UnoClass.Content.DamageClasses;
using UnoClass.Common.Players;

namespace UnoClass.Common.GlobalNPCs
{
	public class NPCCards : GlobalNPC
	{ //this is for giving every npc an arbitrary card number
        public override bool InstancePerEntity => true; //this makes globalnpc run for every npc :D
		public int SpecialID;
        private float lerpAlpha;
        public UnoCard currentCard = new UnoCard(true);
        private float rotationLerp;
        public float scaleLerp = 1;
        public static Asset<Texture2D> CardSpritesheet;

        public override void SetStaticDefaults() { //per-globalnpc defaults, so get card spritesheet
            CardSpritesheet = ModContent.Request<Texture2D>("UnoClass/Content/Items/Spritesheets/Cards");
        }
        public override void SetDefaults(NPC entity) { //per-instance defaults, so randomize card
            randomizeCard();
            lerpAlpha = 1;
            SpecialID = Main.rand.Next(-1000000, 1000000);
        }

        public override void OnSpawn(NPC npc, IEntitySource source) {
            InitiateCard();
            base.OnSpawn(npc, source);
        }

        private void InitiateCard() {
            if (!currentCard.Empty) return;
            randomizeCard();
        }

        public void randomizeCard() {
            currentCard = new UnoCard(
                false, Main.rand.Next(0, 4), Main.rand.Next(0, 10));
        }
        public static bool IsValidForCard(NPC npc) { //this seems racist????
            if (npc == null) return false;
            if (npc.friendly) return false;
            if (npc.CountsAsACritter) return false;
            if (npc.lifeMax <= 2) return false;
            if (npc.isLikeATownNPC) return false;
            return true;
        }

        public Byte lerpByte(Byte a, Byte b, float t) {
            return (byte)(((int)a * (1 - t)) + ((int)b * (t)));
        }
        public float lerpFloat(float a, float b, float t) {
            return (a * (1 - t)) + (b * t);
        }
        public Color lerpColor(Color a, Color b, float t) {
            return new Color(lerpByte(a.R, b.R, t), lerpByte(a.G, b.G, t), lerpByte(a.B, b.B, t), a.A);
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
            if (!IsValidForCard(npc)) return;
            UnoModifiersPlayer modPlayer = Main.CurrentPlayer.GetModPlayer<UnoModifiersPlayer>();
            if (modPlayer.scaleLerp == 0) return; // (if player ain't holding an uno weapon, don't show)
            float alpha = 1;
            Color newColor = drawColor;
            System.Drawing.Color testColor = System.Drawing.Color.FromArgb(newColor.R, newColor.G, newColor.B, newColor.A);
            
            float brightness = (testColor.GetBrightness() - 0.5f) * 2f;
            newColor = lerpColor(newColor, Color.White, (brightness*0.5f));
            if (brightness <= 0.01f) {
                alpha = brightness / 0.01f;
            }
            float needScaleLerp = 1;
            if (modPlayer.CurrentCardProjectileModSave != null) {
                float distance = Vector2.DistanceSquared(modPlayer.CurrentCardProjectileModSave.Projectile.Center, npc.Center);
                float range = modPlayer.CurrentCardProjectileModSave.CalculateDamageRadius() / 4;
                if (distance <= range * range) {
                    needScaleLerp = 1.175f;
                }
            }
            if (scaleLerp == 0) scaleLerp = needScaleLerp;
            scaleLerp = MathHelper.Lerp(scaleLerp, needScaleLerp, 0.5f);
            float totalScale = (MathF.Pow(npc.width * npc.height, 1f/5f) / 3.8f) * modPlayer.scaleLerp * scaleLerp;

            Vector2 offset = new Vector2(-20, (totalScale * -40) - 10);
            Vector2 visualProjectileLocation = (new Vector2(npc.Hitbox.Center.ToVector2().X,npc.Hitbox.TopLeft().Y) + offset) - screenPos;
            lerpAlpha = lerpFloat(lerpAlpha, alpha, 0.08f);
            newColor = Color.Lerp(newColor, new Color(0, 0, 0, 0), 1 - lerpAlpha);
            rotationLerp = 0;
            spriteBatch.Draw(
                (Texture2D)CardSpritesheet,
                new Rectangle(
                    (int)(visualProjectileLocation.X + (20 * (1 - totalScale)) + (20 * totalScale)),
                    (int)(visualProjectileLocation.Y + (0 * (1 - totalScale)) + (20 * totalScale)),
                    (int)(40 * totalScale),
                    (int)(40 * totalScale)),
                currentCard.Rect(),
                newColor,
                rotationLerp,
                new Vector2(10, 10),
                SpriteEffects.None,
                0
                ); //Draws sprite over the empty projectile sprite
        }

    }
}
