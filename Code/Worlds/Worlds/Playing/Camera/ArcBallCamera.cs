using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Editors;

namespace WorldsGame.Camera
{
    internal class ArcBallCamera : Camera
    {
        internal static readonly Vector3 DEFAULT_POSITION = new Vector3(0, 32, 0);

        private float _leftRightRotation;
        private float _upDownRotation;
        private int _zoom;

        private Quaternion _rotation = Quaternion.Identity;

        private readonly Vector3 _cameraFinalTarget;
        private readonly ModelEditorActionManager _actionManager;

        internal Vector3 HorizontalPerpendicularLine { get; private set; }

        internal ArcBallCamera(Viewport viewport, ModelEditorActionManager actionManager)
            : base(viewport)
        {
            _actionManager = actionManager;

            _cameraFinalTarget = new Vector3(0, 0, 0);
        }

        internal override void Initialize()
        {
            base.Initialize();

            _leftRightRotation = MathHelper.PiOver4;
            _upDownRotation = MathHelper.Pi / 3;
            CalculateView();
            Subscribe();

            ScrollWheelDeltaChanged(-2);
        }

        private void Subscribe()
        {
            if (_actionManager != null)
            {
                _actionManager.OnRotateCamera += RotateCamera;
                _actionManager.OnScrollWheelDeltaChanged += ScrollWheelDeltaChanged;
            }
        }

        private void ScrollWheelDeltaChanged(int deltaTicks)
        {
            _zoom += deltaTicks;

            CalculateView();
        }

        private void RotateCamera(float rotationDeltaY, float rotationDeltaX)
        {
            _leftRightRotation += MathHelper.ToRadians(rotationDeltaY) * 4;
            _upDownRotation += MathHelper.ToRadians(rotationDeltaX) * 4;
            _upDownRotation = MathHelper.Clamp(_upDownRotation, 0.01f, MathHelper.Pi * 0.99f);

            CalculateView();
        }

        internal float LeftRightRotation
        {
            get { return _leftRightRotation; }
            set
            {
                _leftRightRotation = value;
                CalculateView();
            }
        }

        internal float UpDownRotation
        {
            get { return _upDownRotation; }
            set
            {
                _upDownRotation = value;
                CalculateView();
            }
        }

        protected override void CalculateView()
        {
            Vector3 cameraPosition = DEFAULT_POSITION;

            cameraPosition = CalculateZoom(cameraPosition);

            Quaternion additionalRotation = Quaternion.CreateFromYawPitchRoll(_leftRightRotation, _upDownRotation, 0f);

            _rotation *= additionalRotation;

            cameraPosition = Vector3.Transform(cameraPosition, additionalRotation);

            View = Matrix.CreateLookAt(cameraPosition, _cameraFinalTarget, Vector3.Up);

            base.CalculateView();
        }

        private Vector3 CalculateZoom(Vector3 cameraPosition)
        {
            // This all should work because direction vector == default position
            if (_zoom != 0)
            {
                bool isZoomedForward = _zoom > 0;

                for (int i = 0; i <= Math.Abs(_zoom); i++)
                {
                    if (isZoomedForward)
                    {
                        // forward means that distance to target is decreasing
                        cameraPosition -= cameraPosition / 5;
                    }
                    else
                    {
                        cameraPosition += cameraPosition / 5;
                    }
                }
            }
            return cameraPosition;
        }

        private void CalculatePerpendicularLine(Vector3 cameraPosition)
        {
            Vector3 cameraPositionNormal = Vector3.Normalize(cameraPosition);
            Vector3 rotatedUp = Vector3.Transform(Vector3.Up, _rotation);
            HorizontalPerpendicularLine = Vector3.Cross(rotatedUp, cameraPositionNormal);
        }

        internal override void Update(GameTime gameTime)
        {
            CalculateView();
            base.Update(gameTime);
        }

        private void Unsubscribe()
        {
            if (_actionManager != null)
            {
                _actionManager.OnRotateCamera -= RotateCamera;
                _actionManager.OnScrollWheelDeltaChanged -= ScrollWheelDeltaChanged;
            }
        }

        public override void Dispose()
        {
            Unsubscribe();
        }
    }
}