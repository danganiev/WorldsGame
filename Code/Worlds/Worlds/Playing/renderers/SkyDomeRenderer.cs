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

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.Renderers.ContentLoaders;
using WorldsGame.Terrain;
using SystemColor = System.Drawing.Color;

namespace WorldsGame.Renderers
{
    internal class SkyDomeRenderer : IRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly World _world;

        //Atmospheric settings

        //TODO accord with WorldRenderer fog constants

        internal const float FARPLANE = 220 * 4;
        internal const int FOGNEAR = 200 * 4;
        internal const int FOGFAR = 220 * 4;

        private static readonly Vector4 OVERHEADSUNCOLOR = new Color(180, 207, 255, 255).ToVector4();

        //        private static readonly Vector4 NIGHTCOLOR = Color.Black.ToVector4();
        private static readonly Vector4 HORIZONCOLOR = Color.White.ToVector4();

        private static readonly Vector4 EVENINGTINT = Color.Red.ToVector4();
        private static readonly Vector4 MORNINGTINT = Color.Gold.ToVector4();

        private readonly RasterizerState _wireframedRaster = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        private readonly RasterizerState _normalRaster = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace, FillMode = FillMode.Solid };

        // SkyDome
        private Model _skyDome;

        private Matrix _projectionMatrix;

        private float _tod;
        private bool _graphicsConstantsSet;

        internal SkyDomeRenderer(GraphicsDevice graphicsDevice, World world)
        {
            _graphicsDevice = graphicsDevice;
            _world = world;
        }

        public void Initialize()
        {
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
            // SkyDome
            _skyDome = content.Load<Model>("Models\\dome");
            _skyDome.Meshes[0].MeshParts[0].Effect = content.Load<Effect>("Effects\\SkyDome");

            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _graphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);
        }

        public void Dispose()
        {
            _wireframedRaster.Dispose();
            _normalRaster.Dispose();
        }

        public void Draw(GameTime gameTime)
        {
            _graphicsDevice.RasterizerState = !_world.IsWireframed ? _normalRaster : _wireframedRaster;

            Matrix currentViewMatrix = _world.ClientPlayer.CameraView;

            _tod = _world.TimeOfDay;

            var modelTransforms = new Matrix[_skyDome.Bones.Count];
            var inverseModelTransforms = new Matrix[_skyDome.Bones.Count];
            _skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);
            _skyDome.CopyAbsoluteBoneTransformsTo(inverseModelTransforms);

            for (int index = 0; index < _skyDome.Bones.Count; index++)
            {
                inverseModelTransforms[index] = Matrix.CreateRotationZ(MathHelper.Pi) * modelTransforms[index];
            }

            // Stars (this also colors the dome)
            var wStarMatrix = CreateSkydomeMatrix();
            DrawSky(modelTransforms, wStarMatrix, currentViewMatrix);
            DrawSky(inverseModelTransforms, wStarMatrix, currentViewMatrix);
        }

        private Matrix CreateSkydomeMatrix()
        {
            Matrix wStarMatrix = Matrix.CreateTranslation(0, -0.1f, 0) *
                                 Matrix.CreateScale(110) * Matrix.CreateTranslation(_world.ClientPlayer.CameraPosition);
            return wStarMatrix;
        }

        private void DrawSky(Matrix[] modelTransforms, Matrix wSkyMatrix, Matrix currentViewMatrix)
        {
            foreach (ModelMesh mesh in _skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wSkyMatrix;

                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];

                    if (!_graphicsConstantsSet)
                    {
                        currentEffect.Parameters["xProjection"].SetValue(_projectionMatrix);
                        //                        currentEffect.Parameters["NightColor"].SetValue(NIGHTCOLOR);
                        currentEffect.Parameters["HorizonColor"].SetValue(HORIZONCOLOR);

                        currentEffect.Parameters["MorningTint"].SetValue(MORNINGTINT);
                        currentEffect.Parameters["EveningTint"].SetValue(EVENINGTINT);

                        _graphicsConstantsSet = true;
                    }

                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);

                    currentEffect.Parameters["TimeOfDay"].SetValue(_tod);
                    currentEffect.Parameters["CurrentAtmosphereColor"].SetValue(_world.CurrentAtmosphereColor.ToVector4());
                    currentEffect.Parameters["NextAtmosphereColor"].SetValue(_world.TimeUpdater.NextAtmosphereColor.ToVector4());
                    currentEffect.Parameters["PreviousHour"].SetValue(_world.TimeUpdater.PreviousHour);
                    currentEffect.Parameters["NextHour"].SetValue(_world.TimeUpdater.NextHour);
                }
                mesh.Draw();
            }
        }

        public void DrawTransparent(GameTime gameTime)
        {
        }
    }
}