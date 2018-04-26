using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Editors.Blocks;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.GeometricPrimitives;

namespace WorldsGame.Saving.DataClasses
{
    [Serializable]
    public class Character : ISaveDataSerializable<Character>
    {
        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Characters"; } }

        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        public Vector3 MinVertice { get; set; }

        public Vector3 MaxVertice { get; set; }

        // This is for recreating the character in model editor
        public List<Cuboid> Cuboids { get; set; }

        public Dictionary<AnimationType, CompiledAnimation> Animations { get; set; }

        public List<SpawnedItemRule> InventorySpawnRules { get; set; }

        public ItemCuboidData DefaultItemData { get; set; }

        public Dictionary<string, ItemCuboidData> OverriddenItemsData { get; set; }

        // Only for model editor.

        // X
        public int LengthInBlocks { get; set; }

        // Y
        public int HeightInBlocks { get; set; }

        // Z
        public int WidthInBlocks { get; set; }

        // This MUST be parallel to x/z plane
        //        public Vector3 FaceNormal { get; set; }

        public float FaceHeight { get; set; }

        public bool IsPlayerCharacter { get; set; }

        // There is no constructor and that is by design

        internal static SaverHelper<Character> SaverHelper(string name)
        {
            return new SaverHelper<Character>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<Character> SaverHelper()
        {
            return SaverHelper(WorldSettingsName);
        }

        public void Save()
        {
            SaveDefaultItemCuboid();
            SaverHelper().Save(this);
        }

        private void SaveDefaultItemCuboid()
        {
            try
            {
                //                Cuboid itemCuboid = (from cuboid in Cuboids
                //                                     where cuboid.IsItem
                //                                     select cuboid).First();

                //                Cuboid itemCuboid = null;
                //                foreach (Cuboid cuboid in Cuboids)
                //                {
                //                    if (cuboid.IsItem)
                //                    {
                //                        itemCuboid = cuboid;
                //                        break;
                //                    }
                //                }

                Cuboid itemCuboid = Cuboids.FirstOrDefault(cuboid => cuboid.IsItem);

                DefaultItemData = new ItemCuboidData
                                      {
                                          Cuboid = itemCuboid,
                                          //                StickedCuboidID =
                                      };
            }
            catch (InvalidOperationException)
            {
#if DEBUG
                throw;
#endif
            }
        }

        public void Delete()
        {
            SaverHelper().Delete(Name);
        }

        public static void Delete(string worldSettingsName, string name)
        {
            SaverHelper(worldSettingsName).Delete(name);
        }

        public bool ContainsTexture(string name)
        {
            foreach (Cuboid cuboid in Cuboids)
            {
                bool result = cuboid.ContainsTexture(name);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<string> GetTextureNames()
        {
            var textureNames = new List<string>();
            foreach (Cuboid cuboid in Cuboids)
            {
                textureNames.AddRange(cuboid.GetTextureNames());
            }

            return textureNames.Distinct();
        }

        // This could be done via text or xml file, but whatever
        public static void CreateDefaultPlayer(string worldSettingsName, ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            CreateDefaultPlayerTextures(worldSettingsName, contentManager);

            var character = new Character
            {
                Name = "Player",
                IsPlayerCharacter = true,
                WorldSettingsName = worldSettingsName,
                Cuboids = GetDefaultPlayerCuboids(graphicsDevice),
                DefaultItemData = new ItemCuboidData(),
                InventorySpawnRules = new List<SpawnedItemRule>(),
                //                FaceNormal = Vector3.Forward,
                FaceHeight = 1.7f * Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER,
                OverriddenItemsData = new Dictionary<string, ItemCuboidData>()
            };

            character.PrepareAnimations(isNew: true);

            var minVertice = Vector3.Zero;
            var maxVertice = Vector3.Zero;

            foreach (Cuboid cuboid in character.Cuboids)
            {
                foreach (Plane plane in cuboid.Planes)
                {
                    foreach (var vertice in plane.Vertices)
                    {
                        minVertice = Vector3.Min(minVertice, vertice);
                        maxVertice = Vector3.Max(maxVertice, vertice);
                    }
                }
            }

            character.MaxVertice = maxVertice;
            character.MinVertice = minVertice;

            character.Save();
        }

        private static void CreateDefaultPlayerTextures(string worldSettingsName, ContentManager contentManager)
        {
            using (var characterSkin = contentManager.Load<Texture2D>("Models//DefaultCharacter"))
            {
                var face = new Color[64];

                characterSkin.GetData(0, new Rectangle(0, 0, 8, 8), face, 0, face.Length);

                var faceTexture = new Texture
                                      {
                                          Name = "PlayerFace",
                                          WorldSettingsName = worldSettingsName,
                                          Colors = face,
                                          Width = 8,
                                          Height = 8
                                      };

                faceTexture.Save();
            }
        }

        private static List<Cuboid> GetDefaultPlayerCuboids(GraphicsDevice graphicsDevice)
        {
            var result = new List<Cuboid>();
            var face = new CuboidPrimitive(graphicsDevice, new Vector3(20, 58, 8), new Vector3(0, -6, 0));
            var facePlanes = face.GetUnmodifiedPlanes();

            foreach (Plane facePlane in facePlanes)
            {
                facePlane.TextureName = "PlayerFace";
            }

            var faceCuboid = new Cuboid(
                facePlanes, face.Position,
                face.Yaw, face.Pitch, face.Roll);

            result.Add(faceCuboid);

            return result;
        }

        public void PrepareAnimations(EditedModel characterModel = null, bool isNew = false)
        {
            if (Animations == null)
            {
                Animations = new Dictionary<AnimationType, CompiledAnimation>();

                foreach (AnimationType at in EnumUtils.GetValues<AnimationType>())
                {
                    Animations.Add(at, new CompiledAnimation(Cuboids.Count));
                }
            }
            if (!isNew)
            {
                RefreshAnimationsPerCuboids(characterModel);
            }
        }

        // Updates data on animations if cuboids were added/removed on model
        private void RefreshAnimationsPerCuboids(EditedModel characterModel = null)
        {
            if (characterModel != null)
            {
                foreach (KeyValuePair<AnimationType, CompiledAnimation> compiledAnimation in Animations)
                {
                    compiledAnimation.Value.RefreshAnimationPerCuboids(characterModel);
                }
            }
        }
    }
}