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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using WorldsGame.Playing.Players;
using WorldsGame.Playing.Renderers.ContentLoaders;

namespace WorldsGame.Renderers
{
    /*render selection block */

    internal class SelectionBlockRenderer : IRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private BasicEffect _selectionBlockEffect;
        private Texture2D _selectionBlockTexture;

        private readonly Player _player;

        // SelectionBlock
        private Model _selectionBlock;

        internal SelectionBlockRenderer(GraphicsDevice graphicsDevice, Player player)
        {
            _graphicsDevice = graphicsDevice;
            _player = player;
        }

        public void Initialize()
        {
            _selectionBlockEffect = new BasicEffect(_graphicsDevice);
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
            _selectionBlock = content.Load<Model>("Models\\SelectionBlock");
            _selectionBlockTexture = content.Load<Texture2D>("Textures\\SelectionBlock");
        }

        public void Dispose()
        {
            _selectionBlockEffect.Dispose();
        }

        internal void RenderSelectionBlock(GameTime gameTime)
        {
            _graphicsDevice.BlendState = BlendState.NonPremultiplied; // allows any transparent pixels in original PNG to draw transparent

            if (_player.CurrentSelection == null || _player.HideSelectionBlock)
            {
                return;
            }

            Vector3 position = _player.CurrentSelection.Position.AsVector3() + new Vector3(0.5f, 0.5f, 0.5f);

            Matrix matrixA, matrixB;
            Matrix.CreateTranslation(ref position, out matrixA); // translate the position a half block in each direction
            Matrix.CreateScale(0.505f, out matrixB); // scales the selection box slightly larger than the targeted block

            Matrix identity = Matrix.Multiply(matrixB, matrixA);

            // set up the World, View and Projection
            _selectionBlockEffect.World = identity;
            _selectionBlockEffect.View = _player.CameraView;
            _selectionBlockEffect.Projection = _player.CameraProjection;
            _selectionBlockEffect.Texture = _selectionBlockTexture;
            _selectionBlockEffect.TextureEnabled = true;

            // apply the effect
            foreach (EffectPass pass in _selectionBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                DrawSelectionBlockMesh(_selectionBlock.Meshes[0]);
            }

            _graphicsDevice.BlendState = BlendState.Opaque;
        }

        private void DrawSelectionBlockMesh(ModelMesh mesh)
        {
            int count = mesh.MeshParts.Count;
            for (int i = 0; i < count; i++)
            {
                ModelMeshPart parts = mesh.MeshParts[i];
                if (parts.NumVertices > 0)
                {
                    _graphicsDevice.Indices = parts.IndexBuffer;
                    _graphicsDevice.SetVertexBuffer(parts.VertexBuffer);
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, parts.NumVertices, parts.StartIndex, parts.PrimitiveCount);
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            RenderSelectionBlock(gameTime);
        }

        public void DrawTransparent(GameTime gameTime)
        {
        }
    }
}