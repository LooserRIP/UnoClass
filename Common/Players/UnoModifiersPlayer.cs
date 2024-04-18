using UnoClass.Content.Items;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Linq;
using Terraria.Utilities;
using System.Diagnostics;
using UnoClass.Content.DamageClasses;
using UnoClass.Content.Projectiles;
using UnoClass.Networking;
using UnoClass;
using UnoClass.Common.GlobalItems;
using Terraria.Chat;

namespace UnoClass.Common.Players
{
	internal class UnoModifiersPlayer : ModPlayer {
        public int CardSlots = 0;
        public float RegenerationSpeed;
		public float AccessRange;

        public float AccessRangeMult;
        public float RegenerationMult;
        public float ImpactRangeMult;

        // when reshufflespeed's at 1, it'll take 1 second to shuffle a new card,
        // when it's 2, it'll take 0.5 seconds. etc.
        public int DeckTimer = 0;

        //actual deck vars
        public int offsetDeck = 0;
        public int deckRotation = 0;
        public List<UnoCard> Deck = new List<UnoCard>() { };
        private int EmptyCards;
        public Vector2 SpawnCardPosition;
        public Projectile CurrentCardProjectile;
        public CardProjectile CurrentCardProjectileMod;
        public CardProjectile CurrentCardProjectileModSave;
        public UnoCard CurrentCardData;
        private int SkippedCards;

        //stat variables
        public bool SkipCardKnockback;

		public override void PreUpdate() {
            UpdateLerp();
			// Timers and cooldowns should be adjusted in PreUpdate
		}
        public override void PostUpdate() {
            if (Deck.Count > CardSlots) { //if there are more cards in the deck than card slots, trim the deck\
                for (int i = CardSlots; i < Deck.Count; i++) { //account for empty cards counter
                    if (Deck[i].Empty) {
                        EmptyCards--;
                    }
                }
                Deck = Deck.Take(CardSlots).ToList();
            }
			if (Deck.Count < CardSlots) {
				Deck.Add(new UnoCard(true, 0, 0));
                EmptyCards++;
			}

        }

        public override void PostUpdateMiscEffects() {
            UpdateDeck();
            UpdateCurrentCardProjectile();
            UpdateCurrentCardProjectileSave();
        }

        public override void OnRespawn() {
            Deck = new List<UnoCard>() { };
            EmptyCards = 0;
            SkippedCards = 0;
            DeckTimer = 0;
            offsetDeck = 0;
            deckRotation = 0;
            PostUpdate();
        }

        public void UpdatePlayerStats() {
            AccessRange = 0;
            AccessRangeMult = 1;
            RegenerationMult = 1;
            RegenerationSpeed = 1;
            ImpactRangeMult = 1;

            GlobalDecks deckItem;
            if (UnoSystem.GetDeckGlobal(Player.HeldItem, out deckItem)) {
                AccessRange = deckItem.deckRange;
                CardSlots = deckItem.cardSlots;
                AccessRangeMult += ((deckItem.deckRangeMultiplier - 1));
                RegenerationMult += ((deckItem.deckRegenerationMultiplier - 1));
            }
            CardSlots = Math.Max(1, Math.Min(10, CardSlots)); //clamp it where it's needed

            SkipCardKnockback = false;
        }

        public float scaleLerp = 0;
        public float scaleLerpStrong = 0;
        public void UpdateLerp() {
            if ((!UnoSystem.IsDeck(Player.HeldItem.type)) || Player.dead) {
                scaleLerp = Math.Max(0, scaleLerp * 0.9f - 0.05f);
                scaleLerpStrong = Math.Max(0,scaleLerpStrong * 0.8f - 0.1f);
            } else {
                scaleLerp = scaleLerp * 0.9f + 0.1f;
                scaleLerpStrong = scaleLerpStrong * 0.8f + 0.2f;
            }
            if (scaleLerp < 0.001f) {
                scaleLerp = 0f;
                scaleLerpStrong = 0f;
                offsetDeck = 0;
                deckRotation = 0;
            }
            if (scaleLerp > 0.999f) {
                scaleLerp = 1f;
                scaleLerpStrong = 1f;
            }
        }

        public void SpinDeck() {
            deckRotation++;
        }

		public void UpdateDeck() {
            if (CurrentCardProjectileModSave == null) {
                DeckTimer++;
            }
            if (!UnoSystem.IsDeck(Player.HeldItem.type) || EmptyCards == 0) {
                DeckTimer = 0;
                if (EmptyCards == 0) {
                    SkippedCards = 0;
                }
            }
            if (DeckTimer > (60f / Math.Max(RegenerationSpeed * RegenerationMult, 0.01f)) * (SkippedCards > 0 ? 0.75f : 1f)) {
                DeckTimer = 0;
				int foundid = -1;
                for (int idt = 0; idt < CardSlots; idt++) {
                    int realid = GetIndex(idt);
                    if (realid >= Deck.Count) continue;
                    if (Deck[realid].Empty) {
                        foundid = realid;
                        break;
                    }
                }
				if (foundid != -1) {
                    //shuffle a new card
                    Deck[foundid] = RollCard();
                    SkippedCards--;
                    EmptyCards--;
                }
            }
        }
        public string ColorName(int color) {
            if (color == 0) return "Red";
            if (color == 1) return "Blue";
            if (color == 2) return "Yellow";
            return "Green";
        }
        public float GetAccessRange() {
            return (AccessRange ) * AccessRangeMult;
        }
        public UnoCard RollCard() {
            int holdID = UnoSystem.GetDeckID(Player.HeldItem);
            int value = Main.rand.Next(0, 10);
            int color = Main.rand.Next(0, 4);
            int special = 0;
            switch (holdID) {
                case 2: //Frozen Deck, give snowball card (Special ID 1)
                    if (Main.rand.Next(0, 20) == 0) special = 1;
                    break;
                case 4: //Gemstone Deck, give gem blitz card (special id 2)
                    if (Main.rand.Next(0, 20) == 0) special = 2;
                    break;

            }
            UnoCard newCard = new UnoCard(false, color, value, special);
            return newCard;
        }
        public bool UseDeck(Item item) {
            float distance = DeckWheelDistance();
            if (distance <= GetAccessRange()) {
                int newIndex = GetIndex(0);
                if (!Deck[newIndex].Empty) {
                    //using the card
                    //emptying it from the deck
                    SpawnCard(Deck[newIndex].Clone(), item);
                    Deck[newIndex] = new UnoCard(true);
                    EmptyCards++;
                    LookForNewIndex();
                    return true;
                }
            }
            return true;
        }
        public void SpawnCard(UnoCard card, Item deck) {
            //    ModContent.ProjectileType<CardProjectile>()
            Vector2 spawnPos = (SpawnCardPosition + new Vector2(0,-20)) + Main.screenPosition;
            CurrentCardProjectile = Projectile.NewProjectileDirect(
                Player.GetSource_ItemUse(deck),
                spawnPos,
                Vector2.Zero,
                ModContent.ProjectileType<CardProjectile>(),
                deck.damage,
                deck.knockBack,
                Player.whoAmI);
            CurrentCardProjectile.DamageType = ModContent.GetInstance<UnoDamageClass>();
            CurrentCardData = card;
            CurrentCardProjectileMod = CurrentCardProjectile.ModProjectile as CardProjectile;
            CurrentCardProjectileModSave = CurrentCardProjectileMod;
            CurrentCardProjectileMod.card = card;
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModNetHandler.cardProjectile.SendCardSpawn( //Send the card that we spawned so the other clients know
                    Player.whoAmI, //player id
                    CurrentCardProjectile.identity, //projectile id
                    card, //projectile's card
                    255); //who to send to server (255 means server)
            }
            // card packet here, figure out how to do this tmr ^_^
        }
        public void UpdateCurrentCardProjectile() {
            if (CurrentCardProjectile == null) return;
            if (!CurrentCardProjectile.active) {
                CurrentCardProjectileMod.render = true;
                CurrentCardProjectile = null;
                return;
            }
            if (CurrentCardProjectileMod.placed) {
                CurrentCardProjectileMod.render = true;
                CurrentCardProjectile = null;
                return;
            }
            if (Vector2.Distance(CurrentCardProjectile.Center, UnoSystem.DeckPositionWorld()) > 100) {
                CurrentCardProjectile = null;
                CurrentCardProjectileMod.render = true;
                return;
            } 
        }
        public void UpdateCurrentCardProjectileSave() {
            if (CurrentCardProjectileModSave == null) return;
            if (CurrentCardProjectileModSave.Projectile == null) {
                CurrentCardProjectileModSave = null;
                return;
            }
            if (!CurrentCardProjectileModSave.Projectile.active) {
                CurrentCardProjectileModSave = null;
                return;
            }
            if (CurrentCardProjectileModSave.landed) {
                CurrentCardProjectileModSave = null;
                return;
            }
        }
        public bool SkipDeck(Item item) {
            int newIndex = GetIndex(0);
            if (!Deck[newIndex].Empty) {
                //emptying the card from the deck
                SkippedCards++;
                Deck[newIndex] = new UnoCard(true, 0, 0);
                EmptyCards++;
                LookForNewIndex();
                if (SkipCardKnockback) SkipCardKnockbackActivate();
                return true;
            }
            return true;
        }

        public void SkipCardKnockbackActivate() {
            Main.NewText("I didn't code functionality for this fuckkkk");
        }

        public int GetIndex(int ind) {
            return GetIndex(ind, offsetDeck);
        }
        public int GetIndex(int ind, int offset) {
            return ((CardSlots - ((ind + offset) % CardSlots))) % CardSlots;
        }
        public float DeckWheelDistance() {
            Point tileLocation = Main.CurrentPlayer.Center.ToTileCoordinates();
            float distance = Vector2.Distance(
            (Main.MouseScreen),
            UnoSystem.DeckPosition());
            distance /= 16f;
            //distance /= Main.GameZoomTarget;
            return distance;
        }
        public void LookForNewIndex() {
            for (int i = 1; i < CardSlots; i++) {
                int newIndexCheck = GetIndex(0, i + offsetDeck);
				if (!Deck[Math.Abs(newIndexCheck % CardSlots)].Empty) {
					offsetDeck += i;
					return;
				}
            }
			offsetDeck += 1;
        }

        public List<UnoCard> CreateShiftedList(List<UnoCard> list, int offset) {
            int length = list.Count;
            List<UnoCard> shiftedList = new List<UnoCard>(length);
            offset = offset % length;
            for (int i = 0; i < length; i++) {
                int newIndex = (i + offset) % length;
                shiftedList.Add(list[newIndex]);
            }
            return shiftedList;
        }

        public override void ResetEffects() {
            UpdatePlayerStats();
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			/*
			if (AdditiveCritDamageBonus > 0) {
				modifiers.CritDamage += AdditiveCritDamageBonus;
			}*/
		}
	}
}
