using System;

using Microsoft.Xna.Framework.Graphics;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;

namespace WorldsGame.GUI.Loading
{
    internal class LoadingGUI : View.GUI.GUI
    {
        private readonly SpriteFont _defaultFont;
        private LabelControl _middleTextControl;
        private LabelControl _adviceTextControl;

        internal int AdviceTextNegativeY { get { return 170; } }

        internal string MiddleText
        {
            set
            {
                float middleTextWidth = _defaultFont.MeasureString(value).X;

                _middleTextControl.Text = value;
                _middleTextControl.Bounds = new UniRectangle((Screen.Width - middleTextWidth) / 2, Screen.Height / 2,
                                                             middleTextWidth, 30);
            }
        }

        internal string AdviceBottomText
        {
            set
            {
                float adviceTextWidth = _defaultFont.MeasureString(value).X;

                _adviceTextControl.Text = value;
                _adviceTextControl.Bounds = new UniRectangle((Screen.Width - adviceTextWidth) / 2, Screen.Height - AdviceTextNegativeY,
                                                             adviceTextWidth, 30);
            }
        }

        internal LoadingGUI(WorldsGame game)
            : base(game)
        {
            _defaultFont = game.Content.Load<SpriteFont>("Fonts/DefaultFont");
        }

        protected override void CreateControls()
        {
            CreateMiddleText();
            CreateAdviceText();
        }

        private void CreateMiddleText()
        {
            _middleTextControl = new LabelControl("");
            Screen.Desktop.Children.Add(_middleTextControl);

            MiddleText = "Loading";
        }

        private void CreateAdviceText()
        {
            _adviceTextControl = new LabelControl("");
            Screen.Desktop.Children.Add(_adviceTextControl);

            AdviceBottomText = AdviceText.GetNewAdvice();
        }
    }
}