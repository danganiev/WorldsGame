using System;
using Microsoft.Xna.Framework.Input;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.Input;

namespace WorldsGame.Editors
{
    // I might be getting wrong the whole idea of action manager.
    // Now it only remaps events to other events, but it might take more responsibility, and operate objects.
    internal class ModelEditorActionManager : IDisposable
    {
        private readonly InputController _inputController;

        private bool IsMiddleMouseButtonPressed { get { return _inputController.CurrentMouseState.MiddleButton == ButtonState.Pressed; } }

        private bool IsLeftMouseButtonPressed { get { return _inputController.CurrentMouseState.LeftButton == ButtonState.Pressed; } }

        private bool IsRightMouseButtonPressed { get { return _inputController.CurrentMouseState.RightButton == ButtonState.Pressed; } }

        private bool IsShiftPressed { get { return _inputController.CurrentKeyboardState.IsKeyDown(Keys.LeftShift); } }

        private bool IsCtrlPressed { get { return _inputController.CurrentKeyboardState.IsKeyDown(Keys.LeftControl); } }

        private bool IsAltPressed { get { return _inputController.CurrentKeyboardState.IsKeyDown(Keys.LeftAlt); } }

        //        private bool _isDeltaXDetectionStarted;
        private bool _isDeltaDetectionStarted;

        private float _startingXPoint;
        private float _startingYPoint;

        //params: rotation around Y, rotation around X
        internal event Action<float, float> OnRotateCamera;

        internal event Action<float> OnScale;

        internal event Action<Cursor> MouseLeftClick;

        internal event Action<Cursor, float, float> OnLeftClickContiniousAction;

        internal event Action<Cursor, float, float> OnShiftLeftClickContiniousAction;

        internal event Action<Cursor, float, float> OnCtrlLeftClickContiniousAction;

        internal event Action<Cursor, float, float> OnAltLeftClickContiniousAction;

        internal event Action<Cursor, float, float> OnRightClickContiniousAction;

        internal event Action<Cursor, float, float> OnShiftRightClickContiniousAction;

        internal event Action<Cursor, float, float> OnCtrlRightClickContiniousAction;

        internal event Action<Cursor, float, float> OnAltRightClickContiniousAction;

        internal event Action OnRightMouseButtonReleased;

        internal event Action OnLeftMouseButtonReleased;

        internal event Action<int> OnScrollWheelDeltaChanged;

        internal void RotatedLeft()
        {
            OnRotateCamera(1, 0);
        }

        internal void RotatedRight()
        {
            OnRotateCamera(-1, 0);
        }

        internal void ScaledUp()
        {
            OnScale(1.05f);
        }

        internal void ScaledDown()
        {
            OnScale(0.95f);
        }

        internal void ScrollWheelDeltaChanged(int ticks)
        {
            OnScrollWheelDeltaChanged(ticks);
        }

        internal ModelEditorActionManager(InputController inputController)
        {
            _inputController = inputController;

            ResetEvents();
        }

        internal void SubscribeToInputs()
        {
            //            Messenger.On("QKeyPressed", RotatedLeft);
            //            Messenger.On("EKeyPressed", RotatedRight);

            Messenger.On("PageUpKeyPressed", ScaledUp);
            Messenger.On("PageDownKeyPressed", ScaledDown);

            Messenger.On<float, float>("MouseDeltaChange", OnMouseDeltaChange);
            Messenger.On("MouseLeftButtonClick", OnMouseLeftClick);

            Messenger.On<int>("ScrollWheelDeltaChanged", ScrollWheelDeltaChanged);
        }

        private void OnMouseLeftClick()
        {
            MouseLeftClick(_inputController.Cursor);
        }

        private void OnMouseDeltaChange(float deltaX, float deltaY)
        {
            if (_inputController.PreviousMouseState.RightButton == ButtonState.Pressed && _inputController.CurrentMouseState.RightButton == ButtonState.Released)
            {
                OnRightMouseButtonReleased();
            }
            if (_inputController.PreviousMouseState.LeftButton == ButtonState.Pressed && _inputController.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                OnLeftMouseButtonReleased();
            }

            if (IsMiddleMouseButtonPressed)
            {
                if (!_isDeltaDetectionStarted)
                {
                    _isDeltaDetectionStarted = true;
                    _startingXPoint = deltaX;
                    _startingYPoint = deltaY;
                }
                else
                {
                    OnRotateCamera((_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);
                    _isDeltaDetectionStarted = false;
                }
            }
            else if (IsLeftMouseButtonPressed)
            {
                if (!_isDeltaDetectionStarted)
                {
                    _isDeltaDetectionStarted = true;
                    _startingXPoint = deltaX;
                    _startingYPoint = deltaY;
                }
                else
                {
                    _isDeltaDetectionStarted = false;
                    if (IsShiftPressed)
                    {
                        OnShiftLeftClickContiniousAction(_inputController.Cursor, (_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);

                        return;
                    }
                    if (IsCtrlPressed)
                    {
                        OnCtrlLeftClickContiniousAction(_inputController.Cursor, (_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);
                        return;
                    }
                    if (IsAltPressed)
                    {
                        OnAltLeftClickContiniousAction(_inputController.Cursor, (_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);
                        return;
                    }
                    OnLeftClickContiniousAction(_inputController.Cursor, (_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);
                }
            }
            else if (IsRightMouseButtonPressed)
            {
                if (!_isDeltaDetectionStarted)
                {
                    _isDeltaDetectionStarted = true;
                    _startingXPoint = deltaX;
                    _startingYPoint = deltaY;
                }
                else
                {
                    _isDeltaDetectionStarted = false;

                    if (IsShiftPressed)
                    {
                        OnShiftRightClickContiniousAction(_inputController.Cursor, (_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);
                        return;
                    }
                    if (IsCtrlPressed)
                    {
                        OnCtrlRightClickContiniousAction(_inputController.Cursor, (_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);
                        return;
                    }
                    if (IsAltPressed)
                    {
                        OnAltRightClickContiniousAction(_inputController.Cursor, (_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);
                        return;
                    }
                    OnRightClickContiniousAction(_inputController.Cursor, (_startingXPoint - deltaX) / 10, (_startingYPoint - deltaY) / 10);
                }
            }
            else if (_isDeltaDetectionStarted)
            {
                _isDeltaDetectionStarted = false;
            }
        }

        internal void UnsubscribeFromKeyboardInputs()
        {
            //            Messenger.Off("QKeyPressed", RotatedLeft);
            //            Messenger.Off("EKeyPressed", RotatedRight);

            Messenger.Off("PageUpKeyPressed", ScaledUp);
            Messenger.Off("PageDownKeyPressed", ScaledDown);

            Messenger.Off<int>("ScrollWheelDeltaChanged", ScrollWheelDeltaChanged);

            Messenger.Off<float, float>("MouseDeltaChange", OnMouseDeltaChange);
        }

        internal void ResetEvents()
        {
            OnRotateCamera = (rotationY, rotationX) => { };
            OnScale = value => { };
            OnLeftClickContiniousAction = (cursor, deltaX, deltaY) => { };
            OnShiftLeftClickContiniousAction = (cursor, deltaX, deltaY) => { };
            OnCtrlLeftClickContiniousAction = (cursor, deltaX, deltaY) => { };
            OnAltLeftClickContiniousAction = (cursor, deltaX, deltaY) => { };
            OnScrollWheelDeltaChanged = delta => { };
            OnRightClickContiniousAction = (cursor, deltaX, deltaY) => { };
            OnShiftRightClickContiniousAction = (cursor, deltaX, deltaY) => { };
            OnCtrlRightClickContiniousAction = (cursor, deltaX, deltaY) => { };
            OnAltRightClickContiniousAction = (cursor, deltaX, deltaY) => { };
            OnRightMouseButtonReleased = () => { };
            OnLeftMouseButtonReleased = () => { };
            MouseLeftClick = cursor => { };
        }

        public void Dispose()
        {
            ResetEvents();
        }
    }
}