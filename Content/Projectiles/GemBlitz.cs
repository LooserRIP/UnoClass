using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using UnoClass.Content.DamageClasses;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using UnoClass.Networking;
using Terraria.Graphics.CameraModifiers;
using System.Linq;
using UnoClass.Common.GlobalNPCs;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Steamworks;
using Terraria.DataStructures;
using UnoClass.Common.GlobalItems;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using Terraria.Map;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace UnoClass.Content.Projectiles {
    public class GemBlitz : ModProjectile {
        private static Asset<Texture2D> GemSpritesheet;
        private static Tuple<float, float, float>[] LightColors;
        private List<int> entityIDIgnore;
        public Player player;
        public int gemid;

        public override void SetStaticDefaults() {
            GemSpritesheet = ModContent.Request<Texture2D>("UnoClass/Content/Projectiles/GemBlitzSpritesheet");
            LightColors = new Tuple<float, float, float>[] {
                new Tuple<float,float,float>(238f / 255f, 51f/255f, 53f/255f),//ruby
                new Tuple<float,float,float>(25f / 255f, 33f/255f, 191f/255f),//sapphire
                new Tuple<float,float,float>(1f, 198f/255f, 0f), //topaz
                new Tuple<float,float,float>(33f / 255f, 184f/255f, 115f/255f), //emerald
                new Tuple<float,float,float>(218f / 255f, 185f/255f, 210f/255f) //diamond
            };
        }
        public override void SetDefaults() {
            Projectile.width = 28; // The width of projectile hitbox
            Projectile.height = 40; // The height of projectile hitbox

            Projectile.aiStyle = 0; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
            Projectile.DamageType = ModContent.GetInstance<UnoDamageClass>(); // What type of damage does this projectile affect?
            Projectile.friendly = true; // Can the projectile deal damage to enemies?
            Projectile.hostile = false; // Can the projectile deal damage to the player?
            Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
            Projectile.tileCollide = false; // Can the projectile collide with tiles
            Projectile.penetrate = 5;
            Projectile.knockBack = 7f;
            Projectile.damage = 30;
            Projectile.timeLeft = 200;
            player = Main.player[Projectile.owner];
            entityIDIgnore = new List<int>() { };
            //player = Main.player[Projectile.owner];
        }
        public override void OnSpawn(IEntitySource source) {
            player = Main.player[Projectile.owner];
        }
        private int cycle = 0;
        public override void AI() {
            int DustType = DustID.GemDiamond;
            switch (gemid) {
                case 0: DustType = DustID.GemRuby; break;
                case 1: DustType = DustID.GemSapphire; break;
                case 2: DustType = DustID.GemTopaz; break;
                case 3: DustType = DustID.GemEmerald; break;
            }
            cycle++;
            if (cycle % 2 == 0) {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustType, 0f, 0f, 100, default, 0.75f);
                dust.noGravity = false;
                dust.velocity = new Vector2(0,-1.5f);
            }
            if (Projectile.owner == Main.myPlayer) {
                float maxDetectRadius = 600f;
                if (gemid == 4) maxDetectRadius *= 2;
                float projSpeed = 17f + (Projectile.numHits * 5f);

                NPC closestNPC = FindClosestNPC(maxDetectRadius);
                if (closestNPC == null) {
                    Projectile.velocity *= 0.97f;
                    return;
                }

                if (Main.netMode == NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity,(closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * projSpeed,0.04f);
                Projectile.rotation = Projectile.velocity.ToRotation();
            } else {
                Projectile.netUpdate = false;
                Projectile.velocity = Vector2.Zero;
            }
            // add some lighting ^_^
            Lighting.AddLight(Projectile.Center, LightColors[gemid].Item1, LightColors[gemid].Item2, LightColors[gemid].Item3);
        }

        // Finding the closest NPC to attack within maxDetectDistance range
        // If not found then returns null
        public NPC FindClosestNPC(float maxDetectDistance) {
            NPC closestNPC = null;

            // Using squared values in distance checks will let us skip square root calculations, drastically improving this method's speed.
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            // Loop through all NPCs(max always 200)
            for (int k = 0; k < Main.maxNPCs; k++) {
                NPC target = Main.npc[k];
                if (target.CanBeChasedBy() && IsValidNPC(target)) {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    if (gemid == 4) {
                        sqrDistanceToTarget = 10000 - target.life;
                    }
                    if (sqrDistanceToTarget < sqrMaxDetectDistance) {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }

            return closestNPC;
        }


        public override void PostDraw(Color lightColor) {
            Vector2 visualProjectileLocation = Projectile.Center - Main.screenPosition;
            //if (landed) visualProjectileLocation = Projectile.Center - Main.screenPosition;
            float totalScale = 2;
            float alpha = 1;
            int width = 11;
            int height = 9;
            Color colorDraw = Color.White * alpha;
            float landedOffsetX = (width * 0.5f) - (width * 0.5f * totalScale);
            float landedOffsetY = (height * 0.5f) - (height * 0.5f * totalScale);
            //if (landed) landedOffset = (widthheight * totalScale * 0.5f);
            Main.spriteBatch.Draw(
                (Texture2D)GemSpritesheet,
                new Rectangle(
                    (int)(visualProjectileLocation.X + ((width*0.5f) * (1 - totalScale)) - landedOffsetX),
                    (int)(visualProjectileLocation.Y + ((height*0.5f) * (1 - totalScale)) - landedOffsetY),
                    (int)(11f * totalScale),
                    (int)(9f * totalScale)),
                new Rectangle(11 * gemid, 0, 11, 9),
                colorDraw,
                Projectile.rotation,
                new Vector2(5.5f, 4.5f),
                SpriteEffects.None,
                0
                );
            base.PostDraw(lightColor);
        }

        public override void OnKill(int timeLeft) {
            int DustType = DustID.GemDiamond;
            switch (gemid) {
                case 0: DustType = DustID.GemRuby; break;
                case 1: DustType = DustID.GemSapphire; break;
                case 2: DustType = DustID.GemTopaz; break;
                case 3: DustType = DustID.GemEmerald; break;
            }
            for (int i = 0; i < 15; i++) {    
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustType, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 5f;
            }
            base.OnKill(timeLeft);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            if (!IsValidNPC(target)) return;
            entityIDIgnore.Add(target.GetGlobalNPC<NPCCards>().SpecialID);
            if (entityIDIgnore.Count >= (gemid == 4 ? 2 : 3)) entityIDIgnore.RemoveAt(0);
            base.OnHitNPC(target, hit, damageDone);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            modifiers.HitDirectionOverride = (target.Center.X >= Main.player[Projectile.owner].Center.X) ? 1 : -1;
            base.ModifyHitNPC(target, ref modifiers);
        }
        public bool IsValidNPC(NPC target) {
            if (!target.active) return false;
            NPCCards npc;
            bool got = target.TryGetGlobalNPC<NPCCards>(out npc);
            if (!got) {
                return false;
            }
            UnoCard enemyCard = npc.currentCard;
            bool can = false;
            if (!NPCCards.IsValidForCard(target)) return false;
            if (entityIDIgnore.Contains(npc.SpecialID)) return false;
            if (enemyCard.Color == gemid) can = true;
            if (gemid == 4) can = true;
            if (!can) return false;
            return true;
        }
        public override bool? CanHitNPC(NPC target) {
            if (IsValidNPC(target)) return null;
            return false;
        }

    }
}
