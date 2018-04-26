#region CPL License

/*
Nuclex Framework
Copyright (C) 2002-2010 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/

#endregion CPL License

using System;
using System.Collections.Generic;

namespace Nuclex.UserInterface.Controls.Desktop
{
    /// <summary>Pushable button that can initiate an action</summary>
    public class ButtonControl : PressableControl
    {
        /// <summary>Will be triggered when the button is pressed</summary>
        public event EventHandler Pressed;

        private TextPopupControl _textPopup;

        /// <summary>Text that will be shown on the button</summary>
        public string Text;

        private string _popupText;

        /// <summary>
        /// Whether button is always pressed. (used when using buttons as radio controls for example)
        /// </summary>
        public bool ConstantlyPressed { get; set; }

        /// <summary>Whether the pressable control is in the depressed state</summary>

        // NOTE: Depressed actually means Pressed, what the fuck, Nuclex?
        public override bool Depressed
        {
            get { return base.Depressed || ConstantlyPressed; }
        }

        public string PopupText
        {
            get { return _popupText; }
            set
            {
                _popupText = value;

                if (string.IsNullOrEmpty(value))
                {
                    Children.Remove(_textPopup);
                    return;
                }

                _textPopup = new TextPopupControl
                {
                    Text = value,
                    Enabled = false
                };
                Children.Add(_textPopup);
            }
        }

        /// <summary>Called when the button is pressed</summary>
        protected override void OnPressed()
        {
            if (Pressed != null)
            {
                Pressed(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseEntered()
        {
            base.OnMouseEntered();

            if (_textPopup != null)
            {
                _textPopup.Enabled = true;
            }
        }

        /// <summary>
        ///   Called when the mouse has left the control and is no longer hovering over it
        /// </summary>
        protected override void OnMouseLeft()
        {
            base.OnMouseLeft();

            if (_textPopup != null)
            {
                _textPopup.Enabled = false;
            }
        }
    }
} // namespace Nuclex.UserInterface.Controls.Desktop