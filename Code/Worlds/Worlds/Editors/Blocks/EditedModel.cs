using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Camera;
using WorldsGame.Gamestates;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils;
using WorldsGame.Utils.GeometricPrimitives;
using WorldsGame.Utils.Input;
using Plane = WorldsGame.Saving.Plane;
using Texture = WorldsGame.Saving.Texture;

namespace WorldsGame.Editors.Blocks
{
    /// <summary>
    /// Model being edited in the model editor (could be character, block or item)
    /// </summary>
    public class EditedModel : IDisposable
    {
        public const int BLOCK_SIZE = 32;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly Camera.Camera _camera;

        private readonly BlockEditorState _blockEditorState;
        private readonly CharacterEditorState _characterEditorState;
        private readonly ItemEditorState _itemEditorState;

        // _isItem is applied to first cuboid only, you can add normal cuboids to the model later,
        private readonly bool _isItem;

        private CuboidPrimitive _intersectedCuboid;
        private CuboidPrimitive _selectedCuboid;
        private bool _changingStarted;
        private Vector3 _itemPosition;
        private Vector3 _itemSize;

        internal List<CuboidPrimitive> CuboidPrimitives { get; private set; }

        internal List<Cuboid> OriginalNonanimatedCuboids { get; private set; }

        private List<CuboidPrimitive> _originalNonanimatedCuboidPrimitives;
        private float _currentFrameTime;
        private int _currentFrame;
        //        private List<ComputedKeyframe> _currentKeyframes;

        internal int LengthInBlocks { get; set; }

        internal int LengthInBlockCells { get { return LengthInBlocks * BLOCK_SIZE; } }

        internal int HeightInBlocks { get; set; }

        internal int HeightInBlockCells { get { return HeightInBlocks * BLOCK_SIZE; } }

        internal int WidthInBlocks { get; set; }

        internal int WidthInBlockCells { get { return WidthInBlocks * BLOCK_SIZE; } }

        internal bool IsFullBlock { get; set; }

        internal bool IsLiquid { get; set; }

        internal Color[] IconTextureColors { get; set; }

        internal bool IsSideSelected { get { return _selectedCuboid != null; } }

        internal bool IsBeingAnimated { get; set; }

        internal ComputedAnimation CurrentAnimation { get; set; }

        internal bool IsBeingTransformedForAnimation { get; set; }

        internal bool IsSelectable { get; set; }

        internal int CuboidCount { get { return CuboidPrimitives.Count; } }

        //        internal EditedModel ChildModel { get; set; }

        // Used to stop events when we have both character and item models subscribed
        // internal EditedModel RelatedModel { get; set; }

        internal List<int> AddedCuboids { get; private set; }

        internal List<int> RemovedCuboids { get; private set; }

        private CuboidPrimitive _itemCuboid;

        internal CuboidPrimitive ItemCuboid
        {
            get { return _itemCuboid; }
            set
            {
                if (_itemCuboid != null)
                {
                    CuboidPrimitives.Remove(_itemCuboid);
                }

                _itemCuboid = value;
                CuboidPrimitives.Add(_itemCuboid);
            }
        }

        internal bool ChangingStarted
        {
            get { return _changingStarted; }
        }

        internal event Action<CuboidPrimitive> OnSideSelected = cuboid => { };

        internal event Action OnSideDeselected = () => { };

        internal event Action OnAnimationStop = () => { };

        private EditedModel(GraphicsDevice graphicsDevice, Camera.Camera camera)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            CuboidPrimitives = new List<CuboidPrimitive>();
            IsSelectable = true;
            _itemSize = new Vector3(2, 2, 2);
            _originalNonanimatedCuboidPrimitives = new List<CuboidPrimitive>();
            //            _currentKeyframes = new List<ComputedKeyframe>();
            AddedCuboids = new List<int>();
            RemovedCuboids = new List<int>();
        }

        internal EditedModel(
            GraphicsDevice graphicsDevice, Camera.Camera camera,
            BlockEditorState blockEditorState, bool isFullBlock = false, Block blockSave = null, bool isLiquid = false)
            : this(graphicsDevice, camera)
        {
            _blockEditorState = blockEditorState;
            IsFullBlock = blockSave == null ? isFullBlock : blockSave.IsFullBlock;
            LengthInBlocks = blockSave == null ? 1 : blockSave.LengthInBlocks;
            HeightInBlocks = blockSave == null ? 1 : blockSave.HeightInBlocks;
            WidthInBlocks = blockSave == null ? 1 : blockSave.WidthInBlocks;

            IsLiquid = isLiquid;

            New();
        }

        internal EditedModel(
            GraphicsDevice graphicsDevice, Camera.Camera camera, CharacterEditorState characterEditorState,
            bool isItem = false, Character characterSave = null, Vector3 size = new Vector3(), Vector3 position = new Vector3())
            : this(graphicsDevice, camera)
        {
            _characterEditorState = characterEditorState;
            _isItem = isItem;

            if (_isItem)
            {
                _itemPosition = position;
                _itemSize = size;
            }

            IsFullBlock = false;
            LengthInBlocks = characterSave == null ? 2 : characterSave.LengthInBlocks;
            HeightInBlocks = characterSave == null ? 2 : characterSave.HeightInBlocks;
            WidthInBlocks = characterSave == null ? 2 : characterSave.WidthInBlocks;
            New();
        }

        internal EditedModel(
            GraphicsDevice graphicsDevice, Camera.Camera camera,
            ItemEditorState itemEditorState, Item itemSave = null, bool isFirstPersonItem = false,
            Vector3 size = new Vector3(), Vector3 position = new Vector3())
            : this(graphicsDevice, camera)
        {
            _isItem = isFirstPersonItem;
            _itemEditorState = itemEditorState;
            IsFullBlock = false;

            if (!_isItem)
            {
                LengthInBlocks = itemSave == null ? 1 : itemSave.LengthInBlocks;
                HeightInBlocks = itemSave == null ? 1 : itemSave.HeightInBlocks;
                WidthInBlocks = itemSave == null ? 1 : itemSave.WidthInBlocks;
            }
            else
            {
                LengthInBlocks = 2;
                HeightInBlocks = 2;
                WidthInBlocks = 2;
                _itemSize = size;
                _itemPosition = position;
            }

            New();
        }

        internal void Draw(Matrix view, Matrix projection, AlphaTestEffect textureEffect)
        {
            if (IsBeingAnimated)
            {
                for (int index = 0; index < CuboidPrimitives.Count; index++)
                {
                    CuboidPrimitive cuboidPrimitive = CuboidPrimitives[index];
                    Matrix world = GetAnimationWorldMatrix(index);
                    cuboidPrimitive.Draw(world, view, projection, textureEffect);
                }
            }
            else
            {
                foreach (CuboidPrimitive cuboidPrimitive in CuboidPrimitives)
                {
                    cuboidPrimitive.Draw(view, projection, textureEffect);
                }
            }
        }

        internal void UpdateAnimation(GameTime gameTime)
        {
            _currentFrameTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            _currentFrame = (int)_currentFrameTime / Constants.MILLISECONDS_PER_KEYFRAME;
            Debug.WriteLine(_currentFrame);

            if (_currentFrame >= CurrentAnimation.KeyframeCount)
            {
                StopAnimation();
            }
        }

        private Matrix GetAnimationWorldMatrix(int cuboidIndex)
        {
            return CurrentAnimation.KeyframesPerCuboids[cuboidIndex][_currentFrame].Rotation *
                   CurrentAnimation.KeyframesPerCuboids[cuboidIndex][_currentFrame].Scale *
                   CurrentAnimation.KeyframesPerCuboids[cuboidIndex][_currentFrame].Translation;
        }

        private void StopAnimation()
        {
            _currentFrame = 0;
            _currentFrameTime = 0;
            IsBeingAnimated = false;
            OnAnimationStop();
        }

        internal void Update(GameTime gameTime)
        {
        }

        internal bool IsIntersecting(Ray ray, bool isRelated = false)
        {
            float? distance = null;
            float? minDistance = null;
            foreach (CuboidPrimitive cuboidPrimitive in CuboidPrimitives)
            {
                distance = ray.Intersects(cuboidPrimitive.GetBoundingBox());
                if (distance != null)
                {
                    if (minDistance == null || distance < minDistance)
                    {
                        DeselectEverything();

                        minDistance = distance;

                        _intersectedCuboid = cuboidPrimitive;
                        _intersectedCuboid.DetectSelectedPlane(ray);
                    }
                }
            }

            if (minDistance == null)
            {
                _intersectedCuboid = null;
            }

            _selectedCuboid = _isItem ? _itemCuboid : _intersectedCuboid;

            if (_intersectedCuboid == null)
            {
                return false;
            }

            return true;
        }

        internal void OnSelect(Cursor cursor)
        {
            if (ShouldNotSelect(cursor) || ChangingStarted)
            {
                return;
            }

            DeselectEverything(fireEvent: true);

            if (IsIntersecting(cursor.CalculateCursorRay(_camera.Projection, _camera.View)))
            {
                _selectedCuboid = _isItem ? _itemCuboid : _intersectedCuboid;

                OnSideSelected(_selectedCuboid);
            }
            else
            {
                _selectedCuboid = null;
            }
        }

        private bool ShouldNotSelect(Cursor cursor)
        {
            return !IsSelectable || cursor.Position.Y < 100 || (_blockEditorState != null && _blockEditorState.IsMenuOpenOrPaused) ||
                (_characterEditorState != null && _characterEditorState.IsMenuOpenOrPaused) || (_itemEditorState != null && _itemEditorState.IsMenuOpenOrPaused)/* || (RelatedModel != null && RelatedModel.ChangingStarted)*/;
        }

        internal void DeselectEverything(bool fireEvent = false)
        {
            foreach (CuboidPrimitive cuboidPrimitive in CuboidPrimitives)
            {
                cuboidPrimitive.DeselectEverything();
            }

            _selectedCuboid = null;

            if (fireEvent && !IsSideSelected && OnSideDeselected != null)
            {
                OnSideDeselected();
            }
        }

        internal void OnTransform(Cursor cursor, float deltaX, float deltaY)
        {
            if (ShouldNotSelect(cursor))
            {
                return;
            }

            if (!ChangingStarted)
            {
                OnSelect(cursor);
            }

            if (IsFullBlock || IsLiquid)
            {
                return;
            }

            Ray ray = cursor.CalculateCursorRay(_camera.Projection, _camera.View);
            if (ChangingStarted || IsIntersecting(ray))
            {
                _changingStarted = true;
                float delta = GetBiggerDelta(deltaX, deltaY);
                TransformSelectedCuboidSide(delta, ray);
            }
        }

        internal void OnScale(float delta)
        {
            if (IsFullBlock || IsLiquid)
            {
                return;
            }

            if (_selectedCuboid != null)
            {
                _changingStarted = true;
                _selectedCuboid.Scale(Matrix.CreateScale(delta));
            }
        }

        internal void OnRotate(Cursor cursor, float deltaX, float deltaY, Vector3 normal)
        {
            if (ShouldNotSelect(cursor) || IsFullBlock || IsLiquid)
            {
                return;
            }

            if (!ChangingStarted)
            {
                OnSelect(cursor);
            }

            if (ChangingStarted || IsIntersecting(cursor.CalculateCursorRay(_camera.Projection, _camera.View)))
            {
                _changingStarted = true;
                float delta = GetBiggerDelta(deltaX, deltaY);
                if (delta != 0)
                {
                    RotateSelectedCuboid(delta, normal);
                }
            }
        }

        internal void OnRotateX(Cursor cursor, float deltaX, float deltaY)
        {
            OnRotate(cursor, deltaX, deltaY, Vector3.UnitX);
        }

        internal void OnRotateY(Cursor cursor, float deltaX, float deltaY)
        {
            OnRotate(cursor, deltaX, deltaY, Vector3.UnitY);
        }

        internal void OnRotateZ(Cursor cursor, float deltaX, float deltaY)
        {
            OnRotate(cursor, deltaX, deltaY, Vector3.UnitZ);
        }

        internal void OnTranslate(Cursor cursor, float deltaX, float deltaY, Vector3 normal)
        {
            if (ShouldNotSelect(cursor) || IsFullBlock || IsLiquid)
            {
                return;
            }

            if (!ChangingStarted)
            {
                OnSelect(cursor);
            }

            if (ChangingStarted || IsIntersecting(cursor.CalculateCursorRay(_camera.Projection, _camera.View)))
            {
                _changingStarted = true;
                float delta = GetBiggerDelta(deltaX, deltaY);

                if (delta != 0)
                {
                    TranslateSelectedCuboid(delta, normal);
                }
            }
        }

        internal void OnTranslateX(Cursor cursor, float deltaX, float deltaY)
        {
            OnTranslate(cursor, deltaX, deltaY, Vector3.UnitX);
        }

        internal void OnTranslateY(Cursor cursor, float deltaX, float deltaY)
        {
            OnTranslate(cursor, deltaX, deltaY, Vector3.UnitY);
        }

        internal void OnTranslateZ(Cursor cursor, float deltaX, float deltaY)
        {
            OnTranslate(cursor, deltaX, deltaY, Vector3.UnitZ);
        }

        private float GetBiggerDelta(float deltaX, float deltaY)
        {
            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                return deltaX;
            }
            else
            {
                return deltaY;
            }
        }

        private void RotateSelectedCuboid(float delta, Vector3 normal)
        {
            if (_selectedCuboid != null)
            {
                _selectedCuboid.Rotate(delta, normal);
            }
        }

        private void TransformSelectedCuboidSide(float delta, Ray ray)
        {
            if (_selectedCuboid != null)
            {
                _selectedCuboid.TransformSelectedSide(delta, ray, !ChangingStarted);
            }
        }

        private void TranslateSelectedCuboid(float delta, Vector3 normal)
        {
            if (_selectedCuboid != null)
            {
                _selectedCuboid.Translate(delta, normal);
            }
        }

        internal void StopChanging()
        {
            _changingStarted = false;
        }

        internal void SetTextureToSelectedSide(Texture texture)
        {
            _selectedCuboid.SetTextureToSelectedSide(texture);
        }

        internal List<Cuboid> GetCuboidsForSaving()
        {
            var result = new List<Cuboid>();

            foreach (CuboidPrimitive cuboidPrimitive in CuboidPrimitives)
            {
                List<Plane> planes = cuboidPrimitive.GetUnmodifiedPlanes();

                var cuboid = new Cuboid(
                    planes, cuboidPrimitive.Position,
                    cuboidPrimitive.Yaw, cuboidPrimitive.Pitch, cuboidPrimitive.Roll);

                cuboid.MaxPoint = cuboidPrimitive.MaxPoint;
                cuboid.MinPoint = cuboidPrimitive.MinPoint;

                cuboid.IsItem = cuboidPrimitive.IsItem;

                result.Add(cuboid);
            }

            return result;
        }

        internal Tuple<Vector3, Vector3> GetMinMaxVertices()
        {
            var minVertice = Vector3.Zero;
            var maxVertice = Vector3.Zero;

            foreach (Cuboid cuboid in GetCuboidsForSaving())
            {
                foreach (Plane plane in cuboid.Planes)
                {
                    foreach (var vertice in plane.Vertices)
                    {
                        minVertice = Vector3.Min(minVertice, vertice);
                        maxVertice = Vector3.Max(maxVertice, vertice);
                    }
                }
            }

            return new Tuple<Vector3, Vector3>(minVertice, maxVertice);
        }

        internal bool CheckIfMinimallyTextured()
        {
            foreach (CuboidPrimitive cuboidPrimitive in CuboidPrimitives)
            {
                bool result = cuboidPrimitive.CheckIfMinimallyTextured();

                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool CheckIfFullyTextured()
        {
            foreach (CuboidPrimitive cuboidPrimitive in CuboidPrimitives)
            {
                bool result = cuboidPrimitive.CheckIfFullyTextured();

                if (!result)
                {
                    return false;
                }
            }

            return true;
        }

        internal void New()
        {
            CuboidPrimitive cuboid;

            Clear();

            if (IsFullBlock)
            {
                cuboid = new CuboidPrimitive(_graphicsDevice, new Vector3(32, 32, 32), new Vector3(0, 0, 0), this);
                CuboidPrimitives.Add(cuboid);
            }
            else if (IsLiquid)
            {
                cuboid = new CuboidPrimitive(
                    _graphicsDevice, 0, 0, 0, new Vector3(0), 
                    new List<PlanePrimitive>
                        {
                            new PlanePrimitive(_graphicsDevice, 32, 32, new Vector3(0, 1, 0))
                        },
                    this);
                CuboidPrimitives.Add(cuboid);
                
            }
            else
            {
                if (_isItem)
                {
                    cuboid = new CuboidPrimitive(_graphicsDevice, _itemSize, _itemPosition, editedModel: this, isItem: _isItem);
                    _itemCuboid = cuboid;
                }
                else
                {
                    cuboid = new CuboidPrimitive(_graphicsDevice, new Vector3(16, 16, 16), new Vector3(0, 0, 0), editedModel: this);
                }

                CuboidPrimitives.Add(cuboid);
            }
        }

        internal void AddNewCuboid()
        {
            if (!IsFullBlock && !IsLiquid)
            {
                AddedCuboids.Add(CuboidPrimitives.Count - 1);

                var cuboid = new CuboidPrimitive(_graphicsDevice, new Vector3(16, 16, 16), new Vector3(0, 0, 0), this);
                CuboidPrimitives.Add(cuboid);
            }
        }

        internal void RemoveSelectedCuboid()
        {
            if (IsSideSelected && !IsFullBlock && !IsLiquid)
            {
                int selectedIndex = CuboidPrimitives.IndexOf(_selectedCuboid);
                AddedCuboids.Remove(selectedIndex);
                RemovedCuboids.Remove(selectedIndex);
                RemovedCuboids.Add(selectedIndex);

                CuboidPrimitives.Remove(_selectedCuboid);

                if (!_characterEditorState.IsNew)
                {
                    foreach (KeyValuePair<AnimationType, CompiledAnimation> compiledAnimation in _characterEditorState.EditedCharacter.Animations)
                    {
                        compiledAnimation.Value.Keyframes.RemoveAt(selectedIndex);
                    }
                }

                DeselectEverything(fireEvent: true);
            }
        }

        internal void CopySelectedCuboid()
        {
            if (IsSideSelected && !IsFullBlock && !IsLiquid)
            {
                AddedCuboids.Add(CuboidPrimitives.Count - 1);

                var cuboidCopy = _selectedCuboid.Clone();

                cuboidCopy.Translate(Matrix.CreateTranslation(new Vector3(0.5f)));

                CuboidPrimitives.Add(cuboidCopy);
            }
        }

        internal Vector3 GetSelectedSideNormal()
        {
            if (IsSideSelected && !IsFullBlock && !IsLiquid)
            {
                return _selectedCuboid.GetSelectedNormal();
            }

            return new Vector3(0, 0, 0);
        }

        internal Vector3 GetSelectedSideCenter()
        {
            if (IsSideSelected && !IsFullBlock && !IsLiquid)
            {
                return _selectedCuboid.GetSelectedCenter();
            }

            return new Vector3(0, 0, 0);
        }

        internal void Load(Block loadedBlock)
        {
            Clear();

            IsFullBlock = loadedBlock.IsFullBlock;
            IsLiquid = loadedBlock.IsLiquid;

            foreach (Cuboid cuboid in loadedBlock.Cuboids)
            {
                CuboidPrimitive primitive = cuboid.GetPrimitive(_graphicsDevice, loadedBlock.WorldSettingsName, this);
                CuboidPrimitives.Add(primitive);
            }
        }

        internal void Load(Character character)
        {
            Clear();

            IsFullBlock = false;
            IsLiquid = false;

            foreach (Cuboid cuboid in character.Cuboids)
            {
                CuboidPrimitive primitive = cuboid.GetPrimitive(_graphicsDevice, character.WorldSettingsName, this);
                CuboidPrimitives.Add(primitive);
            }
        }

        internal void Load(Item item)
        {
            Clear();

            IsFullBlock = false;
            IsLiquid = false;

            foreach (Cuboid cuboid in item.Cuboids)
            {
                CuboidPrimitive primitive = cuboid.GetPrimitive(_graphicsDevice, item.WorldSettingsName, this);
                CuboidPrimitives.Add(primitive);
            }

            IconTextureColors = item.IconColors;
        }

        internal void RotateSelectedTexture()
        {
            _selectedCuboid.RotateSelectedTexture();
        }

        internal void SetIconColors(Color[] colors)
        {
            IconTextureColors = colors;
        }

        internal void StoreOriginalCuboids()
        {
            OriginalNonanimatedCuboids = GetCuboidsForSaving();

            _originalNonanimatedCuboidPrimitives.Clear();

            foreach (CuboidPrimitive cuboidPrimitive in CuboidPrimitives)
            {
                _originalNonanimatedCuboidPrimitives.Add(cuboidPrimitive.Clone());
            }
        }

        internal void LoadOriginalCuboids()
        {
            Clear();
            CuboidPrimitives.AddRange(_originalNonanimatedCuboidPrimitives);
        }

        internal void LoadKeyframeCuboidsPositions(List<CuboidPrimitive> keyframeCuboids)
        {
            Clear();

            if (ItemCuboid != null)
            {
                var oldItem = ItemCuboid;
                ItemCuboid = keyframeCuboids[0];
                ItemCuboid.LoadChildCuboids(oldItem.ChildCuboids);
                oldItem.Dispose();
            }
            else
            {
                CuboidPrimitives.AddRange(keyframeCuboids);
            }
        }

        internal void Clear()
        {
            CuboidPrimitives.Clear();

            _selectedCuboid = null;
            _intersectedCuboid = null;
            _changingStarted = false;

            ClearAddedAndRemovedCuboids();
        }

        internal void ClearAddedAndRemovedCuboids()
        {
            AddedCuboids.Clear();
            RemovedCuboids.Clear();
        }

        public void Dispose()
        {
            OnSideSelected = null;
            OnSideDeselected = null;
            OnAnimationStop = null;

            foreach (CuboidPrimitive cuboidPrimitive in CuboidPrimitives)
            {
                cuboidPrimitive.Dispose();
            }
        }
    }
}