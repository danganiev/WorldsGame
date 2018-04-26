using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Editors.Blocks;
using WorldsGame.Playing.VertexTypes;
using Plane = WorldsGame.Saving.Plane;
using Texture = WorldsGame.Saving.Texture;

// To help in the future: https://stackoverflow.com/questions/8462148/how-to-create-model-from-scratch
namespace WorldsGame.Utils.GeometricPrimitives
{
    /// <summary>
    /// Geometric primitive class for drawing cuboids.
    /// </summary>
    public class CuboidPrimitive : GeometricPrimitive
    {
        private Dictionary<int, PlanePrimitive> _planes;
        private Matrix _world = Matrix.Identity;

        private GraphicsDevice _graphicsDevice;

        private bool _areMinMaxVerticesRecalculated = false;
        private PlanePrimitive _selectedPlane;

        public Vector3 Position { get; private set; }

        public float Yaw { get; private set; }

        public float Pitch { get; private set; }

        public float Roll { get; private set; }

        private Vector3 _minPoint;

        public Vector3 MinPoint
        {
            get
            {
                if (!_areMinMaxVerticesRecalculated)
                {
                    RecalculateMinMaxVertices();
                }
                return _minPoint;
            }
            private set { _minPoint = value; }
        }

        private Vector3 _maxPoint;

        public Vector3 MaxPoint
        {
            get
            {
                if (!_areMinMaxVerticesRecalculated)
                {
                    RecalculateMinMaxVertices();
                }
                return _maxPoint;
            }
            private set { _maxPoint = value; }
        }

        public bool IsItem { get; private set; }

        public bool IsSticky { get; set; }

        public CuboidPrimitive StickedToCuboid { get; private set; }

        public List<CuboidPrimitive> ChildCuboids { get; private set; }

        internal EditedModel EditedModel { get; set; }

        internal event Action<Matrix> OnRotate = rotationMatrix => { };

        internal event Action<Matrix> OnScale = scaleMatrix => { };

        internal event Action<Matrix> OnTranslate = translationMatrix => { };

        /// <summary>
        /// Constructs a new cuboid primitive, using default settings.
        /// </summary>
        public CuboidPrimitive(GraphicsDevice graphicsDevice, EditedModel editedModel)

            : this(graphicsDevice, new Vector3(1, 1, 1), Vector3.Zero, editedModel)
        {
        }

        /// <summary>
        /// Constructs a new cuboid primitive, with the specified size.
        /// </summary>
        public CuboidPrimitive(
            GraphicsDevice graphicsDevice, Vector3 size,
            Vector3 position, EditedModel editedModel = null, bool isItem = false)
        {
            // editedModel = null is only used on default player generation as for v8

            _graphicsDevice = graphicsDevice;
            EditedModel = editedModel;
            Position = position;
            IsItem = isItem;

            // A cuboid has six faces, each one pointing in a different direction.
            _planes = new Dictionary<int, PlanePrimitive>();

            InitializePrimitive(size);
        }

        public CuboidPrimitive(
            GraphicsDevice graphicsDevice, float yaw, float pitch, float roll,
            Vector3 position, List<PlanePrimitive> planes, EditedModel editedModel, bool isItem = false)
        {
            _graphicsDevice = graphicsDevice;
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
            Position = position;
            EditedModel = editedModel;
            IsItem = isItem;
            _planes = new Dictionary<int, PlanePrimitive>();

            if (planes != null)
            {
                for (int i = 0; i < planes.Count; i++)
                {
                    PlanePrimitive planePrimitive = planes[i];

                    if (isItem)
                    {
                        planePrimitive.IsItem = true;
                    }

                    _planes.Add(i, planePrimitive);
                }
            }

            _world = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) * Matrix.CreateTranslation(Position);
            UpdatePlanes();
            _world = Matrix.Identity;
        }

        private void InitializePlanes(Vector3 normal, float size1, float size2)
        {
            var plane = new PlanePrimitive(_graphicsDevice, size1, size2, normal)
            {
                IsItem = IsItem
            };

            if (IsItem)
            {
                if (Vector3.Normalize(normal) == Vector3.Backward)
                {
                    plane.Orientation = PlaneNormalOrientationEnum.Forward;
                }
                else if (Vector3.Normalize(normal) == Vector3.Up)
                {
                    plane.Orientation = PlaneNormalOrientationEnum.Up;
                }
                else if (Vector3.Normalize(normal) == Vector3.Right)
                {
                    plane.Orientation = PlaneNormalOrientationEnum.Right;
                }
            }

            // Hack to rotate all the textures normally by default
            if (Vector3.Normalize(normal) == Vector3.Backward)
            {
                plane.RotateTexture();
                plane.RotateTexture();
                plane.RotateTexture();
            }
            else if (Vector3.Normalize(normal) == Vector3.Up)
            {
                plane.RotateTexture();
                plane.RotateTexture();
            }
            _planes.Add(_planes.Count, plane);

            Vector3 negatedNormal = Vector3.Negate(normal);

            plane = new PlanePrimitive(_graphicsDevice, size1, size2, negatedNormal)
            {
                IsItem = IsItem
            };

            if (Vector3.Normalize(normal) == Vector3.Backward)
            {
                plane.RotateTexture();
            }
            else if (Vector3.Normalize(normal) == Vector3.Up)
            {
            }

            _planes.Add(_planes.Count, plane);
        }

        // This is for virtual member call breaking
        private void InitializePrimitive(Vector3 size)
        {
            _planes.Clear();

            InitializePlanes(Vector3.Right * size.X / 2, size.Y, size.Z);
            InitializePlanes(Vector3.Up * size.Y / 2, size.Z, size.X);
            InitializePlanes(Vector3.Backward * size.Z / 2, size.X, size.Y);

            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                planePrimitive.Value.InitializePrimitive(_graphicsDevice);
            }

            _world = Matrix.CreateTranslation(Position);
            UpdatePlanes();
            _world = Matrix.Identity;
        }

        public override void Draw(Effect effect)
        {
            if (ChildCuboids != null)
            {
                foreach (var cuboidPrimitive in ChildCuboids)
                {
                    cuboidPrimitive.Draw(effect);
                }
            }
            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                planePrimitive.Value.Draw(effect);
            }
        }

        public void Draw(Matrix view, Matrix projection, AlphaTestEffect textureEffect)
        {
            if (ChildCuboids != null)
            {
                foreach (var cuboidPrimitive in ChildCuboids)
                {
                    cuboidPrimitive.Draw(view, projection, textureEffect);
                }
            }
            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                planePrimitive.Value.Draw(_world, view, projection, textureEffect);
            }
        }

        public void Draw(Matrix world, Matrix view, Matrix projection, AlphaTestEffect textureEffect)
        {
            if (ChildCuboids != null)
            {
                foreach (var cuboidPrimitive in ChildCuboids)
                {
                    cuboidPrimitive.Draw(world, view, projection, textureEffect);
                }
            }

            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                planePrimitive.Value.Draw(world, view, projection, textureEffect);
            }
        }

        public BoundingBox GetBoundingBox()
        {
            RecalculateMinMaxVertices();
            return new BoundingBox(MinPoint, MaxPoint);
        }

        private void RecalculateMinMaxVertices()
        {
            if (!_areMinMaxVerticesRecalculated)
            {
                List<Vector3> distinctVertices = GetVertices();
                for (int index = 0; index < distinctVertices.Count; index++)
                {
                    Vector3 vertice = distinctVertices[index];

                    if (index == 0)
                    {
                        MinPoint = vertice;
                        MaxPoint = vertice;
                    }
                    else
                    {
                        MinPoint = Vector3.Min(_minPoint, vertice);
                        MaxPoint = Vector3.Max(_maxPoint, vertice);
                    }
                }
                _areMinMaxVerticesRecalculated = true;
            }
        }

        private List<Vector3> GetVertices()
        {
            var vertices = new List<Vector3>();
            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                vertices.AddRange(planePrimitive.Value.GetVerticesPositions());
            }

            vertices = vertices.Distinct().ToList();
            return vertices;
        }

        public void DeselectEverything()
        {
            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                planePrimitive.Value.IsSelected = false;
            }
        }

        public void DetectSelectedPlane(Ray ray)
        {
            float closestDistance = float.MaxValue;
            PlanePrimitive newPlane = null;
            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                BoundingBox box = planePrimitive.Value.GetBoundingBox();
                float? distance = ray.Intersects(box);
                if (distance != null && distance < closestDistance)
                {
                    newPlane = planePrimitive.Value;
                    closestDistance = Math.Min((float)distance, closestDistance);
                }
            }
            if (newPlane != null)
            {
                if (_selectedPlane != null)
                {
                    _selectedPlane.IsSelected = false;
                }

                _selectedPlane = newPlane;
                _selectedPlane.IsSelected = true;
            }
        }

        //// Animate the progress parameter between 0 and 1
        // Quaternion currentQ = Quaternion.Slerp(startQ, endQ, progress); thats for animations

        public void Scale(Matrix value)
        {
            _world = Matrix.CreateTranslation(-Position) * value * Matrix.CreateTranslation(Position);

            UpdatePlanes();

            if (ChildCuboids != null)
            {
                foreach (CuboidPrimitive cuboidPrimitive in ChildCuboids)
                {
                    cuboidPrimitive.Scale(value);
                }
            }

            OnScale(value);

            _world = Matrix.Identity;
        }

        public void Rotate(float delta, Vector3 normal)
        {
            Matrix rotationMatrix = Matrix.Identity;

            float rotationAngle = delta / 10;

            if (normal == Vector3.UnitX)
            {
                rotationMatrix = Matrix.CreateRotationX(rotationAngle);
                Pitch += rotationAngle;
            }
            if (normal == Vector3.UnitY)
            {
                rotationMatrix = Matrix.CreateRotationY(rotationAngle);
                Yaw += rotationAngle;
            }
            if (normal == Vector3.UnitZ)
            {
                rotationMatrix = Matrix.CreateRotationZ(rotationAngle);
                Roll += rotationAngle;
            }

            Rotate(rotationMatrix);

            //            _world = Matrix.CreateTranslation(-Position) * rotationMatrix * Matrix.CreateTranslation(Position);
            //
            //            UpdatePlanes();
            //
            //            _world = Matrix.Identity;
        }

        public void Rotate(Matrix value, Vector3 aroundPoint)
        {
            Rotate(Matrix.CreateTranslation(aroundPoint) * value * Matrix.CreateTranslation(-aroundPoint));

            //            _world = Matrix.CreateTranslation(-Position) *  * Matrix.CreateTranslation(Position);
            //
            //            UpdatePlanes();
            //
            //            _world = Matrix.Identity;
        }

        public void Rotate(Matrix value)
        {
            _world = Matrix.CreateTranslation(-Position) * value * Matrix.CreateTranslation(Position);

            UpdatePlanes();

            if (ChildCuboids != null)
            {
                foreach (CuboidPrimitive cuboidPrimitive in ChildCuboids)
                {
                    cuboidPrimitive.Rotate(value, cuboidPrimitive.Position - Position);
                }
            }

            OnRotate(value);

            _world = Matrix.Identity;
        }

        public void Translate(float delta, Vector3 normal)
        {
            if (IsItem)
            {
                delta = delta / 2;
            }

            Translate(Matrix.CreateTranslation(delta * normal));

            //            UpdatePlanes();
            //
            //            _world = Matrix.Identity;
        }

        public void Translate(Matrix value)
        {
            _world = value;

            UpdatePlanes();

            if (ChildCuboids != null)
            {
                foreach (CuboidPrimitive cuboidPrimitive in ChildCuboids)
                {
                    cuboidPrimitive.Translate(value);
                }
            }

            OnTranslate(value);

            _world = Matrix.Identity;
        }

        private void UpdatePlanes()
        {
            var newPlanes = new Dictionary<int, PlanePrimitive>();
            var newVertices = new List<Vector3>();
            var allVertices = new List<Vector3>();
            foreach (KeyValuePair<int, PlanePrimitive> plane in _planes)
            {
                newVertices.Clear();
                foreach (Vector3 vertice in plane.Value.GetVerticesPositions())
                {
                    Vector3 newVertice = Vector3.Transform(vertice, _world);
                    allVertices.Add(newVertice);
                    newVertices.Add(newVertice);
                }
                var newPlane = new PlanePrimitive(
                    _graphicsDevice, newVertices, texture: plane.Value.Texture,
                    textureName: plane.Value.TextureName, orientation: plane.Value.Orientation)
                {
                    IsSelected = plane.Value.IsSelected,
                    IsItem = plane.Value.IsItem,
                };

                if (newPlane.IsSelected)
                {
                    _selectedPlane = newPlane;
                }

                newPlanes.Add(
                    plane.Key,
                    newPlane
                );
            }
            allVertices = allVertices.Distinct().ToList();

            if (AreVerticesTooBig(allVertices))
            {
                return;
            }

            UpdatePosition(allVertices);

            _planes.Clear();

            foreach (KeyValuePair<int, PlanePrimitive> plane in newPlanes)
            {
                _planes.Add(plane.Key, plane.Value);
            }

            _areMinMaxVerticesRecalculated = false;

            CheckStickyness();
        }

        private void UpdatePlanes(Dictionary<Vector3, Vector3> oldVerticesToNewMap)
        {
            var newPlanes = new Dictionary<int, PlanePrimitive>();
            var newVertices = new List<Vector3>();

            foreach (KeyValuePair<int, PlanePrimitive> plane in _planes)
            {
                newVertices.Clear();
                foreach (Vector3 vertice in plane.Value.GetVerticesPositions())
                {
                    Vector3 newVertice = oldVerticesToNewMap[vertice];
                    newVertices.Add(newVertice);
                }

                var newPlane = new PlanePrimitive(
                    _graphicsDevice, newVertices, texture: plane.Value.Texture,
                    textureName: plane.Value.TextureName, orientation: plane.Value.Orientation)
                {
                    IsSelected = plane.Value.IsSelected,
                    IsItem = plane.Value.IsItem
                };

                if (newPlane.IsSelected)
                {
                    _selectedPlane = newPlane;
                }

                UpdatePosition(oldVerticesToNewMap.Select(pair => pair.Value).ToList());

                newPlanes.Add(plane.Key, newPlane);
            }

            _planes.Clear();

            foreach (KeyValuePair<int, PlanePrimitive> plane in newPlanes)
            {
                _planes.Add(plane.Key, plane.Value);
            }

            _areMinMaxVerticesRecalculated = false;

            CheckStickyness();
        }

        private void CheckStickyness()
        {
            if (IsSticky)
            {
                foreach (CuboidPrimitive cuboidPrimitive in EditedModel.CuboidPrimitives)
                {
                    if (cuboidPrimitive != this && cuboidPrimitive.Intersects(this))
                    {
                        if (StickedToCuboid != null && StickedToCuboid != cuboidPrimitive)
                        {
                            StickedToCuboid.OnRotate -= OnStickyParentRotate;
                            StickedToCuboid.OnTranslate -= OnStickyParentTranslate;
                            StickedToCuboid.OnScale -= OnStickyParentScale;
                        }

                        if (StickedToCuboid == cuboidPrimitive)
                        {
                            StickedToCuboid = cuboidPrimitive;
                            return;
                        }

                        StickedToCuboid = cuboidPrimitive;
                        StickedToCuboid.OnRotate += OnStickyParentRotate;
                        StickedToCuboid.OnTranslate += OnStickyParentTranslate;
                        StickedToCuboid.OnScale += OnStickyParentScale;
                        return;
                    }
                }
            }
        }

        private void OnStickyParentRotate(Matrix rotationMatrix)
        {
            Rotate(rotationMatrix, StickedToCuboid.Position + Position);
        }

        private void OnStickyParentTranslate(Matrix translationMatrix)
        {
            Translate(translationMatrix);
        }

        private void OnStickyParentScale(Matrix scaleMatrix)
        {
            Scale(scaleMatrix);
        }

        private void UpdatePosition(List<Vector3> newVertices)
        {
            Vector3 positionSum = Vector3.Zero;
            foreach (Vector3 vector3 in newVertices)
            {
                positionSum += vector3;
            }
            Position = positionSum / newVertices.Count;
        }

        public void TransformSelectedSide(float delta, Ray ray, bool reselect = false)
        {
            // Defensive programming
            if (_selectedPlane == null)
            {
                return;
            }

            if (IsItem)
            {
                delta = delta / 2;
            }

            Vector3 normal = Vector3.Normalize(_selectedPlane.InitialDistance);
            List<Vector3> transformedVertices = _selectedPlane.GetVerticesPositions();
            List<Vector3> otherVertices = GetVertices().Except(transformedVertices).ToList();
            var oldNewVerticeMap = new Dictionary<Vector3, Vector3>();
            var newVertices = new List<Vector3>();

            bool areVerticesTooClose = true;

            foreach (Vector3 transformedVertex in transformedVertices)
            {
                Vector3 newVertice = transformedVertex + normal * -delta;

                if (IsVerticeTooBig(newVertice))
                {
                    return;
                }

                bool closeVertexFound = false;
                foreach (Vector3 otherVertex in otherVertices)
                {
                    Vector3 diffVector = otherVertex - newVertice;
                    if (Math.Abs(diffVector.X) < Math.Abs(delta) && Math.Abs(diffVector.Y) < Math.Abs(delta) && Math.Abs(diffVector.Z) < Math.Abs(delta))
                    {
                        closeVertexFound = true;
                        break;
                    }
                }

                areVerticesTooClose = closeVertexFound;

                oldNewVerticeMap[transformedVertex] = newVertice;
                newVertices.Add(newVertice);
            }

            if (areVerticesTooClose)
            {
                return;
            }

            foreach (Vector3 otherVertex in otherVertices)
            {
                oldNewVerticeMap[otherVertex] = otherVertex;
            }

            newVertices.AddRange(otherVertices);

            UpdatePlanes(oldNewVerticeMap);

            if (ChildCuboids != null)
            {
                LoadChildCuboids(ChildCuboids, forceScale: true);
            }

            if (reselect)
            {
                DetectSelectedPlane(ray);
            }
        }

        private bool AreVerticesTooBig(IEnumerable<Vector3> vertices)
        {
            if (EditedModel == null)
            {
                return false;
            }

            foreach (var vector3 in vertices)
            {
                bool result = IsVerticeTooBig(vector3);
                if (result)
                {
                    return result;
                }
            }
            return false;
        }

        private bool IsVerticeTooBig(Vector3 newVertice)
        {
            return Math.Abs(newVertice.X) > (float)EditedModel.LengthInBlockCells / 2 ||
                Math.Abs(newVertice.Y) > (float)EditedModel.HeightInBlockCells / 2 ||
                Math.Abs(newVertice.Z) > (float)EditedModel.WidthInBlockCells / 2;
        }

        public void SetTextureToSelectedSide(Texture texture)
        {
            _selectedPlane.ResetTexture(texture);
        }

        public List<Plane> GetUnmodifiedPlanes()
        {
            var result = new List<Plane>();

            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                // This should be done on compiling instead
                //                if (planePrimitive.Value.Texture == null)
                //                {
                //                    continue;
                //                }

                var unmodifiedVertices = new List<Vector3>();
                foreach (Vector3 vertice in planePrimitive.Value.GetVerticesPositions())
                {
                    Matrix assBackwardsRotation = Matrix.CreateFromYawPitchRoll(-Yaw, -Pitch, -Roll);
                    Vector3 unmodifiedVertice = Vector3.Transform((vertice - Position), assBackwardsRotation);
                    unmodifiedVertices.Add(unmodifiedVertice);
                }
                var plane = new Plane(unmodifiedVertices, planePrimitive.Value.TextureName);
                result.Add(plane);
            }

            return result;
        }

        public Vector3 GetSelectedNormal()
        {
            if (_selectedPlane != null)
            {
                return _selectedPlane.Normal;
            }

            return new Vector3();
        }

        public Vector3 GetSelectedCenter()
        {
            if (_selectedPlane != null)
            {
                List<Vector3> positions = _selectedPlane.GetVerticesPositions();

                Vector3 center = (positions[0] + positions[1] + positions[2] + positions[3]) / 4;

                return center;
            }

            return new Vector3();
        }

        public bool CheckIfMinimallyTextured()
        {
            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                bool result = planePrimitive.Value.Texture != null;

                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckIfFullyTextured()
        {
            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                bool result = planePrimitive.Value.Texture != null;

                if (!result)
                {
                    return false;
                }
            }

            return true;
        }

        public void RotateSelectedTexture()
        {
            _selectedPlane.RotateTexture();
        }

        public CuboidPrimitive Clone()
        {
            var newPlanes = new List<PlanePrimitive>();

            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                newPlanes.Add(planePrimitive.Value);
            }

            var result = new CuboidPrimitive(_graphicsDevice, 0, 0, 0, Vector3.Zero, newPlanes, EditedModel, IsItem);

            if (ChildCuboids != null)
            {
                var newChildren = new List<CuboidPrimitive>();
                foreach (CuboidPrimitive childCuboid in ChildCuboids)
                {
                    newChildren.Add(childCuboid);
                }

                result.LoadChildCuboids(newChildren);
            }

            return result;
        }

        public void LoadChildCuboids(List<CuboidPrimitive> cuboids, bool forceScale = false)
        {
            RecalculateMinMaxVertices();

            ChildCuboids = new List<CuboidPrimitive>();

            // If cuboids are bigger than parent in size, we need to scale them.
            // If they are lesser, we need to scale the container cuboid
            Vector3 childMaxPoint = Vector3.Zero;
            Vector3 childMinPoint = Vector3.Zero;
            bool isFirst = true;

            foreach (CuboidPrimitive cuboidPrimitive in cuboids)
            {
                // just in case, if commented, then isn't needed
                // cuboidPrimitive.RecalculateMinMaxVertices();

                if (isFirst)
                {
                    childMaxPoint = cuboidPrimitive.MaxPoint;
                    childMinPoint = cuboidPrimitive.MinPoint;
                    isFirst = false;
                }

                childMaxPoint = Vector3.Max(childMaxPoint, cuboidPrimitive.MaxPoint);
                childMinPoint = Vector3.Min(childMinPoint, cuboidPrimitive.MinPoint);
            }

            Vector3 childDiameter = childMaxPoint - childMinPoint;

            Vector3 diameter = MaxPoint - MinPoint;

            if (Vector3.Min(childDiameter, diameter) == diameter || forceScale)
            {
                Vector3 scale = diameter / childDiameter;
                Matrix scaleMatrix = Matrix.CreateScale(scale);

                foreach (var cuboidPrimitive in cuboids)
                {
                    CuboidPrimitive newCuboid = cuboidPrimitive.Clone();
                    newCuboid.EditedModel = EditedModel;

                    newCuboid.Scale(scaleMatrix);
                    ChildCuboids.Add(newCuboid);
                }
            }
            else
            {
                Vector3 scale = childDiameter / diameter;
                Matrix scaleMatrix = Matrix.CreateScale(scale);

                Scale(scaleMatrix);

                foreach (var cuboidPrimitive in cuboids)
                {
                    CuboidPrimitive newCuboid = cuboidPrimitive.Clone();
                    newCuboid.EditedModel = EditedModel;

                    ChildCuboids.Add(newCuboid);
                }
            }

            TranslateChildCuboids();
        }

        private void TranslateChildCuboids()
        {
            Vector3 childMaxPoint = Vector3.Zero;
            Vector3 childMinPoint = Vector3.Zero;
            bool isFirst = true;

            foreach (CuboidPrimitive cuboidPrimitive in ChildCuboids)
            {
                if (isFirst)
                {
                    childMaxPoint = cuboidPrimitive.MaxPoint;
                    childMinPoint = cuboidPrimitive.MinPoint;
                    isFirst = false;
                }

                childMaxPoint = Vector3.Max(childMaxPoint, cuboidPrimitive.MaxPoint);
                childMinPoint = Vector3.Min(childMinPoint, cuboidPrimitive.MinPoint);
            }

            //            Vector3 childDiameter = childMaxPoint - childMinPoint;

            Vector3 childCenter = (childMaxPoint + childMinPoint) / 2;

            //            Vector3 diff = Position - childCenter;

            Matrix translation = Matrix.CreateTranslation(-childCenter) * Matrix.CreateTranslation(Position);

            foreach (CuboidPrimitive cuboidPrimitive in ChildCuboids)
            {
                cuboidPrimitive.Translate(translation);
            }
        }

        public bool Intersects(CuboidPrimitive cuboid)
        {
            foreach (KeyValuePair<int, PlanePrimitive> plane in cuboid._planes)
            {
                foreach (KeyValuePair<int, PlanePrimitive> localPlane in _planes)
                {
                    if (plane.Value.Intersects(localPlane.Value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            foreach (KeyValuePair<int, PlanePrimitive> planePrimitive in _planes)
            {
                planePrimitive.Value.Dispose();
            }
            if (ChildCuboids != null)
            {
                foreach (CuboidPrimitive cuboidPrimitive in ChildCuboids)
                {
                    cuboidPrimitive.Dispose(disposing);
                }
            }
        }
    }
}