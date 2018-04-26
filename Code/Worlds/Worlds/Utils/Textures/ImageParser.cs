using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Saving;
using Texture = WorldsGame.Saving.Texture;

namespace WorldsGame.Utils.Textures
{
    internal class ImageParser
    {
        private readonly WorldSettings _worldSettings;
        private readonly GraphicsDevice _graphicsDevice;

        internal ImageParser(GraphicsDevice graphicsDevice, WorldSettings worldSettings)
        {
            _worldSettings = worldSettings;
            _graphicsDevice = graphicsDevice;
        }

        internal void Parse()
        {
            SaverHelper<Texture> saverHelper = Texture.SaverHelper(_worldSettings.Name);

            var names = saverHelper.LoadNames();
            var textureNames =
                names.Where(s => new[] {"sav"}.Contains(s.Split('.').Last())).Select(s =>
                    {
                        var splat = s.Split('.').ToList();
                        splat.RemoveAt(splat.Count - 1);

                        return string.Join(".", splat); 
                    }).ToList();

            var imageNames =
                from s in names
                where new[] {"jpg", "png"}.Contains(s.Split('.').Last())
                select s;

            foreach (string name in imageNames)
            {
                List<string> splitNameList = name.Split('.').ToList();
//                string extension = splitNameList.Last();
                splitNameList.RemoveAt(splitNameList.Count - 1);

                string noExtensionName = string.Join(".", splitNameList);

                if (!textureNames.Contains(noExtensionName))
                {
                    Texture2D texture = saverHelper.LoadImage(name, _graphicsDevice);

                    if (texture.Width < 32 || texture.Height < 32)
                    {
                        continue;
                    }

                    var colors = new Color[32*32];
                    texture.GetData(0, new Rectangle(0,0,32,32), colors, 0, colors.Length);

                    var worldsTexture = new Texture
                    {
                        Name = noExtensionName,
                        WorldSettingsName = _worldSettings.Name,
                        Colors = colors
                    };

                    texture.Dispose();
//                    saverHelper.Delete(name, extension);

                    worldsTexture.Save();
                }
            }            
        }
    }
}
