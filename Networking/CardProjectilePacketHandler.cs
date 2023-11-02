using System.IO;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using UnoClass.Networking;
using Terraria.GameContent.UI.States;
using UnoClass.Content.Projectiles;
using System.Collections.Generic;
using System.Linq;

namespace UnoClass.Networking {
    internal class CardProjectilePacketHandler : PacketHandler {
        public const byte SendCardSpawnTarget = 0;
        public const byte SendCardPlacedTarget = 1;

        public CardProjectilePacketHandler(byte handlerType) : base(handlerType) {

        }

        public override void HandlePacket(BinaryReader reader, int fromWho) {
            switch (reader.ReadByte()) {
                case (SendCardSpawnTarget): //card spawning
                    if (Main.netMode == NetmodeID.Server) {
                        int projectileIDPacket = reader.ReadInt32();
                        byte cardPacket = reader.ReadByte();
                        SendCardSpawn(fromWho, projectileIDPacket, cardPacket, -1);
                    } else {
                        ReceiveCardSpawn(reader, fromWho);
                    }
                    break;
                case (SendCardPlacedTarget): //card position update
                    if (Main.netMode == NetmodeID.Server) {
                        int projectileIDPacket = reader.ReadInt32();
                        float positionX = reader.ReadSingle();
                        float positionY = reader.ReadSingle();
                        SendCardPlaced(fromWho, projectileIDPacket, -1);
                    } else {
                        ReceiveCardPlaced(reader, fromWho);
                    }
                    break;
            }
        }

        public void SendCardSpawn(int fromWho, int projectileID, UnoCard card, int toWho = -1) {
            SendCardSpawn(fromWho, projectileID, card.ToPacket(), toWho);
        }
        public void SendCardSpawn(int fromWho, int projectileID, byte card, int toWho = -1) {
            ModPacket packet = GetPacket(SendCardSpawnTarget, fromWho); //this is for spawning a card projectile
            packet.Write(projectileID);
            packet.Write(card);
            Main.NewText("Sending packet card spawn of pid " + projectileID + " with card packet " + card.ToString());
            packet.Send(toWho, fromWho);
        }
        public void ReceiveCardSpawn(BinaryReader reader, int fromWho) {
            int projectileID = reader.ReadInt32();
            UnoCard card = UnoCard.FromPacket(reader.ReadByte());
            Main.NewText("Hey man i received a card or some shit from like " + fromWho + " saying it's like a uhh " + card.ToPacket().ToString() + " and shit fr and its ID is like TOTALLY " + projectileID, new Microsoft.Xna.Framework.Color(12, 182, 100));
            Projectile proj = Main.projectile.FirstOrDefault(x => x.identity == projectileID);
            Main.NewText("Hot reload: " + proj.Name);
            CardProjectile cardProj = proj.ModProjectile as CardProjectile;
            cardProj.card = card;
        }
        public void SendCardPlaced(int fromWho, int projectileID, int toWho = -1) {
            ModPacket packet = GetPacket(SendCardPlacedTarget, fromWho); //this is for updating a card projectile's position
            packet.Write(projectileID);
            Main.NewText("Sending packet card placed of pid " + projectileID);
            packet.Send(toWho, fromWho);
        }
        public void ReceiveCardPlaced(BinaryReader reader, int fromWho) {
            int projectileID = reader.ReadInt32();
            Main.NewText("Hey man i received a card position or some shit from like " + fromWho + " and shit fr and its ID is like TOTALLY " + projectileID, new Microsoft.Xna.Framework.Color(12, 182, 10));
            Projectile proj = Main.projectile[projectileID];
            CardProjectile cardProj = proj.ModProjectile as CardProjectile;
            cardProj.setPlaced();
        }
    }
}