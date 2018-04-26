using System;
using Nuclex.UserInterface.Controls.Desktop;

namespace WorldsGame.Utils.UIControls
{
    internal class WorldsColorInputControl : InputControl
    {
        internal WorldsColorInputControl()
        {
            OnCharEntered += c => { };
            OnColorEntered += s => { };
        }

        protected override void OnCharacterEntered(char character)
        {
            if (!"1234567890abcdef".Contains(character.ToString().ToLowerInvariant()))
            {
                return;
            }
            if (Text.Length >= 6)
            {
                return;
            }
            base.OnCharacterEntered(character);
            OnCharEntered(character);

            if (Text.Length == 6)
            {
                OnColorEntered(Text);
            }
        }

        internal event Action<char> OnCharEntered;

        internal event Action<string> OnColorEntered;
    }
}