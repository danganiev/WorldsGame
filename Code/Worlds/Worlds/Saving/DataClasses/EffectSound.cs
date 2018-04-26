using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Sound;

namespace WorldsGame.Saving.DataClasses
{
    // Such is life of name because SoundEffect is already part of XNA
    internal static class EffectSound
    {
        internal const string StaticContainerName = "SoundEffects";
        internal const string Extension = "*.wav";

        internal static void GetWorldSounds(CompiledGameBundle bundle, AudioManager audioManager)
        {
            //            string soundsPath = Path.Combine(
            //                SavingUtils.GetContainerSavingPath(StaticContainerName), worldName);

            string savingPath = SavingUtils.GetFullContainerPath(CompiledGameBundleSave.StaticContainerName, bundle.FullName);
            string soundDirPath = Path.Combine(savingPath, StaticContainerName);

            var soundsDirInfo = new DirectoryInfo(soundDirPath);
            IEnumerable<FileInfo> soundsFiles = soundsDirInfo.EnumerateFiles(Extension);

            foreach (FileInfo soundsFile in soundsFiles)
            {
                audioManager.LoadSound(Path.GetFileNameWithoutExtension(soundsFile.Name), soundsFile.FullName);
            }
        }
    }
}