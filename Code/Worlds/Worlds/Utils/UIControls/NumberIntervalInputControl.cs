using System;
using Nuclex.UserInterface.Controls.Desktop;

namespace WorldsGame.Utils.UIControls
{
    internal class NumberIntervalInputControl : InputControl
    {
        internal int MaxValue { get; set; }

        internal int MinValue { get; set; }

        internal NumberIntervalInputControl()
        {
            OnCharEntered += c => { };
        }

        protected override void OnCharacterEntered(char character)
        {
            string stringLowerInvariant = character.ToString().ToLowerInvariant();

            if (!"1234567890-".Contains(stringLowerInvariant))
            {
                return;
            }

            if (character == '-' && CaretPosition == 0 || (MinValue > 0 && character == '0' && CaretPosition == 0))
            {
                return;
            }

            string oldText = Text;

            base.OnCharacterEntered(character);

            string[] values = Text.Split('-');
            int minValue = 0;
            int maxValue = 0;

            if (values.Length > 2)
            {
                Text = oldText;
                return;
            }

            if (values.Length == 1)
            {
                minValue = int.Parse(Text);

                if (minValue > MaxValue)
                {
                    Text = oldText;
                    return;
                }

                //                maxValue = MaxValue;
            }
            //            if (values.Length == 1)
            //            {
            //                minValue = int.Parse(values[0]);
            //
            //
            //
            //                maxValue = MaxValue;
            //            }
            if (values.Length == 2)
            {
                //                minValue = int.Parse(values[0]);
                bool result = int.TryParse(values[1], out maxValue);

                if (!result)
                {
                    return;
                }

                //                if (minValue < MinValue || minValue > MaxValue)
                //                {
                //                    return;
                //                }
                if (maxValue < MinValue || maxValue > MaxValue)
                {
                    Text = oldText;
                    return;
                }
            }

            //            if (minValue > maxValue)
            //            {
            //                Text = oldText;
            //                return;
            //            }

            //            bool parseResult;
            //            float value;
            //            parseResult = float.TryParse(Text, out value);

            //            if (character == '-' && CaretPosition == 1)
            //            {
            //            }
            //            else if ((!parseResult || value < MinValue || value > MaxValue) && character != '.')
            //            {
            //                Text = oldText;
            //                return;
            //            }

            OnCharEntered(character);
        }

        internal event Action<char> OnCharEntered;

        internal bool GetInterval(out int minValue, out int maxValue)
        {
            minValue = MinValue;
            maxValue = MaxValue;
            string[] values = Text.Split('-');

            if (values.Length > 2)
            {
                return false;
            }
            if (values.Length == 1)
            {
                int.TryParse(Text, out minValue);
                int.TryParse(Text, out maxValue);
                return false;
            }
            if (values.Length == 2)
            {
                minValue = int.Parse(values[0]);
                bool result = int.TryParse(values[1], out maxValue);

                if (!result)
                {
                    maxValue = MaxValue;
                    return false;
                }
            }

            return true;
        }
    }
}