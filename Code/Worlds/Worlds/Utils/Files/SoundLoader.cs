using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Utils.Files
{
    internal static class SoundLoader
    {
        internal static List<string> SoundList
        {
            get 
            {
                IEnumerable<FileInfo> soundFilesInfo = GetSoundFilesInfo();
                
                var soundList = new List<string>();

                foreach (FileInfo fileInfo in soundFilesInfo)
                {
                    soundList.Add(Path.GetFileNameWithoutExtension(fileInfo.Name));
                }

                return soundList;
            }
        }

        private static IEnumerable<FileInfo> GetSoundFilesInfo()
        {
            var soundsInfo = new DirectoryInfo(SoundsDir);
            IEnumerable<FileInfo> soundsFiles = soundsInfo.EnumerateFiles(EffectSound.Extension);

            return soundsFiles;
        }

        private static string SoundsDir
        {
            get
            {
                string applicationDir = Directory.GetCurrentDirectory();
                return Path.Combine(applicationDir, Constants.SOUND_EFFECTS_FOLDER_NAME);
            }
        }

        internal static void CopySounds(CompiledGameBundle gameBundle)
        {
            string savingPath = SavingUtils.GetFullContainerPath(CompiledGameBundleSave.StaticContainerName, gameBundle.FullName);
            string soundDirPath = Path.Combine(savingPath, EffectSound.StaticContainerName);

            if (!Directory.Exists(SoundsDir))
            {
                return;
            }

            var soundsInfo = new DirectoryInfo(SoundsDir);
            IEnumerable<FileInfo> soundsFiles = soundsInfo.EnumerateFiles(EffectSound.Extension);

            if (!Directory.Exists(soundDirPath))
            {
                Directory.CreateDirectory(soundDirPath);
            }

            foreach (FileInfo sound in soundsFiles)
            {
                try
                {
                    sound.CopyTo(Path.Combine(soundDirPath, sound.Name));
                }
                catch (IOException)
                {
                }
            }
        }
    }
}