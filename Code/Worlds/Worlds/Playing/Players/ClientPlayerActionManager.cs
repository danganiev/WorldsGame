using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using WorldsGame.Players.MovementBehaviours;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.Settings;

namespace WorldsGame.Playing.Players
{
    // The ultimate manager of player actions, whether they are coming from the keyboard, or deployed by jesus mothafuking christ himself
    internal class ClientPlayerActionManager : IDisposable
    {
        private Dictionary<Keys, PossiblePlayerContiniousActions> _continiousActions;
        private Dictionary<Keys, PossiblePlayerSingleActions> _singleActions;

        internal HashSet<Keys> ExceptionalToDisableKeys { get; private set; }

        internal bool Enabled { get; set; }

        internal event Action OnPlayerStartedMovingForward;

        internal void PlayerStartedMovingForward()
        {
            OnPlayerStartedMovingForward();
        }

        internal event Action OnPlayerStartedMovingBack;

        internal void PlayerStartedMovingBack()
        {
            OnPlayerStartedMovingBack();
        }

        internal event Action OnPlayerStartedStrafingRight;

        internal void PlayerStartedStrafingRight()
        {
            OnPlayerStartedStrafingRight();
        }

        internal event Action OnPlayerStartedStrafingLeft;

        internal void PlayerStartedStrafingLeft()
        {
            OnPlayerStartedStrafingLeft();
        }

        internal event Action OnPlayerStoppedMovingForward;

        internal void PlayerStoppedMovingForward()
        {
            OnPlayerStoppedMovingForward();
        }

        internal event Action OnPlayerStoppedMovingBack;

        internal void PlayerStoppedMovingBack()
        {
            OnPlayerStoppedMovingBack();
        }

        internal event Action OnPlayerStoppedStrafingRight;

        internal void PlayerStoppedStrafingRight()
        {
            OnPlayerStoppedStrafingRight();
        }

        internal event Action OnPlayerStoppedStrafingLeft;

        internal void PlayerStoppedStrafingLeft()
        {
            OnPlayerStoppedStrafingLeft();
        }

        internal event Action OnPlayerJumped;

        internal void PlayerJumped()
        {
            OnPlayerJumped();
        }

        internal event Action OnPlayerDidPrimaryAction;

        internal void PlayerDidPrimaryAction()
        {
            OnPlayerDidPrimaryAction();
        }

        internal event Action OnPlayerDidSecondaryAction;

        internal void PlayerDidSecondaryAction()
        {
            OnPlayerDidSecondaryAction();
        }

        internal event Action OnPlayerToggledInventory;

        internal void PlayerToggledInventory()
        {
            OnPlayerToggledInventory();
        }

        internal event Action<byte> OnPlayerSelectedItem;

        internal void PlayerSelectedItem(byte item)
        {
            OnPlayerSelectedItem(item);
        }

        internal event Action OnPlayerWroteInChat;

        internal void PlayerWroteInChat()
        {
            OnPlayerWroteInChat();
        }

        internal event Action OnPlayerChangedCamera;

        internal void PlayerChangedCamera()
        {
            OnPlayerChangedCamera();
        }

        internal ClientPlayerActionManager()
        {
            ResetEvents();

            Enabled = true;

            _continiousActions = SettingsManager.ControlSettings.GetContiniousActionsKeys();
            _singleActions = SettingsManager.ControlSettings.GetSingleActionsKeys();

            ExceptionalToDisableKeys = new HashSet<Keys>();
        }

        internal void SubscribeToInputs()
        {
            Messenger.On<Keys>("KeyPressed", OnKeyPressed);

            Messenger.On<Keys>("KeyReleased", OnKeyReleased);
        }

        private void OnKeyPressed(Keys key)
        {
            if (!Enabled && !ExceptionalToDisableKeys.Contains(key))
            {
                return;
            }

            if (_continiousActions.ContainsKey(key))
            {
                switch (_continiousActions[key])
                {
                    case PossiblePlayerContiniousActions.MoveForward:
                        PlayerStartedMovingForward();
                        break;

                    case PossiblePlayerContiniousActions.MoveBack:
                        PlayerStartedMovingBack();
                        break;

                    case PossiblePlayerContiniousActions.StrafeLeft:
                        PlayerStartedStrafingLeft();
                        break;

                    case PossiblePlayerContiniousActions.StrafeRight:
                        PlayerStartedStrafingRight();
                        break;
                }
            }
            if (_singleActions.ContainsKey(key))
            {
                switch (_singleActions[key])
                {
                    case PossiblePlayerSingleActions.Jump:
                        PlayerJumped();
                        break;

                    case PossiblePlayerSingleActions.PrimaryAction:
                        PlayerDidPrimaryAction();
                        break;

                    case PossiblePlayerSingleActions.SecondaryAction:
                        PlayerDidSecondaryAction();
                        break;

                    case PossiblePlayerSingleActions.Say:
                        PlayerWroteInChat();
                        break;

                    case PossiblePlayerSingleActions.InventoryToggle:
                        PlayerToggledInventory();
                        break;

                    case PossiblePlayerSingleActions.InventoryItem1:
                        PlayerSelectedItem(0);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem2:
                        PlayerSelectedItem(1);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem3:
                        PlayerSelectedItem(2);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem4:
                        PlayerSelectedItem(3);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem5:
                        PlayerSelectedItem(4);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem6:
                        PlayerSelectedItem(5);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem7:
                        PlayerSelectedItem(6);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem8:
                        PlayerSelectedItem(7);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem9:
                        PlayerSelectedItem(8);
                        break;

                    case PossiblePlayerSingleActions.InventoryItem10:
                        PlayerSelectedItem(9);
                        break;

                    case PossiblePlayerSingleActions.ChangeCameraView:
                        PlayerChangedCamera();
                        break;
                }
            }
        }

        private void OnKeyReleased(Keys key)
        {
            if (!Enabled && !ExceptionalToDisableKeys.Contains(key))
            {
                return;
            }

            if (_continiousActions.ContainsKey(key))
            {
                switch (_continiousActions[key])
                {
                    case PossiblePlayerContiniousActions.MoveForward:
                        PlayerStoppedMovingForward();
                        break;

                    case PossiblePlayerContiniousActions.MoveBack:
                        PlayerStoppedMovingBack();
                        break;

                    case PossiblePlayerContiniousActions.StrafeLeft:
                        PlayerStoppedStrafingLeft();
                        break;

                    case PossiblePlayerContiniousActions.StrafeRight:
                        PlayerStoppedStrafingRight();
                        break;
                }
            }
        }

        internal void UnsubscribeFromKeyboardInputs()
        {
            Messenger.Off<Keys>("KeyPressed", OnKeyPressed);
            Messenger.Off<Keys>("KeyReleased", OnKeyReleased);
        }

        internal void ResetEvents()
        {
            OnPlayerStartedMovingForward = () => { };
            OnPlayerStartedMovingBack = () => { };
            OnPlayerStartedStrafingRight = () => { };
            OnPlayerStartedStrafingLeft = () => { };
            OnPlayerStoppedMovingForward = () => { };
            OnPlayerStoppedMovingBack = () => { };
            OnPlayerStoppedStrafingRight = () => { };
            OnPlayerStoppedStrafingLeft = () => { };
            OnPlayerJumped = () => { };
            OnPlayerDidPrimaryAction = () => { };
            OnPlayerDidSecondaryAction = () => { };
            OnPlayerToggledInventory = () => { };
            OnPlayerWroteInChat = () => { };
            OnPlayerSelectedItem = item => { };
            OnPlayerChangedCamera = () => { };
        }

        public void Dispose()
        {
            ResetEvents();
        }
    }
}