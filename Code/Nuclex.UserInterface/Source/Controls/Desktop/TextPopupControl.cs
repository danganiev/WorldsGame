using System;
using Nuclex.Input;

namespace Nuclex.UserInterface.Controls.Desktop
{
    /// <summary>A cell that contains an icon</summary>
    public class TextPopupControl : Control, IDisposable
    {
        /// <summary>Whether the user can interact with the choice</summary>
        public bool Enabled;

        public bool IsInventory { get; set; }

        /// <summary>Name of whatever object lies inside (like item name)</summary>
        public string Text { get; set; }

        /// <summary>Initializes a new command control</summary>
        public TextPopupControl()
        {
            Enabled = true;
        }

        public void Dispose()
        {
        }
    }
}