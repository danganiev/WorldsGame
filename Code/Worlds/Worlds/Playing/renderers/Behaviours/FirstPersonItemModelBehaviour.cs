using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Players;

namespace WorldsGame.Playing.Entities
{
    internal class FirstPersonItemModelBehaviour : EntityBehaviour
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
            //            var positionComponent = owner.GetComponent<PositionComponent>();
            var animationComponent = owner.GetComponent<AnimationComponent>();

            Effect customModelEffect = contentLoaderComponent.ContentLoader.CustomModelEffect;

            Player player = worldComponent.World.ClientPlayer;

            customModelEffect.Parameters["View"].SetValue(player._fpsCamera.View);
            customModelEffect.Parameters["Projection"].SetValue(player._fpsCamera.Projection);
            customModelEffect.Parameters["CameraPosition"].SetValue(player._fpsCamera.Position);

            Matrix cameraWorld = Matrix.Invert(player._fpsCamera.View);

            Matrix modelWorldMatrix = cameraWorld;

            Vector3 dscale;
            Quaternion drotation;
            Vector3 dtranslation;
            modelWorldMatrix.Decompose(out dscale, out drotation, out dtranslation);

            var translation = owner.GetComponent<CustomModelComponent>().AdditionalTranslationMatrix;

            if (!(animationComponent != null && animationComponent.CurrentlyPlayedAnimations.Count > 0))
            {
                modelWorldMatrix = Matrix.CreateTranslation((Vector3.Forward * translation.Translation.Z * 32) +
                                                            (Vector3.Up * (translation.Translation.Y * 32 - 1.8f)) +
                                                            (Vector3.Right * -translation.Translation.X * 32)) *
                                   Matrix.CreateScale(dscale) *
                                   Matrix.CreateFromQuaternion(drotation) *
                                   Matrix.CreateTranslation(dtranslation);
            }

            var buffers = owner.GetComponent<CustomModelBuffersComponent>();

            var textureAtlases = contentLoaderComponent.ContentLoader.TextureAtlases;

            worldComponent.World.Graphics.DepthStencilState = DepthStencilState.None;

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

                    Matrix animationWorldMatrix;

                    if (animationComponent != null && animationComponent.CurrentlyPlayedAnimations.Count > 0)
                    {
                        //                        animationWorldMatrix = Matrix.CreateRotationY(scaleAndRotationComponent.LeftRightRotation) * animationComponent.CurrentKeyframes[i].Rotation *
                        //                                                Matrix.CreateScale(scaleAndRotationComponent.Scale) * animationComponent.CurrentKeyframes[i].Scale *
                        //                                                Matrix.CreateTranslation(positionComponent.Position) * animationComponent.CurrentKeyframes[i].Translation;

                        animationWorldMatrix = Matrix.CreateTranslation((Vector3.Forward * translation.Translation.Z * 32) +
                                    (Vector3.Up * (translation.Translation.Y * 32 - 1.8f)) +
                                    (Vector3.Right * -translation.Translation.X * 32)) *
                                animationComponent.CurrentKeyframes[i].Scale *
                                animationComponent.CurrentKeyframes[i].Rotation *
                                animationComponent.CurrentKeyframes[i].Translation *
                                Matrix.CreateScale(dscale) *
                                Matrix.CreateFromQuaternion(drotation) *
                                Matrix.CreateTranslation(dtranslation);
                    }
                    else
                    {
                        animationWorldMatrix = modelWorldMatrix;
                    }

                    customModelEffect.Parameters["World"].SetValue(animationWorldMatrix);

                    CustomModelPassLoop(worldComponent.World.Graphics, customModelEffect, vertexBuffer, indexBuffer);
                }
            }

            worldComponent.World.Graphics.DepthStencilState = DepthStencilState.Default;
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