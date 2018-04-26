using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorldsGame.Utils
{
    public static class Constants
    {
        public const string TEMP_DIR_NAME = "Temp";
        public const int TEXTURE_SIZE = 32;
        public const int TEXTURE_MIPMAP_BORDER_SIZE = 1;

        public const string SINGLEPLAYER_SAVE_FOLDER_NAME = "Worlds";
        public const string SERVER_SAVE_FOLDER_NAME = "WorldsServer";
        public const int MILLISECONDS_PER_KEYFRAME = 33;

        public static readonly string SOUND_EFFECTS_FOLDER_NAME = Path.Combine("Data", "Sounds");

        public static string SaveGamesFolder { get; set; }
    }
}