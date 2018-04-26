using Nuclex.UserInterface.Controls.Desktop;

namespace WorldsGame.Utils.UIControls
{
    internal class NumberInputControl : InputControl
    {
        internal float MaxValue { get; set; }

        internal float MinValue { get; set; }

        internal bool IsPositiveOnly { get; set; }

        internal bool IsIntegerOnly { get; set; }

        internal NumberInputControl()
        {
            MinValue = float.MinValue;
            MaxValue = float.MaxValue;
        }

        protected override void OnCharacterEntered(char character)
        {
            string stringLowerInvariant = character.ToString().ToLowerInvariant();

            if (!"1234567890,.-".Contains(stringLowerInvariant))
            {
                return;
            }

            if (IsIntegerOnly && ",.".Contains(stringLowerInvariant))
            {
                return;
            }

            if (".".Contains(stringLowerInvariant))
            {
                character = ',';
            }

            if (IsPositiveOnly && "-".Contains(stringLowerInvariant))
            {
                return;
            }

            if (character == '-' && CaretPosition > 0)
            {
                return;
            }

            string oldText = Text;

            base.OnCharacterEntered(character);

            bool parseResult;
            float value;
            parseResult = float.TryParse(Text, out value);

            if (character == '-' && CaretPosition == 1)
            {
            }
            else if ((!parseResult || value < MinValue || value > MaxValue) && character != '.')
            {
                Text = oldText;
            }
        }

        internal float GetFloat()
        {
            float value;
            bool result = float.TryParse(Text, out value);

            if (!result)
            {
                return 0;
            }

            return value;
        }

        internal int GetInt()
        {
            return (int)GetFloat();
        }
    }
}