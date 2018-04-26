using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using WorldsGame.Playing.Players;
using WorldsGame.Playing.Renderers.ContentLoaders;

namespace WorldsGame.Playing.Renderers.Character
{
    internal class NetworkCharactersRenderer : ICharactersRenderer
    {
        private const float BILLBOARD_TEXT_SIZE = 0.02f;

        private BasicEffect _playerCharacterEffect;
        private readonly ClientPlayerManager _playerManager;

        private readonly GraphicsDevice _graphicsDevice;
        private Model _playerModel;
        private Texture2D _playerTexture;

        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        internal NetworkCharactersRenderer(GraphicsDevice graphicsDevice, ClientPlayerManager playerManager)
        {
            _graphicsDevice = graphicsDevice;
            _playerManager = playerManager;
        }

        public void Initialize()
        {
            _playerCharacterEffect = new BasicEffect(_graphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true
            };
        }

        public void Draw(GameTime gameTime)
        {
            _graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            foreach (Player otherPlayer in _playerManager.OtherPlayers)
            {
                if (otherPlayer == null)
                {
                    continue;
                }

                Matrix matrixA = Matrix.CreateScale(0.6f) * Matrix.CreateRotationY(MathHelper.ToRadians(90)) * Matrix.CreateRotationY(otherPlayer.LeftRightRotation) *
                        Matrix.CreateTranslation(otherPlayer.Position /*+ new Vector3(0, 1.3f, 0)*/);

                foreach (ModelMesh mesh in _playerModel.Meshes)
                {
                    Matrix[] transforms = new Matrix[_playerModel.Bones.Count];
                    _playerModel.CopyAbsoluteBoneTransformsTo(transforms);

                    Matrix world = transforms[mesh.ParentBone.Index] * matrixA;

                    foreach (SkinnedEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();

                        effect.World = world;

                        effect.Texture = _playerTexture;
                        effect.View = _playerManager.ClientPlayer.CameraView;
                        effect.Projection = _playerManager.ClientPlayer.CameraProjection;
                    }

                    mesh.Draw();
                }
            }
        }

        // http://blogs.msdn.com/b/shawnhar/archive/2011/01/12/spritebatch-billboards-in-a-3d-world.aspx
        private void DrawUsernameBillboard()
        {
            foreach (Player otherPlayer in _playerManager.OtherPlayers)
            {
                if (otherPlayer == null)
                {
                    continue;
                }

                _playerCharacterEffect.World = Matrix.CreateConstrainedBillboard(
                    otherPlayer.HeadPosition + new Vector3(0, 0.5f, 0), _playerManager.ClientPlayer.CameraPosition, Vector3.Down, null, null);
                _playerCharacterEffect.View = _playerManager.ClientPlayer.CameraView;
                _playerCharacterEffect.Projection = _playerManager.ClientPlayer.CameraProjection;

                Vector2 textOrigin = _spriteFont.MeasureString(otherPlayer.Username) / 2;

                _spriteBatch.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone,
                                   _playerCharacterEffect);
                _spriteBatch.DrawString(_spriteFont, otherPlayer.Username, Vector2.Zero, Color.Black, 0, textOrigin, BILLBOARD_TEXT_SIZE, 0, 0);
                _spriteBatch.End();
            }
        }

        public void DrawTransparent(GameTime gameTime)
        {
            DrawUsernameBillboard();
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
            _playerModel = content.Load<Model>("Models\\CharacterModel");
            _playerTexture = content.Load<Texture2D>("Models\\Character");

            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _spriteFont = content.Load<SpriteFont>("Fonts/DefaultFont");
        }

        public void Dispose()
        {
        }
    }
}