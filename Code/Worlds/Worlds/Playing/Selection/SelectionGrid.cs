using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Players;
using WorldsGame.Playing.VertexTypes;
using WorldsGame.Saving;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;

using WorldsLib;

namespace WorldsGame.Playing
{

//    internal enum SelectionStage
//    {
//        Hidden,
//        StartIndexSelected,
//        Selected
//    }

    internal class SelectionGrid : IDisposable
    {
        internal bool AreVerticesChanged { get; set; }
//        internal SelectionStage SelectionStage { get; private set; }
        internal Dictionary<Vector3i, string> SelectedBlocks { get; private set; }
        internal List<VertexPositionTextureLight> Vertices { get; set; }
        /// <summary>
        /// Lowest in every XYZ parallelepiped coordinates point
        /// </summary>
        private Vector3i StartIndex { get; set; }
        /// <summary>
        /// Highest in every XYZ parallelepiped coordinates point
        /// </summary>
        private Vector3i EndIndex { get; set; }

        private readonly CompiledGameBundle _compiledGameBundle;
        private readonly Player _player;

        private readonly float _oneOverAtlasSize;
        private readonly CompiledTexture _gridSelectionTexture;

        private HashSet<Vector3i> FilledCoords { get { return _player.World.FilledCoords; } } 

        internal GameObject StartGameObject { get; private set; }

//        internal event EventHandler<EventArgs> Change = (sender, args) => { };

        internal SelectionGrid(CompiledGameBundle compiledGameBundle, Player player)
        {
            _compiledGameBundle = compiledGameBundle;
            _player = player;

            Vertices = new List<VertexPositionTextureLight>();

//            Change = OnChange;

//            _gridSelectionTexture = _compiledGameBundle.GetTexture("System__SelectionGridTexture");
//            _oneOverAtlasSize = _gridSelectionTexture.OneOverAtlasSize();

//            SelectionStage = SelectionStage.Hidden;

            SelectedBlocks = new Dictionary<Vector3i, string>();

            StartGameObject = null;

            Messenger.On<Vector3i, BlockType>("WorldBlockChange", OnWorldBlockChange);
//            Messenger.On("ShiftMouseRightButtonClick", HideSelectionGrid);
        }

        internal void OnWorldBlockChange(Vector3i position, BlockType blockType)
        {
            if (blockType.IsAirType())
            {
                FilledCoords.Remove(position);
            }
            else
            {
                FilledCoords.Add(position);
            }

            if (FilledCoords.Count == 0)
            {
                StartIndex = new Vector3i(0, 0, 0);
                EndIndex = new Vector3i(0, 0, 0);
            }
            else
            {
                int minX = FilledCoords.Min(p => p.X);
                int maxX = FilledCoords.Max(p => p.X) + 1;

                int minY = FilledCoords.Min(p => p.Y);
                int maxY = FilledCoords.Max(p => p.Y) + 1;

                int minZ = FilledCoords.Min(p => p.Z);
                int maxZ = FilledCoords.Max(p => p.Z) + 1;

                StartIndex = new Vector3i(minX, minY, minZ);
                EndIndex = new Vector3i(maxX, maxY, maxZ);
            }
            

//            FillGridVertices();
        }

        public void Dispose()
        {
//            Messenger.Off("ShiftMouseLeftButtonClick", ChangeSelection);
//            Messenger.Off("ShiftMouseRightButtonClick", HideSelectionGrid);
//            Messenger.Off("PlayerSelectionChange", ChangeSelectionOnSelectionBlockChange);
        }

//        private void HideSelectionGrid()
//        {
////            SelectionStage = SelectionStage.Hidden;
//
//            _startIndex = default(Vector3i);
//            _endIndex = default(Vector3i);
//
//            Messenger.Off("PlayerSelectionChange", ChangeSelectionOnSelectionBlockChange);
//        }

//        private void OnChange(object sender, EventArgs eventArgs)
//        {
//            FillGridVertices();
//        }

//        private void ChangeSelectionOnSelectionBlockChange()
//        {
//            EndIndex = _player.CurrentSelection.Position;
//        }

//        private void ChangeSelection()
//        {
//            switch (SelectionStage)
//            {
//                case SelectionStage.Hidden:
//                case SelectionStage.Selected:
//                    // Just 1,1,0 won't work
//                    StartIndex = _player.CurrentSelection.Position + new Vector3i(1,1,0);
//                    EndIndex = StartIndex;                    
//
//                    SelectionStage = SelectionStage.StartIndexSelected;
//                    Messenger.On("PlayerSelectionChange", ChangeSelectionOnSelectionBlockChange);
//
//                    break;
//                case SelectionStage.StartIndexSelected:
//                    EndIndex = _player.CurrentSelection.Position;
//
//                    Messenger.Off("PlayerSelectionChange", ChangeSelectionOnSelectionBlockChange);
//
//                    if (StartIndex.X == EndIndex.X || StartIndex.Y == EndIndex.Y || StartIndex.Z == EndIndex.Z)
//                    {
//                        HideSelectionGrid();
//                        break;
//                    }
//
//                    // Making indices truly start and end
//                    int minX = Math.Min(StartIndex.X, EndIndex.X);
//                    int maxX = Math.Max(StartIndex.X, EndIndex.X);
//                    
//                    int minY = Math.Min(StartIndex.Y, EndIndex.Y);
//                    int maxY = Math.Max(StartIndex.Y, EndIndex.Y);
//                    
//                    int minZ = Math.Min(StartIndex.Z, EndIndex.Z);
//                    int maxZ = Math.Max(StartIndex.Z, EndIndex.Z);
//
//                    _startIndex = new Vector3i(minX, minY, minZ);
//                    _endIndex = new Vector3i(maxX, maxY, maxZ);
//                    FillGridVertices();
//
//                    SelectionStage = SelectionStage.Selected;
//
//                    break;
//            }
//            
//        }

//        private void FillGridVertices()
//        {
//            // Should pull debug/release trick here
//            float xOfs, yOfs;
//
//            Vertices.Clear();
//
//            _gridSelectionTexture.GetTextureUVCoordinates(out xOfs, out yOfs);
//
//            int minX = Math.Min(StartIndex.X, EndIndex.X);
//            int maxX = Math.Max(StartIndex.X, EndIndex.X);
//
//            int minY = Math.Min(StartIndex.Y, EndIndex.Y);
//            int maxY = Math.Max(StartIndex.Y, EndIndex.Y);
//
//            int minZ = Math.Min(StartIndex.Z, EndIndex.Z);
//            int maxZ = Math.Max(StartIndex.Z, EndIndex.Z);
//
//            Color color = Color.Black;
//
//            //X sides
//            for (int i = minX; i < maxX; i++)
//            {
//                for (int j = minY; j < maxY; j++)
//                {
//                    foreach (int k in new[] {StartIndex.Z, EndIndex.Z})
//                    {
////                        new VertexPositionTextureLight(new Short4(position.X, position.Y, position.Z, 1),
////                new NormalizedShort2(textureCoordinate), Color.Black)
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                    }
//                }
//            }
//
//            //Z sides
//            for (int k = minZ; k < maxZ; k++)
//            {
//                for (int j = minY; j < maxY; j++)
//                {
//                    foreach (int i in new[] {StartIndex.X, EndIndex.X})
//                    {
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k + 1, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k + 1, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j + 1, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                    }
//                }
//            }
//
//            //Y-sides
//            for (float k = minZ; k < maxZ; k++)
//            {
//                for (float i = minX; i < maxX; i++)
//                {
//                    foreach (float j in new[] {StartIndex.Y, EndIndex.Y})
//                    {
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k, 1), new NormalizedShort2(new Vector2(xOfs, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i, j, k + 1, 1), new NormalizedShort2(new Vector2(xOfs, yOfs)), color));
//                        Vertices.Add(new VertexPositionTextureLight(
//                            new Short4(i + 1, j, k, 1), new NormalizedShort2(new Vector2(xOfs + _oneOverAtlasSize, yOfs + _oneOverAtlasSize)), color));
//                    }
//                }
//            }
//
//            AreVerticesChanged = true;
//        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void SaveSelectedObject(string name)
        {
            // Doesn't really belong here, but this is the best place for now
            Debug.Assert(name != "");

            ComputeSelectedVolume();

            StartGameObject = new GameObject
            {
                WorldSettingsName = _compiledGameBundle.WorldSettingsName,
                Name = name,
                Blocks = SelectedBlocks
            };

            StartGameObject.Save();
        }

        private void ComputeSelectedVolume()
        {
            SelectedBlocks.Clear();

            for (var i = StartIndex.X; i < EndIndex.X; i++)
            {
                for (var j = StartIndex.Y; j < EndIndex.Y; j++)
                {
                    for (var k = StartIndex.Z; k < EndIndex.Z; k++)
                    {
                        BlockType block = _player.World.GetBlock(i, j, k);

                        // Eliminating every air block
                        if (block != BlockTypeHelper.AIR_BLOCK_TYPE)
                        {
                            // Making position relative to Zero taking StartIndex as the Zero point.
                            SelectedBlocks.Add(
                                new Vector3i((i - StartIndex.X), (j - StartIndex.Y), (k - StartIndex.Z)),
                                block.Name);
                        }
                    }
                }
            }
        }
    }
}