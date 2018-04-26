using System;
using System.Threading;
using System.Windows.Forms;
using Nuclex.UserInterface.Controls.Desktop;

namespace WorldsGame.Utils.UIControls
{
    internal class BaseWorldsTextControl : InputControl
    {
        internal event Action<char> OnCharEntered;

        internal event Action<string> OnTextChanged;

        internal BaseWorldsTextControl()
        {
            OnCharEntered += c => { };
            OnTextChanged += s => { };
        }

        protected override void OnCharacterEntered(char character)
        {
            // TAB, Esc, Ctrl+Z, Enter
            if (character == 9 || character == 27 || character == 26 || character == (char)Keys.Return)
            {
                return;
            }

            base.OnCharacterEntered(character);

            // Ctrl + V
            if (character == 22)
            {
                Paste();
            }

            OnCharEntered(character);
            OnTextChanged(Text);
        }

        private string ClipboardText { get; set; }

        private void Paste()
        {
            var thread = new Thread(GetClipboardText);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            while (thread.IsAlive)
            {
            }

            Text = Text.Substring(0, Text.Length - 1) + ClipboardText;
            CaretPosition = Text.Length;
        }

        [STAThread]
        private void GetClipboardText()
        {
            ClipboardText = Clipboard.GetText(TextDataFormat.UnicodeText);
        }
    }
}