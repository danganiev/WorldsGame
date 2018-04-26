using System;
using System.Collections.Generic;

using Lidgren.Network;

using WorldsGame.Network.Manager;
using WorldsGame.Playing.Players;

namespace WorldsGame.Network.Message
{
    // Player data from the client
    public class PlayerDeltaMessage : IGameMessage, IClientSendableMessage, ITimeStampedMessage
    {
        // Lidgren already does bit magic for us with bools, https://stackoverflow.com/questions/4854207/get-a-specific-bit-from-byte

        public int PlayerDescriptionCount { get; set; }

        public int Index { get; set; }

        public double Timestamp { get; set; }

        // Used only on client
        public double PreviousTimestamp { get; set; }

        public List<NetworkPlayerDescription> CollectedMovementChanges { get; set; }

        // NOTE: look fields could and will be hacked too, if the game becomes popular, so I'm saving the task for later
        public PlayerDeltaMessage(List<NetworkPlayerDescription> collectedMovementChanges, double timestamp)
        {
            CollectedMovementChanges = collectedMovementChanges;

            Timestamp = timestamp;
        }

        public PlayerDeltaMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.PlayerDelta; }
        }

        public void Decode(NetIncomingMessage im)
        {
            PlayerDescriptionCount = im.ReadInt32();
            CollectedMovementChanges = new List<NetworkPlayerDescription>(PlayerDescriptionCount);
            for (int i = 0; i < PlayerDescriptionCount; i++)
            {
                var networkPlayerDescription = new NetworkPlayerDescription
                {
                    IsMovingForward = im.ReadBoolean(),
                    IsMovingBackward = im.ReadBoolean(),
                    IsStrafingLeft = im.ReadBoolean(),
                    IsStrafingRight = im.ReadBoolean(),
                    JumpOccured = im.ReadBoolean(),

                    LeftRightRotation = im.ReadFloat(),
                    UpDownRotation = im.ReadFloat(),
                    Timestamp = im.ReadDouble()
                };
                CollectedMovementChanges.Add(networkPlayerDescription);
            }

            Index = im.ReadInt32();

            Timestamp = im.ReadTime(false);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(CollectedMovementChanges.Count);

            foreach (NetworkPlayerDescription networkPlayerDescription in CollectedMovementChanges)
            {
                om.Write(networkPlayerDescription.IsMovingForward);
                om.Write(networkPlayerDescription.IsMovingBackward);
                om.Write(networkPlayerDescription.IsStrafingLeft);
                om.Write(networkPlayerDescription.IsStrafingRight);
                om.Write(networkPlayerDescription.JumpOccured);

                om.Write(networkPlayerDescription.LeftRightRotation);
                om.Write(networkPlayerDescription.UpDownRotation);

                om.Write(networkPlayerDescription.Timestamp);
            }

            om.Write(Index);

            om.WriteTime(Timestamp, false);
        }

        public void Send(ClientNetworkManager networkManager)
        {
            networkManager.SendMessage(this, deliveryMethod: NetDeliveryMethod.UnreliableSequenced);
        }
    }
}