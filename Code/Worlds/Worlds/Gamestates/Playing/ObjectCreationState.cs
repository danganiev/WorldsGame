using System.Collections.Generic;

using Microsoft.Xna.Framework;
using WorldsGame.Models;
using WorldsGame.Playing;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Renderers;
using WorldsGame.Saving;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;

using WorldsLib;

namespace WorldsGame.Gamestates
{
    internal sealed class ObjectCreationState : PlayingState
    {
        private SelectionGrid _selectionGrid;
        private SelectionGridRenderer _selectionGridRenderer;

        internal ObjectCreationState(WorldsGame game, CompiledGameBundle compiledGameBundle, World world)
            : base(game, compiledGameBundle, world, null)
        {
        }

        protected override void InitializeMainVariables()
        {
            base.InitializeMainVariables();

            _selectionGrid = new SelectionGrid(world.CompiledGameBundle, world.ClientPlayer);

            pauseMenu.SelectionGrid = _selectionGrid;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _selectionGridRenderer.LoadContent(Content, worldsContentLoader);
        }

        protected override void CreateRenderers()
        {
            base.CreateRenderers();

            _selectionGridRenderer = new SelectionGridRenderer(Game.GraphicsDevice, _selectionGrid);
        }

        protected override void AdditionalDrawTransparent(GameTime gameTime)
        {
            base.AdditionalDrawTransparent(gameTime);

            _selectionGridRenderer.DrawTransparent(gameTime);
        }

        internal void LoadObject(string objectName)
        {
            world.ObjectCreationHelper.LoadObject(objectName, this);
        }

        internal void DeleteObject(string objectName)
        {
            world.ObjectCreationHelper.DeleteObject(objectName);
        }
    }

    internal class EmptyObjectCreationWorldHelper
    {
        internal GameObject GameObject { get; set; }

        internal HashSet<Vector3i> FilledCoords { get; private set; }

        internal EmptyObjectCreationWorldHelper()
        {
            FilledCoords = new HashSet<Vector3i>();
        }

        internal virtual void LoadObject(string objectName, ObjectCreationState state)
        {
        }

        internal virtual void DeleteObject(string objectName)
        {
        }
    }

    internal class ObjectCreationWorldHelper : EmptyObjectCreationWorldHelper
    {
        private World World { get; set; }

        internal ObjectCreationWorldHelper(World world)
        {
            World = world;
        }

        internal override void LoadObject(string objectName, ObjectCreationState state)
        {
            World.ClientPlayer.SetIntoDefaultPosition();

            GameObject gameObject = GameObject.Load(World.WorldSettingsName, objectName);
            var involvedChunks = new Dictionary<Vector3i, Chunk>();

            foreach (Vector3i filledCoord in FilledCoords)
            {
                World.SetBlock(filledCoord, BlockTypeHelper.AIR_BLOCK_TYPE, suppressEvents: true);

                Vector3i chunkIndex = Chunk.GetChunkIndex(filledCoord);
                if (!involvedChunks.ContainsKey(chunkIndex))
                {
                    Chunk chunk = World.Chunks.Get(chunkIndex);
                    involvedChunks.Add(chunkIndex, chunk);
                }
            }

            FilledCoords.Clear();

            foreach (KeyValuePair<Vector3i, string> keyValuePair in gameObject.Blocks)
            {
                World.SetBlock(keyValuePair.Key, BlockTypeHelper.Get(keyValuePair.Value));

                Vector3i chunkIndex = Chunk.GetChunkIndex(keyValuePair.Key);
                if (!involvedChunks.ContainsKey(chunkIndex))
                {
                    Chunk chunk = World.Chunks.Get(chunkIndex);
                    involvedChunks.Add(chunkIndex, chunk);
                }
            }

            foreach (KeyValuePair<Vector3i, Chunk> involvedChunk in involvedChunks)
            {
                involvedChunk.Value.RedrawWithNeighbours();
            }

            state.Resume();
        }

        internal override void DeleteObject(string objectName)
        {
            GameObject.Delete(World.WorldSettingsName, objectName);
        }
    }
}