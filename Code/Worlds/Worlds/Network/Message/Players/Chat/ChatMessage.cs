using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using WorldsGame.Network.Manager;

namespace WorldsGame.Network.Message.Players
{
    internal class ChatMessage : IGameMessage, IClientSendableMessage, ITimeStampedMessage, IServerSendableMessage
    {
        public string Text { get; set; }

        public byte PlayerSlot { get; set; }

        public double Timestamp { get; set; }

        public GameMessageType MessageType
        {
            get { return GameMessageType.ChatMessage; }
        }

        public ChatMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public ChatMessage()
        {
        }

        public void Decode(NetIncomingMessage im)
        {
            Text = im.ReadString();
            PlayerSlot = im.ReadByte();
            Timestamp = im.ReadDouble();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Text);

            om.Write(PlayerSlot);

            Timestamp = NetTime.Now;

            om.Write(Timestamp);
        }

        public void Send(ClientNetworkManager networkManager)
        {
            networkManager.SendMessage(this);
        }

        public void Send(NetConnection connection)
        {
        }
    }
}