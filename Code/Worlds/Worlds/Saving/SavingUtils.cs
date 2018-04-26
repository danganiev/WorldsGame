using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WorldsGame.Utils;

namespace WorldsGame.Saving
{
    public static class SavingUtils
    {
        public static string GetFullContainerPath(string containerName, string directoryRelativePath)
        {
            return Path.Combine(GetContainerSavingPath(containerName), directoryRelativePath);
        }

        public static string GetContainerSavingPath(string containerName)
        {
            return Path.Combine(FullSavingPath, Constants.SaveGamesFolder, containerName);
        }

        public static string FullSavingPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SavedGames"); } }
    }
}