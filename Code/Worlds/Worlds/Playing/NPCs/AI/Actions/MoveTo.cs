using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Physics.Components;

namespace WorldsGame.Playing.NPCs.AI
{
    internal class MoveTo : AIAction
    {
        internal const float DESTINATION_DELTA_SQUARED = 4f;

        private readonly Vector3 _destination;
        private readonly AIAction _actionOnArrival;
        private Vector3 _normal;

        internal MoveTo(ActionList actionList, Vector3 destination, AIAction actionOnArrival = null)
            : base(actionList)
        {
            //            _destination = destination;
            _destination = new Vector3(-10, 0, 0);
            _actionOnArrival = actionOnArrival ?? new Roam(ownerList);
            _normal = Vector3.Forward;

            Lanes = Lanes.Lane1;
            isBlocking = true;
        }

        internal override void Update(GameTime gameTime)
        {
            bool isRotated = RotateTowardsArrivalPosition(gameTime);

            if (isRotated)
            {
                Move();
                CheckIfArrived();
            }
            else
            {
                Stop();
            }
        }

        private float _requiredAngle;

        private bool _isAngleFound;
        private float _minDistance = 1000f;
        private float _theta;

        private bool RotateTowardsArrivalPosition(GameTime gameTime)
        {
            var scaleAndRotate = entity.GetComponent<ScaleAndRotateComponent>();

            if (_isAngleFound && Math.Abs(_requiredAngle - scaleAndRotate.LeftRightRotation) < 0.1f)
            {
                return true;
            }

            if (_isAngleFound)
            {
                //                scaleAndRotate.LeftRightRotation = MathHelper.Lerp(_requiredAngle, scaleAndRotate.LeftRightRotation,
                //                                _rotationTime);
                //                if (_requiredAngle > scaleAndRotate.LeftRightRotation)
                if (_theta > 0)
                {
                    scaleAndRotate.LeftRightRotation = MathHelper.WrapAngle(scaleAndRotate.LeftRightRotation -
                                                   (float)gameTime.ElapsedGameTime.TotalSeconds * 1f);
                }
                else
                {
                    scaleAndRotate.LeftRightRotation = MathHelper.WrapAngle(scaleAndRotate.LeftRightRotation +
                                                   (float)gameTime.ElapsedGameTime.TotalSeconds * 1f);
                }

                if (Math.Abs(_requiredAngle - scaleAndRotate.LeftRightRotation) < 0.1f)
                {
                    // I have no idea why, but NPC always tries to go right the other way
                    scaleAndRotate.LeftRightRotation = _requiredAngle;

                    return true;
                }
            }
            else
            {
                //            var currentLookVector = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(scaleAndRotate.LeftRightRotation));

                var normalWithoutY = Vector3.Normalize(new Vector3(_normal.X, 0, _normal.Z));
                _requiredAngle = MathHelper.WrapAngle((float)Math.Acos(Vector3.Dot(Vector3.Forward, normalWithoutY)));
                Vector3 currentNormal = Vector3.Transform(
                    Vector3.Forward, Matrix.CreateRotationY(scaleAndRotate.LeftRightRotation));

                // http://math.stackexchange.com/questions/555198/find-direction-of-angle-between-2-vectors
                _theta = currentNormal.X * normalWithoutY.Z - currentNormal.Z * normalWithoutY.X;

                if (_theta > 0)
                {
                    _requiredAngle *= -1;
                }

                _isAngleFound = true;

                if (normalWithoutY == Vector3.Zero)
                {
                    ComputeNormal();
                }

                //            float angleDiff = (float)Math.Acos(Vector3.Dot(currentLookVector, normalWithoutY));

                //            _minAngle = MathHelper.Min(angleDiff, _minAngle);

                //            if (Math.Abs(angleDiff) < 0.1f)
                //            {
                //                return true;
                //            }

                //            scaleAndRotate.LeftRightRotation = angleDiff > 0
                //                ? scaleAndRotate.LeftRightRotation + (float)gameTime.ElapsedGameTime.TotalSeconds * 1f
                //                : scaleAndRotate.LeftRightRotation - (float)gameTime.ElapsedGameTime.TotalSeconds * 1f;
            }

            return false;
        }

        private void Move()
        {
            entity.GetComponent<PhysicsComponent>().IsMovingForward = true;
        }

        private void Stop()
        {
            entity.GetComponent<PhysicsComponent>().IsMovingForward = false;
        }

        private void CheckIfArrived()
        {
            var positionComponent = entity.GetComponent<PositionComponent>();

            var distance = Vector3.Distance(positionComponent.Position, _destination);

            _minDistance = MathHelper.Min(distance, _minDistance);

            if (distance > _minDistance)
            {
                ComputeNormal();
            }

            if (Vector3.DistanceSquared(positionComponent.Position, _destination) < DESTINATION_DELTA_SQUARED)
            {
                entity.GetComponent<PhysicsComponent>().IsMovingForward = false;
                InsertInFrontOfMe(_actionOnArrival);
                Finish();
            }
        }

        internal override void OnStart(Entity entity)
        {
            base.OnStart(entity);

            ComputeNormal();
        }

        private void ComputeNormal()
        {
            var positionComponent = entity.GetComponent<PositionComponent>();

            _normal = Vector3.Normalize(_destination - positionComponent.Position);
            //            _minAngle = 10f;
            //            _minDistance = 1000f;
            _isAngleFound = false;
        }
    }
}