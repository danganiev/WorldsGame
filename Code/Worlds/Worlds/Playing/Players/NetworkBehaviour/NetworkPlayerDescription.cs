using Microsoft.Xna.Framework;

using WorldsGame.Players.MovementBehaviours;

namespace WorldsGame.Playing.Players
{
    public struct NetworkPlayerDescription : IMovementProxy
    {
        public float LeftRightRotation { get; set; }

        public float UpDownRotation { get; set; }

        public Vector3 Position { get; set; }

        public bool IsMovingForward { get; set; }

        public bool IsMovingBackward { get; set; }

        public bool IsStrafingLeft { get; set; }

        public bool IsStrafingRight { get; set; }

        public bool JumpOccured { get; set; }

        public double Timestamp { get; set; }
    }

    public struct ServerNetworkPlayerDescription : IMovementProxy
    {
        public float LeftRightRotation { get; set; }

        public float UpDownRotation { get; set; }

        public float YVelocity { get; set; }

        public Vector3 Position { get; set; }

        public bool IsMovingForward { get; set; }

        public bool IsMovingBackward { get; set; }

        public bool IsStrafingLeft { get; set; }

        public bool IsStrafingRight { get; set; }

        public static ServerNetworkPlayerDescription CreateFromNetworkDescription(NetworkPlayerDescription networkPlayerDescription)
        {
            return new ServerNetworkPlayerDescription
            {
                IsMovingBackward = networkPlayerDescription.IsMovingBackward,
                IsMovingForward = networkPlayerDescription.IsMovingForward,
                IsStrafingLeft = networkPlayerDescription.IsStrafingLeft,
                IsStrafingRight = networkPlayerDescription.IsStrafingRight,

                LeftRightRotation = networkPlayerDescription.LeftRightRotation,
                UpDownRotation = networkPlayerDescription.UpDownRotation                
            };
        }
    }
}