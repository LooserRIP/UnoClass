using System.IO;

namespace UnoClass.Networking {
    internal class ModNetHandler {
        // When a lot of handlers are added, it might be wise to automate
        // creation of them
        public const byte CardProjectileType = 0;
        internal static CardProjectilePacketHandler cardProjectile = new CardProjectilePacketHandler(CardProjectileType);
        public static void HandlePacket(BinaryReader r, int fromWho) {
            switch (r.ReadByte()) {
                case CardProjectileType:
                    cardProjectile.HandlePacket(r, fromWho);
                    break;
            }
        }
    }
}