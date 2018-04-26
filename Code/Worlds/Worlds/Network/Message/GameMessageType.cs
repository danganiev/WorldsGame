namespace WorldsGame.Network.Message
{
    /// <summary>
    /// The game message types.
    /// </summary>
    public enum GameMessageType
    {
        ServerAuthorizationRequest,
        StandartHailMessage,

        // File downloading
        GameBundleRequest,

        AtlasesRequest,
        AtlasCount,
        FileStreamStart,
        FileStreamData,

        // Chunks
        ChunkRequest,

        ChunkResponse,

        // Blocks
        BlockUpdate,

        // Players
        PlayerInitializationRequest,

        PlayerInitialization,
        TestPlayerDelta,
        PlayerDelta,

        PlayerBlockTypeChange,
        PlayerMovementBehaviourChange,

        PlayerDisconnect,

        // these two are for one message class
        ServerPlayerDelta,

        ServerOtherPlayerDelta,

        PlayerSingleAction,

        //Chat
        ChatMessage
    }
}