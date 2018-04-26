using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Ionic.Zip;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using WorldsGame.Playing.DataClasses;

namespace WorldsGame.Saving
{
    public class SaverHelper<T> where T : class, ISaveDataSerializable<T>
    {
        private readonly string _containerName;
        private T _saveData;
        private readonly BinaryFormatter _serializer;

        internal string DirectoryRelativePath { get; set; }

        internal string FullContainerPath
        {
            get { return SavingUtils.GetFullContainerPath(_containerName, DirectoryRelativePath); }
        }

        public SaverHelper(string containerName)
        {
            if (containerName == string.Empty)
                throw new ArgumentException("Container name could not be empty");

            _containerName = containerName;

            _serializer = new BinaryFormatter();
        }

        private StorageContainer GetContainer()
        {
            IAsyncResult storageResult = StorageDevice.BeginShowSelector(null, null);

            storageResult.AsyncWaitHandle.WaitOne();

            StorageDevice device = StorageDevice.EndShowSelector(storageResult);

            // Open a storage container.

            string newContainerName = string.IsNullOrEmpty(DirectoryRelativePath)
                                          ? _containerName
                                          : _containerName + "/" + DirectoryRelativePath;

            IAsyncResult containerResult = device.BeginOpenContainer(newContainerName, null, null);

            // Wait for the WaitHandle to become signaled.
            containerResult.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(containerResult);

            // Close the wait handle.
            storageResult.AsyncWaitHandle.Close();
            containerResult.AsyncWaitHandle.Close();

            return container;
        }

        internal void Save(T savedata = null)
        {
            if (savedata != null)
            {
                _saveData = savedata;
            }
            using (StorageContainer container = GetContainer())
            {
                string filename = _saveData.FileName;

                DeleteBeforeSave(filename, container);

                using (Stream stream = container.CreateFile(filename))
                {
                    using (var zipStream = new ZipOutputStream(stream))
                    {
                        // Convert the object to XML data and put it in the stream.
                        zipStream.PutNextEntry(filename);
                        _serializer.Serialize(zipStream, _saveData);
                    }
                }
            }
        }

        internal void SaveAtlas(string atlasName, Texture2D texture)
        {
            using (StorageContainer container = GetContainer())
            {
                //                DeleteBeforeSave(atlasName, container);

                // Create the file.
                //                using (Stream stream = container.CreateFile(atlasName))
                //                {
                //                    texture.SaveAsPng(stream, texture.Width, texture.Height);
                //                }
                string dirPath = Path.Combine(FullContainerPath, TextureAtlas.CONTAINER_NAME);
                string fullAtlasPath = Path.Combine(dirPath, atlasName);

                if (File.Exists(fullAtlasPath))
                {
                    File.Delete(fullAtlasPath);
                }

                if (!Directory.Exists(dirPath))
                {
                    // TODO: will have problems on long paths
                    Directory.CreateDirectory(dirPath);
                }

                using (FileStream stream = File.Create(fullAtlasPath))
                {
                    texture.SaveAsPng(stream, texture.Width, texture.Height);
                }
            }
        }

        private static void DeleteBeforeSave(string fileName, StorageContainer container)
        {
            try
            {
                // Check to see whether the save exists.
                // TODO: Exception when wrong characters in file name.
                if (container.FileExists(fileName))
                {
                    // Delete it so that we can create one fresh.
                    container.DeleteFile(fileName);
                }
            }
            catch
            {
                container.Dispose();
                throw;
            }
        }

        internal bool Exists(string filename)
        {
            using (StorageContainer container = GetContainer())
            {
                return container.FileExists(filename);
            }
        }

        internal Texture2D LoadImage(string imageName, GraphicsDevice graphicsDevice)
        {
            Contract.Assert(!string.IsNullOrEmpty(imageName) & !string.IsNullOrWhiteSpace(imageName));

            using (StorageContainer container = GetContainer())
            {
                Stream stream = container.OpenFile(imageName, FileMode.Open);

                Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);

                stream.Close();

                return texture;
            }
        }

        internal Texture2D LoadAtlas(string atlasName, GraphicsDevice graphicsDevice)
        {
            //            Contract.Assert(!string.IsNullOrEmpty(imageName) & !string.IsNullOrWhiteSpace(imageName));
            //
            //            using (StorageContainer container = GetContainer())
            //            {
            //                Stream stream = container.OpenFile(imageName, FileMode.Open);
            //
            //                Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
            //
            //                stream.Close();
            //
            //                return texture;
            //            }
            string dirPath = Path.Combine(FullContainerPath, TextureAtlas.CONTAINER_NAME);
            string fullAtlasPath = Path.Combine(dirPath, atlasName);

            Texture2D texture;
            using (FileStream stream = File.OpenRead(fullAtlasPath))
            {
                texture = Texture2D.FromStream(graphicsDevice, stream);
            }

            return texture;
        }

        public T Load(string fileName, StorageContainer container_ = null, bool hasExtension = false)
        {
            Contract.Assert(!string.IsNullOrEmpty(fileName) & !string.IsNullOrWhiteSpace(fileName));

            StorageContainer container = container_ ?? GetContainer();
            try
            {
                string fileExtName = hasExtension ? fileName : FileExtensionName(fileName);
                T deserializedSave;

                using (Stream stream = container.OpenFile(fileExtName, FileMode.Open))
                {
                    using (var zipInputStream = new ZipInputStream(stream))
                    {
                        zipInputStream.GetNextEntry();
                        // Convert the object to XML data and put it in the stream.
                        deserializedSave = (T)_serializer.Deserialize(zipInputStream);
                    }
                }

                // Dispose the container, to commit changes.
                if (container_ == null)
                    container.Dispose();

                return deserializedSave;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch
            {
                container.Dispose();
                throw;
            }
        }

        private static string FileExtensionName(string fileName)
        {
            string fileExtName = fileName;

            if (fileName.Split('.').Last() != "sav")
            {
                fileExtName = string.Join(".", fileName, "sav");
            }
            return fileExtName;
        }

        public List<string> LoadNames()
        {
            string[] fileNames;
            using (StorageContainer container = GetContainer())
            {
                fileNames = container.GetFileNames();
            }

            return fileNames.ToList();
        }

        public List<T> LoadList()
        {
            using (StorageContainer container = GetContainer())
            {
                string[] fileNames = container.GetFileNames();
                var list = fileNames.Select(fileName => Load(fileName, container)).Where(arg => arg != null).ToList();
                return list;
            }
        }

        public List<T> LoadList(List<string> fileNames)
        {
            using (StorageContainer container = GetContainer())
            {
                var list = fileNames.Select(fileName => Load(fileName, container)).Where(arg => arg != null).ToList();
                return list;
            }
        }

        internal bool Clear()
        {
            using (StorageContainer container = GetContainer())
            {
                foreach (string name in container.GetFileNames())
                {
                    container.DeleteFile(name);
                }

                foreach (string name in container.GetDirectoryNames())
                {
                    container.DeleteDirectory(name);
                }
            }

            if (DirectoryRelativePath != "")
            {
                string path = FullContainerPath;
                var containerDirectoryInfo = new DirectoryInfo(path);

                if (containerDirectoryInfo.Exists)
                {
                    try
                    {
                        containerDirectoryInfo.Delete(true);
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            return true;
        }

        internal bool Delete(string fileOrDirectoryName, string extension = "sav")
        {
            using (StorageContainer container = GetContainer())
            {
                string fileExtName = fileOrDirectoryName;
                string[] fileNameList = fileOrDirectoryName.Split('.');

                if (fileNameList.Last() != extension)
                {
                    fileExtName = string.Join(".", fileOrDirectoryName, extension);
                }

                if (!container.FileExists(fileExtName))
                {
                    return false;
                }

                container.DeleteFile(fileExtName);
            }

            return true;
        }
    }
}