namespace WorldsGame.Utils
{
    public class AppSettings
    {
        public bool AreDefaultWorldsLoaded { get; set; }

        public bool IsFullScreen { get; set; }

        public bool EnableMusic { get; set; }

        public bool EnableSfx { get; set; }

        public bool DiagnosticMode { get; set; }

        public string ViewDistance { get; set; }

        public int ResolutionWidth { get; set; }

        public int ResolutionHeight { get; set; }

        public int PreviousResolutionWidth { get; set; }

        public int PreviousResolutionHeight { get; set; }

        public bool IsResolutionChanged { get; set; }

        public AppSettings()
        {
            // Create our default settings
            AreDefaultWorldsLoaded = false;
            EnableMusic = true;
            EnableSfx = true;
            ResolutionWidth = 1024;
            ResolutionHeight = 768;
            PreviousResolutionWidth = 1024;
            PreviousResolutionHeight = 768;
            IsResolutionChanged = false;

            // Is the game in diagnostic mode?
            DiagnosticMode = false;

            // View distance is actually a visible chunk radius
            ViewDistance = "Short";

            // Since this is cross platform, you can decide what default values to use for a platform.
            // In the case of full screen, phones and XBoxes are always full screen.
            // In the phone an XBox applications, we don't let the user change this setting.
#if WINDOWS
            IsFullScreen = false;
#else
            IsFullScreen = true;
#endif
        }
    }
}