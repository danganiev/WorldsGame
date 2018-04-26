namespace WorldsGame.Players.MovementBehaviours
{
    internal interface IMovementProxy
    {
        bool IsMovingForward { get; set; }

        bool IsMovingBackward { get; set; }

        bool IsStrafingLeft { get; set; }

        bool IsStrafingRight { get; set; }
    }
}