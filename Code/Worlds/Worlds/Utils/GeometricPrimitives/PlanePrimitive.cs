using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TextureSave = WorldsGame.Saving.Texture;

namespace WorldsGame.Utils.GeometricPrimitives
{
    public enum PlaneNormalOrientationEnum
    {
        None = 0,
        Forward = 1,
        Up = 2,
        Right = 3
    }

    /// <summary>
    /// Geometric primitive class for drawing cuboids.
    /// Used only in model editor, for actual in-game use there are other classes.
    /// </summary>
    public sealed class PlanePrimitive : GeometricPrimitive
    {
        private static readonly Color UNSELECTED_COLOR = Color.Gray;
        private static readonly Color SELECTED_COLOR = Color.DarkGray;
        private static readonly Color UNSELECTED_ITEM_COLOR = new Color(Color.White.R - 15, Color.White.G - 15, Color.White.B - 15, 200);
        private static readonly Color SELECTED_ITEM_COLOR = new Color(Color.White.R, Color.White.G, Color.White.B, 200);

        private static Texture2D _onePixelTexture;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly float _xWidth;
        private readonly float _yWidth;

        private Vector3 _minPoint;
        private Vector3 _maxPoint;

        public Color Color
        {
            get
            {
                if (IsSelected)
                {
                    return IsItem ? SELECTED_ITEM_COLOR : SELECTED_COLOR;
                }
                else
                {
                    return IsItem ? UNSELECTED_ITEM_COLOR : UNSELECTED_COLOR;
                }
            }
        }

        private VertexPositionColorTexture[] _coordinateLineVertices;

        public bool IsSelected { get; set; }

        public bool IsItem { get; set; }

        public Vector3 InitialDistance { get; set; }

        public Texture2D Texture { get; private set; }

        public string TextureName { get; private set; }

        // Not normalized for the sake of storing height
        public Vector3 Normal { get; set; }

        public BoundingBox BoundingBox
        {
            get
            {
                RecalculateMinMaxVertices();
                return new BoundingBox(_minPoint, _maxPoint);
            }
        }

        public PlaneNormalOrientationEnum Orientation { get; set; }

        /// <summary>
        /// Constructs a new plane primitive, using default settings.
        /// </summary>
        public PlanePrimitive(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 1, 1, Vector3.Up)
        {
        }

        /// <summary>
        /// Constructs a new plane primitive, with the specified size.
        /// </summary>
        //        public PlanePrimitive(GraphicsDevice graphicsDevice, float xWidth, float yWidth, Vector3 normal, Vector3 position)
        public PlanePrimitive(
            GraphicsDevice graphicsDevice, float xWidth, float yWidth, Vector3 initialDistance, Texture2D texture = null,
            string textureName = "")
        {
            // A plane has only one face
            if (_onePixelTexture == null)
            {
                _onePixelTexture = new Texture2D(graphicsDevice, 1, 1);
            }
            _graphicsDevice = graphicsDevice;
            _xWidth = xWidth;
            _yWidth = yWidth;
            InitialDistance = initialDistance;
            Texture = texture;
            TextureName = textureName;

            RecalculateVertices();
        }

        public PlanePrimitive(GraphicsDevice graphicsDevice, List<Vector3> vertices, Texture2D texture = null, string textureName = "", PlaneNormalOrientationEnum orientation = PlaneNormalOrientationEnum.None)
        {
            if (_onePixelTexture == null)
            {
                _onePixelTexture = new Texture2D(graphicsDevice, 1, 1);
            }

            _graphicsDevice = graphicsDevice;
            Texture = texture;
            TextureName = textureName;
            Orientation = orientation;

            InitialDistance = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]);

            RecalculateVertices(vertices);
        }

        internal void LoadTexture(string worldName, string textureName)
        {
            if (textureName != "")
            {
                ResetTexture(TextureSave.Load(worldName, textureName));
            }
        }

        public List<Vector3> GetVerticesPositions()
        {
            return (from vertex in vertices
                    select vertex.Position).ToList();
        }

        public BoundingBox GetBoundingBox()
        {
            RecalculateMinMaxVertices();
            return new BoundingBox(_minPoint, _maxPoint);
        }

        private void RecalculateMinMaxVertices()
        {
            var vertices = new List<Vector3>();
            vertices.AddRange(GetVerticesPositions());

            for (int index = 0; index < vertices.Count; index++)
            {
                Vector3 vertice = vertices[index];
                if (index == 0)
                {
                    _minPoint = vertice;
                    _maxPoint = vertice;
                }
                else
                {
                    _minPoint = Vector3.Min(_minPoint, vertice);
                    _maxPoint = Vector3.Max(_maxPoint, vertice);
                }
            }
        }

        public override void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            // NOTE: There was a geisenbug here

            // Create a vertex declaration, describing the format of our vertex data.

            // Create a vertex buffer, and copy our vertex data into it.
            _vertexBuffer = new VertexBuffer(graphicsDevice,
                typeof(VertexPositionTexture), vertices.Count, BufferUsage.None);
            _vertexBuffer.SetData(vertices.ToArray());
            verticesCount = vertices.Count;

            // Create an index buffer, and copy our index data into it.
            _indexBuffer = new IndexBuffer(graphicsDevice, typeof(int),
                                          indices.Count, BufferUsage.None);

            _indexBuffer.SetData(indices.ToArray());

            if (Orientation != PlaneNormalOrientationEnum.None)
            {
                SetupCoordinateLines();
            }
        }

        internal void SetupCoordinateLines()
        {
            Color color = Color.Black;

            switch (Orientation)
            {
                case PlaneNormalOrientationEnum.Forward:
                    color = Color.Red;
                    break;

                case PlaneNormalOrientationEnum.Up:
                    color = Color.Blue;
                    break;

                case PlaneNormalOrientationEnum.Right:
                    color = Color.Green;
                    break;
            }

            RecalculateMinMaxVertices();

            List<Vector3> vertices = GetVerticesPositions();

            Vector3 vector = Vector3.Cross(vertices[0] - vertices[1], vertices[2] - vertices[1]);

            _coordinateLineVertices = new VertexPositionColorTexture[2];

            _coordinateLineVertices[0].Position = (_maxPoint + _minPoint) / 2 /*- vector*/;
            _coordinateLineVertices[0].TextureCoordinate = new Vector2(0, 0);
            _coordinateLineVertices[0].Color = color;
            _coordinateLineVertices[1].Position = (_maxPoint + _minPoint) / 2 + vector;
            _coordinateLineVertices[1].TextureCoordinate = new Vector2(0, 0);
            _coordinateLineVertices[1].Color = color;
        }

        private void RecalculateVertices(List<Vector3> newVertices = null)
        {
            vertices.Clear();
            verticesCount = 0;
            indices.Clear();

            // Six indices (two triangles) per face.
            AddIndex(0);
            AddIndex(1);
            AddIndex(2);

            AddIndex(0);
            AddIndex(2);
            AddIndex(3);

            Vector3 side1 = Vector3.Normalize(new Vector3(InitialDistance.Y, InitialDistance.Z, InitialDistance.X));
            Vector3 side2 = Vector3.Normalize(Vector3.Cross(InitialDistance, side1));

            Normal = Vector3.Cross(side1, side2);

            if (newVertices == null)
            {
                AddVertex(InitialDistance - side1 * _yWidth / 2 - side2 * _xWidth / 2, new Vector2(1, 0));
                AddVertex(InitialDistance - side1 * _yWidth / 2 + side2 * _xWidth / 2, new Vector2(1, 1));
                AddVertex(InitialDistance + side1 * _yWidth / 2 + side2 * _xWidth / 2, new Vector2(0, 1));
                AddVertex(InitialDistance + side1 * _yWidth / 2 - side2 * _xWidth / 2, new Vector2(0, 0));
            }
            else
            {
                AddVertex(newVertices[0], new Vector2(1, 0));
                AddVertex(newVertices[1], new Vector2(1, 1));
                AddVertex(newVertices[2], new Vector2(0, 1));
                AddVertex(newVertices[3], new Vector2(0, 0));
            }

            InitializePrimitive(_graphicsDevice);
        }

        public float GetPlaneRadius()
        {
            return (float)Math.Sqrt(_xWidth * _xWidth + _yWidth * _yWidth) / 2;
        }

        public void ResetTexture(TextureSave texture)
        {
            List<Vector3> currentVertices = GetVerticesPositions();
            Texture = texture.GetTexture(_graphicsDevice);
            TextureName = texture.Name;
            RecalculateVertices(currentVertices);
        }

        public void Draw(Matrix world, Matrix view, Matrix projection, AlphaTestEffect textureEffect)
        {
            Draw(world, view, projection, Texture, textureEffect);

            if (Orientation != PlaneNormalOrientationEnum.None)
            {
                textureEffect.World = world;
                textureEffect.View = view;
                textureEffect.Projection = projection;
                textureEffect.VertexColorEnabled = true;

                textureEffect.CurrentTechnique.Passes[0].Apply();

                _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _coordinateLineVertices, 0, 1);

                textureEffect.VertexColorEnabled = false;
            }
        }

        private void Draw(Matrix world, Matrix view, Matrix projection, Texture2D texture, AlphaTestEffect effect)
        {
            //If lighting is enabled, the vertex must have a normal type.
            //If vertex colors are enabled, the vertex must have colors.
            //If texturing is enabled, the vertex must have a texture coordinate.

            // Set BasicEffect parameters.
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;
            if (texture != null)
            {
                effect.Texture = texture;
            }
            else
            {
                effect.GraphicsDevice.Textures[0] = null;
                Color color;
                if (IsItem)
                {
                    color = IsSelected ? SELECTED_ITEM_COLOR : UNSELECTED_ITEM_COLOR;
                }
                else
                {
                    color = IsSelected ? SELECTED_COLOR : UNSELECTED_COLOR;
                }

                _onePixelTexture.SetData(new[] { color });
                effect.Texture = _onePixelTexture;
            }

            GraphicsDevice device = effect.GraphicsDevice;

            effect.Alpha = IsSelected || IsItem ? 0.5f : 1f;
            device.SamplerStates[0] = SamplerState.PointWrap;

            Draw(effect);
        }

        public void RotateTexture()
        {
            List<Vector3> newVertices;
            newVertices = new List<Vector3>
                {
                    vertices[1].Position,
                    vertices[2].Position,
                    vertices[3].Position,
                    vertices[0].Position
                };

            RecalculateVertices(newVertices);
        }

        public bool Intersects(PlanePrimitive plane)
        {
            // NOTE: This is super ineffective, cause for rotated planes it still would return
            // non-rotated and even non-flat bounding boxes
            return plane.BoundingBox.Intersects(BoundingBox);
        }
    }
}