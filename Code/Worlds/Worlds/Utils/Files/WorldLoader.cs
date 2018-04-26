using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Ionic.Zip;
using WorldsGame.Saving;

namespace WorldsGame.Utils
{
    internal static class WorldLoader
    {
        private static string LocalWorldsDir
        {
            get
            {
                string applicationDir = Directory.GetCurrentDirectory();
                return Path.Combine(applicationDir, "Data", "LocalWorlds");
            }
        }

        internal static void LoadLocalWorlds()
        {
            string localWorldsDir = LocalWorldsDir;

            if (!Directory.Exists(localWorldsDir))
            {
                Directory.CreateDirectory(localWorldsDir);
                return;
            }

            var localWorldsInfo = new DirectoryInfo(localWorldsDir);
            IEnumerable<FileInfo> localWorldsFiles = localWorldsInfo.EnumerateFiles("*.zip");

            string loadedPath = Path.Combine(localWorldsDir, "Loaded");
            string extractedPath = Path.Combine(localWorldsDir, "Extracted");

            string savedGamesPath = SavingUtils.FullSavingPath;

            foreach (FileInfo localWorldsFile in localWorldsFiles)
            {
                try
                {
                    using (ZipFile zf = ZipFile.Read(localWorldsFile.FullName))
                    {
                        zf.ExtractAll(extractedPath);
                        string extractedWorldsPath = Path.Combine(extractedPath, "Worlds");
                        if (Directory.Exists(extractedWorldsPath))
                        {
                            MoveDirectory(extractedPath, savedGamesPath);
                        }
                    }
                }
                catch (ZipException)
                {
                }

                if (!Directory.Exists(loadedPath))
                {
                    Directory.CreateDirectory(loadedPath);
                }

                try
                {
                    localWorldsFile.MoveTo(Path.Combine(loadedPath, localWorldsFile.Name));
                }
                catch (IOException)
                {
                }
            }
        }

        internal static void LoadFromURL(string url)
        {
            string localWorldsDir = LocalWorldsDir;

            if (!Directory.Exists(localWorldsDir))
            {
                Directory.CreateDirectory(localWorldsDir);
            }

            string filename = url.Split('/').LastOrDefault();

            if (filename.Length == 0)
            {
                filename = "wrongFileName.zip";
            }

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, Path.Combine(localWorldsDir, filename));
                }
                catch (WebException)
                {
                }
                catch (ArgumentException)
                {
                }
            }
            LoadLocalWorlds();
        }

        internal static void MoveDirectory(string source, string target)
        {
            var stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));

            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }

                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }
            Directory.Delete(source, true);
        }

        internal class Folders
        {
            public string Source { get; private set; }

            public string Target { get; private set; }

            public Folders(string source, string target)
            {
                Source = source;
                Target = target;
            }
        }
    }
}