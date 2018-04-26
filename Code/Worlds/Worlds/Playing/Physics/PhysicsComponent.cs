using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;

namespace WorldsGame.Playing.Physics.Components
{
    internal class PhysicsComponent : IEntityComponent
    {
        internal float YVelocity
        {
            get
            {
                return Velocity.Y;
            }
            set
            {
                Velocity = new Vector3(Velocity.X, value, Velocity.Z);
            }
        }

        internal bool IsMovingForward { get; set; }

        internal bool IsMovingBackward { get; set; }

        internal bool IsStrafingLeft { get; set; }

        internal bool IsStrafingRight { get; set; }

        internal bool IsMoving { get { return IsMovingForward || IsMovingBackward || IsStrafingLeft || IsStrafingRight; } }

        //        internal Vector3 Acceleration { get; set; }

        internal Vector3 Velocity { get; set; }

        internal Vector3 VerticalVelocityAsVector3
        {
            get { return new Vector3(0, Velocity.Y, 0); }
        }

        internal PhysicsComponent()
        {
            IsMovingForward = false;
            IsMovingBackward = false;
            IsStrafingLeft = false;
            IsStrafingRight = false;
            Velocity = Vector3.Zero;
        }

        public void Dispose()
        {
        }
    }
}