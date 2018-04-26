#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

#endregion License

using System;

using Microsoft.Xna.Framework;

using WorldsGame.Players.MovementBehaviours;
using WorldsGame.Playing.Physics.Behaviours;
using WorldsGame.Playing.Physics.Components;
using WorldsGame.Playing.Players;
using WorldsGame.Playing.Players.Entity;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Players
{
    internal class PlayerPhysics : IDisposable
    {
        private readonly Player _player;

        private ClientPlayerActionManager _playerActionManager;

        internal ClientPlayerActionManager PlayerActionManager
        {
            get { return _playerActionManager; }
            set
            {
                _playerActionManager = value;

                if (value != null)
                {
                    value.OnPlayerStartedMovingForward += OnPlayerStartedMovingForward;
                    value.OnPlayerStartedMovingBack += OnPlayerStartedMovingBack;
                    value.OnPlayerStartedStrafingLeft += OnPlayerStartedStrafingLeft;
                    value.OnPlayerStartedStrafingRight += OnPlayerStartedStrafingRight;

                    value.OnPlayerStoppedMovingForward += OnPlayerStoppedMovingForward;
                    value.OnPlayerStoppedMovingBack += OnPlayerStoppedMovingBack;
                    value.OnPlayerStoppedStrafingLeft += OnPlayerStoppedStrafingLeft;
                    value.OnPlayerStoppedStrafingRight += OnPlayerStoppedStrafingRight;

                    value.OnPlayerJumped += OnJump;
                }
            }
        }

        private void OnJump()
        {
            Jump();
        }

        private void OnPlayerStoppedStrafingRight()
        {
            _player.PlayerEntity.GetComponent<PhysicsComponent>().IsStrafingRight = false;
        }

        private void OnPlayerStoppedStrafingLeft()
        {
            _player.PlayerEntity.GetComponent<PhysicsComponent>().IsStrafingLeft = false;
        }

        private void OnPlayerStoppedMovingBack()
        {
            _player.PlayerEntity.GetComponent<PhysicsComponent>().IsMovingBackward = false;
        }

        private void OnPlayerStoppedMovingForward()
        {
            _player.PlayerEntity.GetComponent<PhysicsComponent>().IsMovingForward = false;
        }

        private void OnPlayerStartedStrafingRight()
        {
            _player.PlayerEntity.GetComponent<PhysicsComponent>().IsStrafingRight = true;
        }

        private void OnPlayerStartedStrafingLeft()
        {
            _player.PlayerEntity.GetComponent<PhysicsComponent>().IsStrafingLeft = true;
        }

        private void OnPlayerStartedMovingBack()
        {
            _player.PlayerEntity.GetComponent<PhysicsComponent>().IsMovingBackward = true;
        }

        private void OnPlayerStartedMovingForward()
        {
            _player.PlayerEntity.GetComponent<PhysicsComponent>().IsMovingForward = true;
        }

        internal PlayerPhysics(Player player)
        {
            _player = player;
        }

        //        internal void Update(GameTime gameTime)
        //        {
        //            MovementBehaviour.Update(gameTime);
        //        }

        internal void Update(float totalSeconds, NetworkPlayerDescription playerDescription)
        {
            //            MovementBehaviour.Update(totalSeconds, playerDescription);
        }

        internal void ToggleMovementBehaviourChange()
        {
            if (_player.IsFlying)
            {
                //                MovementBehaviour = _flyBehaviour;
            }
            else
            {
                //                MovementBehaviour = _walkBehaviour;
            }
        }

        internal void Jump(bool quiet = false)
        {
            var movementBehaviour = (IMovementBehaviour)_player.PlayerEntity.World.BehaviourManager.Get(
                _player.PlayerEntity.GetComponent<PlayerComponent>().MovementBehaviourType);

            movementBehaviour.Jump(_player.PlayerEntity, quiet: quiet);
        }

        private void Unsubscribe()
        {
            if (PlayerActionManager != null)
            {
                PlayerActionManager.OnPlayerStartedMovingForward -= OnPlayerStartedMovingForward;
                PlayerActionManager.OnPlayerStartedMovingBack -= OnPlayerStartedMovingBack;
                PlayerActionManager.OnPlayerStartedStrafingLeft -= OnPlayerStartedStrafingLeft;
                PlayerActionManager.OnPlayerStartedStrafingRight -= OnPlayerStartedStrafingRight;

                PlayerActionManager.OnPlayerStoppedMovingForward -= OnPlayerStoppedMovingForward;
                PlayerActionManager.OnPlayerStoppedMovingBack -= OnPlayerStoppedMovingBack;
                PlayerActionManager.OnPlayerStoppedStrafingLeft -= OnPlayerStoppedStrafingLeft;
                PlayerActionManager.OnPlayerStoppedStrafingRight -= OnPlayerStoppedStrafingRight;

                PlayerActionManager.OnPlayerJumped -= OnJump;
            }
        }

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}