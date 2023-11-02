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
using UnoClass.Common.Players;

namespace UnoClass.Content.Projectiles {
    public class CardProjectile : ModProjectile {
        private static Asset<Texture2D> CardSpritesheet;
        private static Asset<Texture2D>[] DustSpritesheets;
        public UnoCard card = new UnoCard(true,0,0);
        public Player player;
        public bool placed;
        public bool landed;
        private bool stopLerping;
        public int explosionTimer;
        public float placeScaleOffset = 1;
        private float disappearTimer;
        public int timer;
        public Vector2 lerpPosition;
        private Vector2 previousPosition;
        public bool render;
        private float totalDistance;
        private int updatePositionTimer;
        private HashSet<int> HitIDs;
        private int HitFrame;
        private bool tileCollide;

        public Item originalDeck;

// dust stuff
        private List<MyStupidCustomDust> customDusts;


        public override void SetStaticDefaults() {
            CardSpritesheet = ModContent.Request<Texture2D>("UnoClass/Content/Items/Spritesheets/Cards");
            DustSpritesheets = new Asset<Texture2D>[] {
                ModContent.Request<Texture2D>("UnoClass/Content/Dusts/FakeSmoke"),
                ModContent.Request<Texture2D>("UnoClass/Content/Dusts/IceShards"),
                ModContent.Request<Texture2D>("UnoClass/Content/Dusts/SnowMist")
            };
        }
        public override void SetDefaults() {
            Projectile.width = 28; // The width of projectile hitbox
            Projectile.height = 40; // The height of projectile hitbox

            Projectile.aiStyle = 0; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
            Projectile.DamageType = ModContent.GetInstance<UnoDamageClass>(); // What type of damage does this projectile affect?
            Projectile.friendly = false; // Can the projectile deal damage to enemies?
            Projectile.hostile = false; // Can the projectile deal damage to the player?
            Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
           // Projectile.light = 1f; // How much light emit around the projectile
            Projectile.tileCollide = false; // Can the projectile collide with tiles
            Projectile.timeLeft = 3600;

            tileCollide = false;
            lerpPosition = Projectile.Center;
            previousPosition = Projectile.Center;
            placed = false;
            landed = false;
            render = false;
            disappearTimer = 1;
            placeScaleOffset = 1;
            player = Main.player[Projectile.owner];
            customDusts = new List<MyStupidCustomDust>() { };
            HitIDs = new HashSet<int>() { };
            //player = Main.player[Projectile.owner];
        }
        public override void OnSpawn(IEntitySource source) {
            player = Main.player[Projectile.owner];
            originalDeck = player.HeldItem;
        }

        private bool solidTile(Point p) {
            if (!Main.tile[p].HasTile) return false;
            return Main.tileSolid[Main.tile[p].TileType];
        }
        public void CheckTileCollision() {
            if (tileCollide) return;
            Point[] points = new Point[] {
                Utils.ToTileCoordinates(Projectile.TopRight),
                Utils.ToTileCoordinates(Projectile.TopLeft),
                Utils.ToTileCoordinates(Projectile.BottomRight),
                Utils.ToTileCoordinates(Projectile.BottomLeft),
                Utils.ToTileCoordinates(Projectile.Center)
                };
            if (!solidTile(points[0]) && !solidTile(points[1]) && !solidTile(points[2]) && !solidTile(points[3]) && !solidTile(points[4])) { 
                tileCollide = true;
                Projectile.tileCollide = true;
                return;
            }
            lerpPosition = Main.CurrentPlayer.Center + new Vector2(0, -16);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if (!tileCollide) return false;
            if (placed) {
                stopLerping = true;
            }
            return false;
           // return base.OnTileCollide(oldVelocity);
        }
        public override void AI() {
            float projSpeed;
            timer++;
            float startupTime = 40f; //writing this in ticks, 60ticks = second :D
            if (landed) {
                disappearTimer -= 0.052f;
                if (disappearTimer <= 0f) {
                    disappearTimer = 0f;
                    float sum = customDusts.Sum(dust => dust.size);
                    if (sum == 0) {
                        Projectile.Kill();
                    }
                }
                foreach (MyStupidCustomDust dust in customDusts) {
                    dust.AI();
                }
            }
            if (Projectile.owner == Main.myPlayer) {
                if (landed) HitFrame++;
                if (!placed)
                    lerpPosition = Main.MouseScreen + Main.screenPosition;
                CheckTileCollision();
                if (!stopLerping && !landed) {
                    Projectile.netUpdate = true;
                    float distance = Vector2.Distance(lerpPosition, Projectile.Center);
                    float lerpSpeed = placed ? 0.7f : 0.3f;
                    projSpeed = distance * lerpSpeed * (MathF.Min(timer / startupTime, 0.5f) + 0.5f);
                    Projectile.velocity =
                        (lerpPosition - Projectile.Center).SafeNormalize(Vector2.Zero) *
                        MathF.Min(projSpeed, distance);
                    Projectile.rotation = Projectile.velocity.ToRotation();
                } else {
                    Projectile.velocity = Vector2.Zero;
                }
                if (!Main.CurrentPlayer.controlUseItem && !placed) {
                    setPlaced();
                    lerpPosition = Main.MouseScreen + Main.screenPosition;
                    if (Main.netMode == NetmodeID.MultiplayerClient) {
                        ModNetHandler.cardProjectile.SendCardPlaced( //Send the card that we PLACED so the other clients know
                            Main.myPlayer, //player id
                            Projectile.whoAmI, //projectile id
                            255); //who to send to server (255 means server)
                    }
                }
            } else {
                //Main.NewText("ayo this is projectile's owner is NOT the current player... fr");
                Projectile.velocity = Vector2.Zero;
            }


            totalDistance += Vector2.Distance(Projectile.Center, previousPosition);
            previousPosition = Projectile.Center;

            if (placed) {
                placeScaleOffset = MathF.Max(1, placeScaleOffset - 0.0375f);
                
                if (placeScaleOffset <= 1 && !landed) { //explode here
                    placeScaleOffset = 1;
                    land();
                }
            } else {
                placeScaleOffset = (placeScaleOffset * 0.9f) + (MathF.Min(1.3f, totalDistance / 160f + 1) * 0.1f);
            }

            //Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public void land() {
            landed = true;
            disappearTimer = 4f;
            SpawnDust();

            if (Projectile.owner == Main.myPlayer) {
                PunchCameraModifier modifier = new PunchCameraModifier(Projectile.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 5f, 4f, 16, 1000f, FullName);
                Main.instance.CameraModifiers.Add(modifier);
                LandDamage();
                if (card.SpecialCard == 2) { //spawn the gem projectiles here
                    for (int i = 0; i < 5; i++) {
                        float angle = i * (2f * MathF.PI / 5f);
                        Vector2 velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * 12f;
                        Projectile GemProjectile = Projectile.NewProjectileDirect(
                            Player.GetSource_None(),
                            Projectile.Center,
                            velocity,
                            ModContent.ProjectileType<GemBlitz>(),
                            60,
                            7f,
                            Projectile.owner);
                        GemProjectile.DamageType = ModContent.GetInstance<UnoDamageClass>();
                        GemBlitz GemProjectileMod = GemProjectile.ModProjectile as GemBlitz;
                        GemProjectileMod.gemid = i;
                    }
                }

            }
        }

        public void setPlaced() {
            placed = true;
            explosionTimer = 60; //by default the explosion timer is 60 seconds, it'll go down lower if enemies nearby
        }

        public override void PostDraw(Color lightColor) {
            if (card.Empty) return;

            //SMOKE PARTICLES :(
            if (landed) {
                RenderDust();
            }

            if (!render) return;

            Vector2 visualProjectileLocation = Projectile.Center - Main.screenPosition;
            //if (landed) visualProjectileLocation = Projectile.Center - Main.screenPosition;
            float totalScale = 1;
            float alpha = 1;
            totalScale *= placeScaleOffset;
            alpha /= ((placeScaleOffset - 1) * 0.33f + 1);
            if (landed) {
                if (disappearTimer <= 1f) {
                    alpha *= disappearTimer;
                    visualProjectileLocation.Y += (1 - disappearTimer) * 10f;
                }
            }
            int widthheight = 40;
            Color colorDraw = Color.White * alpha;
            float landedOffset = (widthheight * 0.5f);
            //if (landed) landedOffset = (widthheight * totalScale * 0.5f);
            Main.spriteBatch.Draw(
                (Texture2D)CardSpritesheet,
                new Rectangle(
                    (int)(visualProjectileLocation.X + ((widthheight*0.5f) * (1 - totalScale)) - landedOffset),
                    (int)(visualProjectileLocation.Y + ((widthheight*0.5f) * (1 - totalScale)) - landedOffset),
                    (int)(40 * totalScale),
                    (int)(40 * totalScale)),
                card.Rect(),
                colorDraw,
                0,
                new Vector2(0, 0),
                SpriteEffects.None,
                0
                );
            base.PostDraw(lightColor);
        }

        public override void OnKill(int timeLeft) {
            base.OnKill(timeLeft);
        }

        public float CalculateDamageRadius() {
            GlobalDecks deckItem;
            bool got = UnoSystem.GetDeckGlobal(originalDeck, out deckItem);
            float damageRadius = 0;
            if (got) {
                damageRadius = deckItem.impactRadius *
                    Main.player[Projectile.owner].GetModPlayer<UnoModifiersPlayer>().ImpactRangeMult;
            }
            return damageRadius;
        }

        public void LandDamage() {
            int damageRadius = (int)MathF.Round(CalculateDamageRadius());
            Projectile.tileCollide = false;
            Projectile.Resize(damageRadius, damageRadius);
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.knockBack = player.GetWeaponKnockback(originalDeck);
            Projectile.damage = player.GetWeaponDamage(originalDeck);

            Projectile.appliesImmunityTimeOnSingleHits = true;
            HitFrame = 0;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            if (!target.active) return;
            NPCCards npc;
            bool got = target.TryGetGlobalNPC<NPCCards>(out npc);
            if (!got) return;
            if (!HitIDs.Contains(npc.SpecialID)) {
                HitIDs.Add(npc.SpecialID);
            }
            if (Projectile.owner == Main.myPlayer && card.SpecialCard == 1) { //snowball freeze
                Main.NewText("adding debuff to " + target.FullName + ", " + target.whoAmI);
                target.AddBuff(BuffID.Slow, Main.rand.Next(60, 180), false);
                target.AddBuff(BuffID.Frostburn, Main.rand.Next(60, 180), false);
            }
            base.OnHitNPC(target, hit, damageDone);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            if (!target.active) return;
            NPCCards npc;
            bool got = target.TryGetGlobalNPC<NPCCards>(out npc);
            if (!got) return;
            if (!HitIDs.Contains(npc.SpecialID)) {
                HitIDs.Add(npc.SpecialID);
            }
            modifiers.HitDirectionOverride = (target.Center.X >= Projectile.Center.X) ? 1 : -1;
            if (card.SpecialCard == 0) {
                if (npc.currentCard.Value == card.Value) {
                    modifiers.SourceDamage += 0.5f;
                    if (npc.currentCard.Color == card.Color) {
                        modifiers.SetCrit();
                    } else {
                        modifiers.DisableCrit();
                    }
                } else {
                    modifiers.DisableCrit();
                }
            } else {
                switch (card.SpecialCard) {
                    case 1:
                        modifiers.Knockback += 0.5f;
                        break;
                }
            }
            base.ModifyHitNPC(target, ref modifiers);
        }
        public override bool? CanHitNPC(NPC target) {
            if (!landed) return false;
            if (HitFrame > 3) return false;
            if (!target.active) return false;
            NPCCards npc;
            bool got = target.TryGetGlobalNPC<NPCCards>(out npc);
            if (!got) {
                return null;
            }
            if (HitIDs.Contains(npc.SpecialID)) return false;
            UnoCard enemyCard = npc.currentCard;
            bool can = false;
            if (!NPCCards.IsValidForCard(target)) can = true;
            if (enemyCard.Color == card.Color) can = true;
            if (enemyCard.Value == card.Value) can = true;
            if (card.SpecialCard == 1) can = true; //snowball card
            if (!can) return false;
            float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
            //Main.NewText("Target " + target.TypeName + " distance: " + sqrDistanceToTarget, new Color(50,78,228));
            if (sqrDistanceToTarget < 5000) {
                return null;
            }
            return false;
        }

        //dust

        public void RenderDust() {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);
            int i = 0;
            foreach (MyStupidCustomDust dust in customDusts) {
                dust.Render(Main.spriteBatch, GetSpritesheet(i));
                float mult = dust.color == Color.White ? 0.3f : 0.7f;
                mult *= dust.size;
                Lighting.AddLight(dust.position, dust.color.R / 255f * mult, dust.color.G / 255f * mult, dust.color.B / 255f * mult);
                i++;
                if (i == (card.SpecialCard == 0 ? 20 : 15)) {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            if (i < (card.SpecialCard == 0 ? 20 : 15)) {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        public Asset<Texture2D> GetSpritesheet(int i) {
            if (card.SpecialCard == 0) return DustSpritesheets[0];
            if (card.SpecialCard == 1) {
                if (i <= 15) {
                    return DustSpritesheets[0];
                }
                if (i <= 25) { //ice shards
                    return DustSpritesheets[1];
                } //snow mist
                return DustSpritesheets[2];
            }
            return DustSpritesheets[0];

        }

        public void SpawnDust() {
            float sizeMult = 1;
            float speed = 4;
            int amt = 15;
            if (card.SpecialCard == 1) amt = 45;
            Vector2 posSpawn = Projectile.Center;
            Color cardColor = card.ToColor();
            for (int i = 0; i < 15; i++) {
                float angle = Main.rand.NextFloat(0f, 360f);
                float brightness = Main.rand.NextFloat(0.9f, 1f);
                Vector2 dir = (new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle)).SafeNormalize(Vector2.Zero)) * speed * Main.rand.NextFloat(0.9f, 1.15f);
                customDusts.Add(new MyStupidCustomDust(0, //(type)
                    posSpawn,
                    Main.rand.NextFloat(0.75f, 1.25f) * sizeMult,
                    Main.rand.Next(0, 3),
                    dir,
                    new Color((byte)(255 * brightness), (byte)(255 * brightness), (byte)(255 * brightness))
                    ));
            }
            speed = 2.7f;
            for (int i = 0; i < amt; i++) {
                float angle = Main.rand.NextFloat(0f, 360f);
                float brightness = Main.rand.NextFloat(0.9f, 1f);
                int frame = Main.rand.Next(0, 3);
                byte type = 0;
                Color color = new Color((byte)(cardColor.R * brightness), (byte)(cardColor.G * brightness), (byte)(cardColor.B * brightness));
                Vector2 dir = (new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle)).SafeNormalize(Vector2.Zero)) * speed * Main.rand.NextFloat(0.85f, 1.15f);
                if (card.SpecialCard == 1) {
                    color = Color.White;
                    type = 1;
                    speed = 2.7f;
                    if (i >= 11) {
                        type = 2;
                        frame = Main.rand.Next(0, 4);
                        speed = 1.7f;
                        sizeMult = (i - 10 / 10f) + 0.9f;
                    } else {
                        dir = new Vector2(speed * Main.rand.NextFloat(0.85f, 1.15f), 0);
                    }
                }
                if (card.SpecialCard == 2) {
                    color = new UnoCard(false, Main.rand.Next(0, 4), 0, 0).ToColor();
                }
                
                customDusts.Add(new MyStupidCustomDust(type, //(type)
                    posSpawn,
                    Main.rand.NextFloat(0.75f, 1.25f) * sizeMult,
                    frame,
                    dir,
                    color
                    ));
            }
        }

    }
}
