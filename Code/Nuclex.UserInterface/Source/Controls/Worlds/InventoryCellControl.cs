using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Source.Controls.Worlds
{
    /// <summary>A cell that contains an icon</summary>
    public class InventoryCellControl : Control, IFocusable, IDisposable
    {
        public const int ICON_CELL_SIZE = 48;

        /// <summary>Whether the command is pressed down using the mouse</summary>
        private bool _pressedDownByMouse;

        /// <summary>Whether the mouse is hovering over the command</summary>
        private bool _mouseHovering;

        /// <summary>Whether the user can interact with the choice</summary>
        public bool Enabled;

        private string _itemName;

        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                if (_textPopup != null)
                {
                    _textPopup.Text = value;
                }
            }
        }

        public event Action<string, int> OnLeftMouseClick;

        public event Action<string, int> OnRightMouseClick;

        public bool IsClicked { get; set; }

        public bool IsEmpty { get; set; }

        public Texture2D Icon { get; set; }

        public int Index { get; set; }

        private int _quantity;

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                _quantitylabel.Text = value > 1 ? value.ToString() : "";
            }
        }

        /// <summary> Popup that shows on mouse hover </summary>
        private TextPopupControl _textPopup;

        private ItemCountLabelControl _quantitylabel;

        /// <summary>Initializes a new command control</summary>
        public InventoryCellControl()
        {
            Enabled = true;

            _textPopup = new TextPopupControl
            {
                Enabled = false,
                Text = ItemName,
                IsInventory = true
            };

            _quantitylabel = new ItemCountLabelControl
            {
                Text = "",
                Bounds = new UniRectangle(new UniScalar(0f, 3), new UniScalar(1f, -10), 1, 1)
            };

            Children.Add(_textPopup);
            Children.Add(_quantitylabel);
        }

        /// <summary>Whether the mouse pointer is hovering over the control</summary>
        public bool MouseHovering
        {
            get { return _mouseHovering; }
        }

        /// <summary>Whether the pressable control is in the depressed state</summary>
        public virtual bool Depressed
        {
            get
            {
                bool mousePressed = (_mouseHovering && _pressedDownByMouse);
                return mousePressed;
            }
        }

        /// <summary>Whether the control currently has the input focus</summary>
        public bool HasFocus
        {
            get
            {
                return
                    (Screen != null) &&
                    ReferenceEquals(Screen.FocusedControl, this);
            }
        }

        /// <summary>
        ///   Called when the mouse has entered the control and is now hovering over it
        /// </summary>
        protected override void OnMouseEntered()
        {
            _mouseHovering = true;
            _textPopup.Enabled = true;
        }

        /// <summary>
        ///   Called when the mouse has left the control and is no longer hovering over it
        /// </summary>
        protected override void OnMouseLeft()
        {
            // Intentionally not calling OnActivated() here because the user has moved
            // the mouse away from the command while holding the mouse button down -
            // a common trick under windows to last-second-abort the clicking of a button
            _mouseHovering = false;
            _textPopup.Enabled = false;
        }

        /// <summary>Called when a mouse button has been pressed down</summary>
        /// <param name="button">Index of the button that has been pressed</param>
        protected override void OnMousePressed(MouseButtons button)
        {
            if (Enabled)
            {
                if (button == MouseButtons.Left)
                {
                    _pressedDownByMouse = true;
                    IsClicked = true;
                    OnLeftMouseClick(ItemName, Index);
                }
                if (button == MouseButtons.Right)
                {
                    _pressedDownByMouse = true;
                    IsClicked = true;
                    OnRightMouseClick(ItemName, Index);
                }
            }
        }

        /// <summary>Called when a mouse button has been released again</summary>
        /// <param name="button">Index of the button that has been released</param>
        protected override void OnMouseReleased(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                _pressedDownByMouse = false;

                // Only trigger the pressed event if the mouse was released over the control.
                // The user can move the mouse cursor away from the control while still holding
                // the mouse button down to do the well-known last-second-abort.
                if (_mouseHovering && Enabled)
                {
                    // If this was the final input device holding down the control, meaning it's
                    // not depressed any longer, this counts as a click and we trigger
                    // the notification!
                    if (!Depressed)
                    {
                        //                        OnPressed();
                    }
                }
            }
        }

        /// <summary>Whether the control can currently obtain the input focus</summary>
        bool IFocusable.CanGetFocus
        {
            get { return Enabled; }
        }

        /// <summary>
        /// Gets icon coordinates
        /// </summary>
        public Vector2 GetIconCoordinates()
        {
            RectangleF bounds = GetAbsoluteBounds();
            const int diff = 8 / 2;
            return new Vector2(bounds.X + diff, bounds.Y + diff);
        }

        public void Dispose()
        {
            Icon.Dispose();
            OnLeftMouseClick = null;
            OnRightMouseClick = null;
        }
    }
}