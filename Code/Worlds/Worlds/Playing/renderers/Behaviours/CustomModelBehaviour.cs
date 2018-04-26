using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Players;

namespace WorldsGame.Playing.Entities
{
    internal class CustomModelBehaviour : EntityBehaviour
    {
        public override bool IsUpdateable
        {
            get
            {
                return false;
            }
        }

        public override void Draw(GameTime gameTime, Entity owner)
        {
            var worldComponent = owner.GetConstantComponent<WorldComponent>();
            var contentLoaderComponent = owner.GetConstantComponent<ContentLoaderComponent>();
            var scaleAndRotationComponent = owner.GetComponent<ScaleAndRotateComponent>();
            var positionComponent = owner.GetComponent<PositionComponent>();
            var animationComponent = owner.GetComponent<AnimationComponent>();

            Effect customModelEffect = contentLoaderComponent.ContentLoader.CustomModelEffect;

            Player player = worldComponent.World.ClientPlayer;

            customModelEffect.Parameters["View"].SetValue(player.CameraView);
            customModelEffect.Parameters["Projection"].SetValue(player.CameraProjection);
            customModelEffect.Parameters["CameraPosition"].SetValue(player.CameraPosition);

            Matrix modelWorldMatrix = Matrix.CreateRotationY(scaleAndRotationComponent.LeftRightRotation) *
                                      Matrix.CreateScale(scaleAndRotationComponent.Scale) *
                                      Matrix.CreateTranslation(positionComponent.Position);

            var buffers = owner.GetComponent<CustomModelBuffersComponent>();

            var textureAtlases = contentLoaderComponent.ContentLoader.TextureAtlases;

            foreach (KeyValuePair<int, Texture2D> textureAtlas in textureAtlases)
            {
                if (textureAtlas.Key == -1)
                {
                    continue;
                }

                // NOTE: It would make big sense if we will combine buffers from all entities into one dict entry per atlas.
                // Then the atlas would be changed only once per tick on the GPU (well twice because of blocks)
                customModelEffect.Parameters["TextureAtlas"].SetValue(textureAtlas.Value);

                for (int i = 0; i < buffers.VertexBuffers[textureAtlas.Key].Count; i++)
                {
                    VertexBuffer vertexBuffer = buffers.VertexBuffers[textureAtlas.Key][i];
                    IndexBuffer indexBuffer = buffers.IndexBuffers[textureAtlas.Key][i];

                    Matrix animationWorldMatrix = modelWorldMatrix;

                    if (animationComponent != null && animationComponent.CurrentlyPlayedAnimations.Count > 0)
                    {
                        animationWorldMatrix = Matrix.CreateRotationY(scaleAndRotationComponent.LeftRightRotation) * animationComponent.CurrentKeyframes[i].Rotation *
                            Matrix.CreateScale(scaleAndRotationComponent.Scale) * animationComponent.CurrentKeyframes[i].Scale *
                            Matrix.CreateTranslation(positionComponent.Position) * animationComponent.CurrentKeyframes[i].Translation;
                    }

                    customModelEffect.Parameters["World"].SetValue(animationWorldMatrix);

                    CustomModelPassLoop(worldComponent.World.Graphics, customModelEffect, vertexBuffer, indexBuffer);
                }
            }
        }

        private void CustomModelPassLoop(GraphicsDevice graphicsDevice, Effect effect, VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            if (indexBuffer != null && vertexBuffer != null && !indexBuffer.IsDisposed &&
                    !vertexBuffer.IsDisposed)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    graphicsDevice.Indices = indexBuffer;
                    graphicsDevice.SetVertexBuffer(vertexBuffer);
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                            vertexBuffer.VertexCount, 0,
                                                            indexBuffer.IndexCount / 3);
                }
            }
        }
    }
}