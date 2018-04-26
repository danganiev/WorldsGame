using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Editors;
using WorldsGame.Playing.Players;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Camera
{
    internal class ThirdPersonCamera : Camera
    {
        private int _zoom;

        private readonly Player _player;

        private Quaternion _rotation = Quaternion.Identity;

        internal float LeftRightRotation
        {
            get { return _player.LeftRightRotation; }
        }

        internal float UpDownRotation
        {
            get { return _player.UpDownRotation; }
        }

        internal Vector3 Position
        {
            get
            {
                return _player.DrawPosition;
            }
        }

        internal Vector3 LookVector { get; private set; }

        internal ThirdPersonCamera(Viewport viewport, Player player)
            : base(viewport)
        {
            _player = player;
        }

        internal override void Initialize()
        {
            base.Initialize();

            CalculateView();
        }

        private void ZoomChanged(int diff)
        {
            _zoom += diff;

            CalculateView();
        }

        protected override void CalculateView()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
            LookVector = Vector3.Transform(Vector3.Forward, rotationMatrix);

            Vector3 cameraFinalTarget = Position + new Vector3(0, _player.FaceHeight, 0) + LookVector;

            Vector3 cameraRotatedUpVector = Vector3.Transform(Vector3.Up, rotationMatrix);
            Vector3 cameraPosition = Position - LookVector * 10;

            if (cameraPosition.Y < Position.Y)
            {
                cameraPosition.Y = Position.Y;
            }

            View = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);

            base.CalculateView();
        }

        private Vector3 CalculateZoom(Vector3 cameraPosition)
        {
            // This all should work because direction vector == default position
            if (_zoom != 0 && _zoom < 20)
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

        internal override void Update(GameTime gameTime)
        {
            CalculateView();
            base.Update(gameTime);
        }
    }
}