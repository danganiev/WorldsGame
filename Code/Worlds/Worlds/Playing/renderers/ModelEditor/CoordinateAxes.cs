using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Camera;
using WorldsGame.Editors.Blocks;

namespace WorldsGame.Playing.Renderers
{
    internal class CoordinateAxes : IDisposable
    {
        private VertexPositionColor[] _coordinateSystemVerticesX, _coordinateSystemVerticesY, _coordinateSystemVerticesZ;

        private BasicEffect _effect;
        private GraphicsDevice _device;
        private readonly EditedModel _editedModel;

        private int _xSize;
        private int _ySize;
        private int _zSize;

        internal CoordinateAxes(GraphicsDevice device, EditedModel editedModel)
        {
            _device = device;
            _editedModel = editedModel;
            _coordinateSystemVerticesX = new VertexPositionColor[2];
            _coordinateSystemVerticesY = new VertexPositionColor[2];
            _coordinateSystemVerticesZ = new VertexPositionColor[2];

            _xSize = editedModel.LengthInBlocks * EditedModel.BLOCK_SIZE / 2;
            _ySize = editedModel.HeightInBlocks * EditedModel.BLOCK_SIZE / 2;
            _zSize = editedModel.WidthInBlocks * EditedModel.BLOCK_SIZE / 2;

            _effect = new BasicEffect(device)
            {
                VertexColorEnabled = true
            };
        }

        internal void SetupCoordinateLines()
        {
            _coordinateSystemVerticesX[0].Position = new Vector3(-_xSize, -_ySize, -_zSize);
            _coordinateSystemVerticesX[0].Color = Color.Green;
            _coordinateSystemVerticesX[1].Position = new Vector3(_xSize * 3, -_ySize, -_zSize);
            _coordinateSystemVerticesX[1].Color = Color.Green;
            _coordinateSystemVerticesY[0].Position = new Vector3(-_xSize, -_ySize, -_zSize);
            _coordinateSystemVerticesY[0].Color = Color.Blue;
            _coordinateSystemVerticesY[1].Position = new Vector3(-_xSize, _ySize * 3, -_zSize);
            _coordinateSystemVerticesY[1].Color = Color.Blue;
            _coordinateSystemVerticesZ[0].Position = new Vector3(-_xSize, -_ySize, -_zSize);
            _coordinateSystemVerticesZ[0].Color = Color.Red;
            _coordinateSystemVerticesZ[1].Position = new Vector3(-_xSize, -_ySize, _zSize * 3);
            _coordinateSystemVerticesZ[1].Color = Color.Red;
        }

        internal void DrawCoordinateSystem(Camera.Camera camera)
        {
            _effect.View = camera.View;
            _effect.Projection = camera.Projection;

            // Inside your Game.Draw method
            _effect.CurrentTechnique.Passes[0].Apply();
            _device.DrawUserPrimitives(PrimitiveType.LineList, _coordinateSystemVerticesX, 0, 1);
            _device.DrawUserPrimitives(PrimitiveType.LineList, _coordinateSystemVerticesY, 0, 1);
            _device.DrawUserPrimitives(PrimitiveType.LineList, _coordinateSystemVerticesZ, 0, 1);

            //            _parallelLine[0].Position = Vector3.Zero;
            //            _parallelLine[0].Color = Color.Black;
            //            _parallelLine[1].Position = _camera.HorizontalPerpendicularLine;
            //            _parallelLine[0].Color = Color.Black;
            //            _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _parallelLine, 0, 1);
        }

        public void Dispose()
        {
            _effect.Dispose();
        }
    }
}