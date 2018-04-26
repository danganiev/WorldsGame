#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

#endregion License

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Models;
using WorldsGame.Playing.Renderers.ContentLoaders;
using WorldsGame.Terrain;
using WorldsGame.Utils.Profiling;

namespace WorldsGame.Renderers
{
    internal class DiagnosticWorldRenderer : IRenderer
    {
        private const bool DEBUG_RECTANGLE = true;

        private BasicEffect _effect;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly World _world;

        private SpriteBatch _debugSpriteBatch;
        private SpriteFont _debugFont;
        private Texture2D _debugRectTexture;
        private Rectangle _backgroundRectangle;

        private Vector2 _chunksVector2;
        private Vector2 _readyVector2;

        internal DiagnosticWorldRenderer(GraphicsDevice graphicsDevice, World world)
        {
            _graphicsDevice = graphicsDevice;
            _world = world;
        }

        public void Initialize()
        {
            _effect = new BasicEffect(_graphicsDevice);

            _debugRectTexture = new Texture2D(_graphicsDevice, 1, 1);
            var texcol = new Color[1];
            _debugRectTexture.GetData(texcol);
            texcol[0] = Color.Black;
            _debugRectTexture.SetData(texcol);

            _backgroundRectangle = new Rectangle(680, 0, 120, 144);

            _chunksVector2 = new Vector2(680, 0);
            _readyVector2 = new Vector2(680, 128);
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
            _debugSpriteBatch = new SpriteBatch(_graphicsDevice);
            _debugFont = content.Load<SpriteFont>("Fonts\\OSDdisplay");
        }

        public void Draw(GameTime gameTime)
        {
            int totalChunksCounter = 0;
            int readyCounter = 0;

            foreach (Chunk chunk in _world.Chunks.Values)
            {
                switch (chunk.State)
                {
                    case ChunkState.Ready:
                        readyCounter++;
                        break;

                    default:
                        Debug.WriteLine("Unchecked State: {0}", chunk.State);
                        Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _world.ClientPlayer.CameraView,
                            _world.ClientPlayer.CameraProjection, Color.Blue);
                        break;
                }

                totalChunksCounter++;
            }

            //OSD debug texts
            _debugSpriteBatch.Begin();
            if (DEBUG_RECTANGLE)
            {
                _debugSpriteBatch.Draw(_debugRectTexture, _backgroundRectangle, Color.Black);
            }

            _debugSpriteBatch.DrawString(_debugFont, "Chunks: " + totalChunksCounter.ToString(), _chunksVector2, Color.White);
            _debugSpriteBatch.DrawString(_debugFont, "Ready: " + readyCounter.ToString(), _readyVector2, Color.White);
            _debugSpriteBatch.End();
        }

        public void DrawTransparent(GameTime gameTime)
        {
        }

        public void Dispose()
        {
            _debugSpriteBatch.Dispose();
            _debugRectTexture.Dispose();
            _effect.Dispose();
        }
    }
}