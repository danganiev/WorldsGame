using WorldsGame.Saving;
using WorldsGame.Saving.World;

namespace WorldsGame
{
    public class ServerSettings
    {
        public string WorldTypeName { get; set; }

        public string WorldName { get; set; }

        public short MaxPlayers { get; set; }

        public bool IsNewGame { get; set; }

        public string Seed { get; set; }

        public WorldSettings WorldSettings { get; set; }

        public WorldSave WorldSave { get; set; }

        public int Port { get; set; }
    }
}