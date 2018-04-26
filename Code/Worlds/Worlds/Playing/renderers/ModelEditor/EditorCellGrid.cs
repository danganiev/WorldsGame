using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Editors.Blocks;

namespace WorldsGame.Playing.Renderers
{
    internal class EditorCellGrid
    {
        private VertexPositionColor[] _gridVertices;

        private readonly BasicEffect _effect;
        private readonly GraphicsDevice _device;

        private int _xSize;
        private int _ySize;
        private int _zSize;

        internal EditorCellGrid(GraphicsDevice device, EditedModel editedModel)
            : this(
            device, editedModel.LengthInBlocks * EditedModel.BLOCK_SIZE,
            editedModel.HeightInBlocks * EditedModel.BLOCK_SIZE,
            editedModel.WidthInBlocks * EditedModel.BLOCK_SIZE)
        {
        }

        internal EditorCellGrid(GraphicsDevice device, int xSize, int ySize, int zSize)
        {
            _xSize = xSize;
            _ySize = ySize;
            _zSize = zSize;

            // two vertices per line * sizes * ?
            _gridVertices = new VertexPositionColor[2 * ((_xSize + 1) + (_ySize + 1) + (_zSize + 1)) * 2];

            _device = device;

            _effect = new BasicEffect(device)
            {
                VertexColorEnabled = true
            };

            FillVertices();
        }

        private void FillVertices()
        {
            Vector3 minusSixteen = new Vector3(-_xSize, -_ySize, -_zSize) / 2;
            int j = 0;
            for (int i = 1; i < _xSize + 1; i++)
            {
                _gridVertices[j] = new VertexPositionColor(new Vector3(i, 0, 0) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
                _gridVertices[j] = new VertexPositionColor(new Vector3(i, 0, _zSize) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
            }
            for (int i = 1; i < _zSize + 1; i++)
            {
                _gridVertices[j] = new VertexPositionColor(new Vector3(0, 0, i) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
                _gridVertices[j] = new VertexPositionColor(new Vector3(_xSize, 0, i) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
            }
            for (int i = 1; i < _zSize + 1; i++)
            {
                _gridVertices[j] = new VertexPositionColor(new Vector3(0, 0, i) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
                _gridVertices[j] = new VertexPositionColor(new Vector3(0, _ySize, i) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
            }
            for (int i = 1; i < _ySize + 1; i++)
            {
                _gridVertices[j] = new VertexPositionColor(new Vector3(0, i, 0) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
                _gridVertices[j] = new VertexPositionColor(new Vector3(0, i, _zSize) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
            }
            for (int i = 1; i < _xSize + 1; i++)
            {
                _gridVertices[j] = new VertexPositionColor(new Vector3(i, 0, 0) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
                _gridVertices[j] = new VertexPositionColor(new Vector3(i, _ySize, 0) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
            }
            for (int i = 1; i < _ySize + 1; i++)
            {
                _gridVertices[j] = new VertexPositionColor(new Vector3(0, i, 0) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
                _gridVertices[j] = new VertexPositionColor(new Vector3(_xSize, i, 0) + minusSixteen,
                    i % EditedModel.BLOCK_SIZE == 0 ? Color.Black : Color.Gray);
                j++;
            }
        }

        internal void Draw(Camera.Camera camera)
        {
            _effect.View = camera.View;
            _effect.Projection = camera.Projection;

            // Inside your Game.Draw method
            _effect.CurrentTechnique.Passes[0].Apply();
            _device.DrawUserPrimitives(PrimitiveType.LineList, _gridVertices, 0, ((_xSize + 1) + (_ySize + 1) + (_zSize + 1)) * 2);
        }
    }
}