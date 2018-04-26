using System;
using System.Collections.Generic;
using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Controls
{
    public class ItemCountLabelControl : Control
    {
        public ItemCountLabelControl()
            : this(string.Empty)
        {
        }

        public ItemCountLabelControl(string text)
        {
            Text = text;
        }

        public string Text;
    }
}