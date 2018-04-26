using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.Players;

namespace WorldsGame.Camera
{
    internal class ModelEditorFirstPersonCamera : Camera
    {
        internal float LeftRightRotation { get; set; }

        internal float UpDownRotation { get; set; }

        internal Vector3 Position { get; set; }

        internal Vector3 LookVector { get; private set; }

        // Item fps editor usage
        internal ModelEditorFirstPersonCamera(Viewport viewport, Vector3 position, float leftRightRotation, float upDownRotation)
            : base(viewport)
        {
            Position = position;
            LeftRightRotation = leftRightRotation;
            UpDownRotation = upDownRotation;
        }

        protected override void CalculateView()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
            LookVector = Vector3.Transform(Vector3.Backward, rotationMatrix);

            Vector3 cameraFinalTarget = Position + LookVector;

            Vector3 cameraRotatedUpVector = Vector3.Transform(Vector3.Up, rotationMatrix);
            View = Matrix.CreateLookAt(Position, cameraFinalTarget, cameraRotatedUpVector);

            base.CalculateView();
        }

        internal override void Update(GameTime gameTime)
        {
            CalculateView();
            base.Update(gameTime);
        }
    }
}