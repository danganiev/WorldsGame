using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Network.Message;

namespace WorldsGame.Playing.Players
{
    // This is NOT the best way to do it. The best way is to adjust velocity and also do some prediction/extrapolation.
    // But it should do for now
    internal class OtherPlayersMovementProcessor
    {
        private const int INTERPOLATION_FRAMES = 3;

        //        private const float INTERPOLATION_COEFFICIENT = 1f/INTERPOLATION_FRAMES;
        private readonly ClientPlayerManager _playerManager;

        // key is the player slot here
        // these two are for prediction/extrapolation, not now.
        //        private readonly Dictionary<byte, ServerNetworkPlayerDescription> _preLatestDescriptions;
        //        private readonly Dictionary<byte, ServerNetworkPlayerDescription> _latestDescriptions;
        private readonly Dictionary<byte, Vector3> _positionDeltas;

        private readonly Dictionary<byte, float> _leftRightRotationDeltas;
        private readonly Dictionary<byte, int> _currentFrames;

        internal OtherPlayersMovementProcessor(ClientPlayerManager playerManager)
        {
            _playerManager = playerManager;
            //            _preLatestDescriptions = new Dictionary<byte, ServerNetworkPlayerDescription>();
            //            _latestDescriptions = new Dictionary<byte, ServerNetworkPlayerDescription>();
            _positionDeltas = new Dictionary<byte, Vector3>();
            _currentFrames = new Dictionary<byte, int>();
            _leftRightRotationDeltas = new Dictionary<byte, float>();
        }

        internal void OnPlayerAdd(byte slot)
        {
            //            _latestDescriptions[slot] = new ServerNetworkPlayerDescription();
            //            _preLatestDescriptions[slot] = new ServerNetworkPlayerDescription();
            _positionDeltas[slot] = new Vector3();
            _currentFrames[slot] = 0;
            _leftRightRotationDeltas[slot] = 0;
        }

        internal void UpdateWithMessage(ServerOtherPlayerDeltaMessage message)
        {
            Player player = _playerManager.OtherPlayers[message.Slot];

            if (player != null)
            {
                int leftFrames = _currentFrames[message.Slot];
                if (leftFrames > 0)
                {
                    player.Position += _positionDeltas[message.Slot] * leftFrames;
                    player.LeftRightRotation += _leftRightRotationDeltas[message.Slot] * leftFrames;
                }

                _positionDeltas[message.Slot] = (message.PlayerDescription.Position - player.Position) / INTERPOLATION_FRAMES;

                //                float rotationDelta = message.PlayerDescription.LeftRightRotation - player.CameraLeftRightRotation;
                if (message.PlayerDescription.LeftRightRotation == 0.0) // That is some kind of bug
                {
                    return;
                }
                //                if (Math.Abs(message.PlayerDescription.LeftRightRotation - player.CameraLeftRightRotation) < MathHelper.PiOver4)
                {
                    _leftRightRotationDeltas[message.Slot] = (message.PlayerDescription.LeftRightRotation -
                                                              player.LeftRightRotation) / INTERPOLATION_FRAMES;
                }
                //                else
                //                {
                ////                    _leftRightRotationDeltas[message.Slot] = (MathHelper.TwoPi -
                ////                                                              (message.PlayerDescription.LeftRightRotation -
                ////                                                               player.CameraLeftRightRotation)) / INTERPOLATION_FRAMES;
                //                }

                _currentFrames[message.Slot] = INTERPOLATION_FRAMES;
            }
        }

        internal void Update(GameTime gameTime)
        {
            foreach (byte activeOtherPlayerSlot in _playerManager.ActiveOtherPlayerSlots)
            {
                Player player = _playerManager.OtherPlayers[activeOtherPlayerSlot];

                int leftFrames = _currentFrames[activeOtherPlayerSlot];
                if (leftFrames > 0)
                {
                    player.Position += _positionDeltas[player.ServerSlot];
                    player.LeftRightRotation += _leftRightRotationDeltas[player.ServerSlot];

                    _currentFrames[player.ServerSlot] = leftFrames - 1;
                }
            }
        }
    }
}