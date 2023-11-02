using System.IO;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace UnoClass.Networking {
    internal abstract class PacketHandler {
        internal byte HandlerType { get; set; }

        public abstract void HandlePacket(BinaryReader reader, int fromWho);

        protected PacketHandler(byte handlerType) {
            HandlerType = handlerType;
        }

        protected ModPacket GetPacket(byte packetType, int fromWho) {
            ModPacket p = UnoClass.Instance.GetPacket();
            p.Write(HandlerType);
            p.Write(packetType);
            if (Main.netMode == NetmodeID.Server) {
                //p.Write((byte)fromWho);
            }
            return p;
        }
    }
}