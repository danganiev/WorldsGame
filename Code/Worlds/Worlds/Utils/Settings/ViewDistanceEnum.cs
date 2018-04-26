using System.Collections.Generic;

namespace WorldsGame.Utils
{
    internal static class ViewDistanceEnum
    {
        internal const byte Tiny = 6;
        internal const byte Short = 9;
        internal const byte Normal = 12;
        internal const byte Large = 16;

        internal static Dictionary<string, byte> SettingsNames { get; private set; }

        static ViewDistanceEnum()
        {
            SettingsNames = new Dictionary<string, byte>
                            {
                                {"Tiny", Tiny},
                                {"Short", Short},
                                {"Normal", Normal},
                                {"Large", Large}
                            };
        }
    }
}