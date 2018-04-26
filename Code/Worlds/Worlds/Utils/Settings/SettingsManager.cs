using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using WorldsGame.Utils.Settings;

namespace WorldsGame.Utils
{
    public static class SettingsManager
    {
        private const string APP_SETTINGS_FILE_NAME = "settings.xml";
        private const string CONTROLS_FILE_NAME = "player_controls.xml";

        public static AppSettings Settings;
        public static ControlSettings ControlSettings;

        static SettingsManager()
        {
            LoadSettings();
            LoadControlSettings();
        }

        public static void LoadSettings()
        {
            // Create our exposed settings class. This class gets serialized to load/save the settings.
            Settings = new AppSettings();
            //Obtain a virtual store for application
#if WINDOWS
            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            // Check if file is there
            if (fileStorage.FileExists(APP_SETTINGS_FILE_NAME))
            {
                var serializer = new XmlSerializer(Settings.GetType());
                using (var stream = new StreamReader(new IsolatedStorageFileStream(APP_SETTINGS_FILE_NAME, FileMode.Open, fileStorage)))
                {
                    try
                    {
                        Settings = (AppSettings)serializer.Deserialize(stream);

                        //                        stream.Close();
                    }
                    catch
                    {
                        // An error occurred so let's use the default settings.
                        //                        stream.Close();
                        Settings = new AppSettings();
                        // Saving is optional - in this sample we assume it works and the error is due to the file not being there.
                        SaveSettings();
                        // Handle other errors here
                    }
                }
            }
            else
            {
                SaveSettings();
            }
        }

        public static void LoadControlSettings()
        {
            ControlSettings = new ControlSettings();
#if WINDOWS
            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            if (fileStorage.FileExists(CONTROLS_FILE_NAME))
            {
                var serializer = new XmlSerializer(ControlSettings.GetType());
                using (var stream = new StreamReader(new IsolatedStorageFileStream(CONTROLS_FILE_NAME, FileMode.Open, fileStorage)))
                {
                    try
                    {
                        ControlSettings = (ControlSettings)serializer.Deserialize(stream);

                        //                        stream.Close();
                    }
                    catch
                    {
                        // An error occurred so let's use the default settings.
                        //                        stream.Close();
                        ControlSettings = new ControlSettings();
                        // Saving is optional - in this sample we assume it works and the error is due to the file not being there.
                        SaveControlSettings();
                        // Handle other errors here
                    }
                }
            }
            else
            {
                SaveControlSettings();
            }
        }

        public static void SaveSettings()
        {
            //Obtain a virtual store for application
#if WINDOWS
            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            var serializer = new XmlSerializer(Settings.GetType());
            using (var stream = new StreamWriter(new IsolatedStorageFileStream(APP_SETTINGS_FILE_NAME, FileMode.Create, fileStorage)))
            {
                try
                {
                    serializer.Serialize(stream, Settings);
                }
                catch
                {
                    // Handle your errors here
                }
            }
        }

        public static void SaveControlSettings()
        {
            //Obtain a virtual store for application
#if WINDOWS
            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            var serializer = new XmlSerializer(ControlSettings.GetType());
            using (var stream = new StreamWriter(new IsolatedStorageFileStream(CONTROLS_FILE_NAME, FileMode.Create, fileStorage)))
            {
                try
                {
                    serializer.Serialize(stream, ControlSettings);
                }
                catch
                {
                    // Handle your errors here
                }
            }
        }
    }

    // Internal game settings that are not saved and reload with every new Worlds.exe execution
    internal static class InternalSystemSettings
    {
        internal static bool IsServer { get; set; }

        static InternalSystemSettings()
        {
            IsServer = false;
        }
    }
}