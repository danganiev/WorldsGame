using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Gamestates;
using WorldsGame.Saving;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.View.GUI
{
    internal class GUI : IDisposable
    {
        internal const int THREAD_LIST_WAIT_TIME = 10;
        protected const int ALERT_PANEL_HEIGHT = 150;

        private readonly List<PressableControl> _alreadyDisabledPressables;

        internal Screen Screen { get; set; }

        internal WorldsGame Game { get; set; }

        internal MenuState MenuState { get; set; }

        internal ButtonControl backButton;
        internal ButtonControl saveButton;
        internal PanelControl alertBoxPanel;

        protected Viewport viewport;

        protected List<PressableControl> pressableControls = new List<PressableControl>();
        protected List<BaseWorldsTextControl> BaseWorldsTextControls = new List<BaseWorldsTextControl>();
        protected List<ListControl> listControls = new List<ListControl>();

        protected LabelControl titleLabel;
        protected LabelControl alertBoxLabel;

        protected virtual string LabelText { get { return ""; } }

        protected virtual string DeletionText { get { return ""; } }

        protected virtual bool IsSaveable { get { return false; } }

        protected virtual bool IsBackable { get { return false; } }

        protected virtual int LabelWidth { get { return 100; } }

        internal int LabelHeight { get { return 30; } }

        protected int TitleX { get { return 100; } }

        protected virtual int TitleY { get { return 60; } }

        protected virtual int FirstRowLabelX { get { return TitleX; } }

        protected virtual int ListHeight { get { return 150; } }

        protected int FinishButtonsY { get { return 500; } }

        protected virtual int ButtonWidth { get { return 65; } }

        protected int ButtonHeight { get { return 30; } }

        protected virtual int ButtonDistanceFromTop { get { return 100; } }

        protected UniScalar ButtonDistanceFromBottom { get { return new UniScalar(1f, -100f); } }

        protected virtual int ButtonDistanceFromLeft { get { return 160; } }

        protected UniScalar ButtonDistanceFromRight { get { return new UniScalar(1f, -160f); } }

        protected virtual int AlertPanelWidth { get { return 470; } }

        //Special case for popup gui, where we don't need to introduce screen
        internal GUI()
        {
        }

        internal GUI(WorldsGame game)
        {
            Game = game;
            viewport = Game.GraphicsDevice.Viewport;

            var screen = new Screen(viewport.Width, viewport.Height);
            Game.GUIManager.Screen = screen;

            Screen = screen;
            _alreadyDisabledPressables = new List<PressableControl>();
        }

        internal GUI(WorldsGame game, Screen screen)
        {
            Game = game;
            viewport = Game.GraphicsDevice.Viewport;

            Game.GUIManager.Screen = screen;

            Screen = screen;
            _alreadyDisabledPressables = new List<PressableControl>();
        }

        protected virtual void CreateControls()
        {
            Screen.Desktop.Children.Clear();

            Game.GUIManager.Screen = Screen;

            AddTitle();

            if (IsBackable)
            {
                AddBackButton();

                if (IsSaveable)
                {
                    AddSaveButton();
                }
            }
        }

        internal virtual void Update(GameTime gameTime)
        {
        }

        protected virtual void LoadData()
        {
        }

        protected virtual void Back()
        {
        }

        protected virtual void Save()
        {
        }

        protected virtual void Save(bool goBack)
        {
        }

        protected virtual void Delete()
        {
        }

        protected void AddTitle()
        {
            titleLabel = new LabelControl
            {
                Text = LabelText,
                Bounds = new UniRectangle(TitleX, TitleY, 110f, 24f),
                IsHeader = true
            };

            Screen.Desktop.Children.Add(titleLabel);
        }

        internal virtual void DisableControls()
        {
            foreach (var pressableControl in pressableControls)
            {
                if (pressableControl.Enabled)
                {
                    pressableControl.Enabled = false;
                }
                else
                {
                    _alreadyDisabledPressables.Add(pressableControl);
                }
            }

            foreach (var BaseWorldsTextControl in BaseWorldsTextControls)
            {
                BaseWorldsTextControl.Enabled = false;
            }

            foreach (ListControl listControl in listControls)
            {
                listControl.SelectionMode = ListSelectionMode.None;
            }
        }

        internal virtual void EnableControls()
        {
            foreach (var pressableControl in pressableControls)
            {
                if (!_alreadyDisabledPressables.Contains(pressableControl))
                {
                    pressableControl.Enabled = true;
                }
            }

            foreach (var BaseWorldsTextControl in BaseWorldsTextControls)
            {
                BaseWorldsTextControl.Enabled = true;
            }

            foreach (ListControl listControl in listControls)
            {
                listControl.SelectionMode = ListSelectionMode.Single;
            }

            _alreadyDisabledPressables.Clear();
        }

        protected void LoadList<T>(ListControl list, SaverHelper<T> saverHelper, string preselectedValue = "",
            List<string> extensions = null, IEnumerable<string> additionalItems = null) where T : class, ISaveDataSerializable<T>
        {
            if (extensions == null)
            {
                extensions = new List<string> { "sav" };
            }
            list.Items.Clear();

            if (additionalItems != null)
            {
                foreach (string additionalItem in additionalItems)
                {
                    AddItem(list, preselectedValue, additionalItem);
                }
            }

            if (saverHelper != null)
            {
                foreach (var listFileName in saverHelper.LoadNames())
                {
                    var listNameArray = listFileName.Split('.').ToList();

                    if (!extensions.Contains(listNameArray.Last()))
                    {
                        continue;
                    }

                    var listNameQuery = listNameArray.Take(listNameArray.Count - 1);
                    string value = string.Join(".", listNameQuery);

                    AddItem(list, preselectedValue, value);
                }
            }
        }

        protected void LoadList(ListControl list, IEnumerable<string> items, string preselectedValue = "")
        {
            list.Items.Clear();

            if (items != null)
            {
                foreach (string item in items)
                {
                    AddItem(list, preselectedValue, item);
                }
            }
        }

        protected void LoadPopupList<T>(ListControl list, IDictionary<T, Tuple<string, string>> items, string preselectedValue = "")
        {
            list.Items.Clear();

            list.ArePopupsCustom = true;

            if (items != null)
            {
                foreach (KeyValuePair<T, Tuple<string, string>> item in items)
                {
                    AddPopupItem(list, preselectedValue, item.Value.Item1, item.Value.Item2);
                }
            }
        }

        private static void AddItem(ListControl list, string preselectedValue, string item)
        {
            list.Items.Add(item);

            if (item == preselectedValue)
            {
                int index = list.Items.Count - 1;
                list.SelectedItems.Add(index);
            }
        }

        private static void AddPopupItem(ListControl list, string preselectedValue, string item, string popupText)
        {
            list.Items.Add(item);
            list.Popups.Add(popupText);

            if (item == preselectedValue)
            {
                int index = list.Items.Count - 1;
                list.SelectedItems.Add(index);
            }
        }

        private void AddBackButton()
        {
            backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(ButtonDistanceFromRight, ButtonDistanceFromBottom, 70, 30)
            };
            backButton.Pressed += (sender, args) => Back();

            pressableControls.Add(backButton);
            Screen.Desktop.Children.Add(backButton);
        }

        protected void AddSaveButton()
        {
            saveButton = new ButtonControl
            {
                Text = "Save",
                Bounds = new UniRectangle(backButton.Bounds.Left - 110, ButtonDistanceFromBottom, 100, 30)
            };
            saveButton.Pressed += (sender, args) => Save();

            pressableControls.Add(saveButton);
            Screen.Desktop.Children.Add(saveButton);
        }

        // Doesn't have 'OK' button, shows when loading, etc.
        protected void ShowMessageBox(string text)
        {
            DisableControls();
            MakeAlertBoxPanel();

            var textLabel = new LabelControl(text)
            {
                Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(0f, 10f), 150, 100)
            };

            alertBoxPanel.Children.Add(textLabel);

            Screen.Desktop.Children.Add(alertBoxPanel);
            alertBoxPanel.BringToFront();
        }

        internal void ShowAlertBox(string firstline, string secondline = "")
        {
            DisableControls();
            MakeAlertBoxPanel();

            if (Screen.Desktop.Children.Contains(alertBoxPanel))
            {
                return;
            }

            //            int height = 100;

            //            if (secondline != "")
            //            {
            //                height += 0;
            //            }

            var X = new UniScalar(0f, 10f);

            var firstLineLabel = new LabelControl(firstline)
            {
                Bounds = new UniRectangle(X, new UniScalar(0f, 10f), 150, 30)
            };

            alertBoxPanel.Children.Add(firstLineLabel);

            if (secondline != "")
            {
                var secondLineLabel = new LabelControl(secondline)
                {
                    Bounds = new UniRectangle(X, firstLineLabel.Bounds.Bottom + 10, 150, 30)
                };

                alertBoxPanel.Children.Add(secondLineLabel);
            }

            const int cancelButtonWidth = 100;

            var cancelButton = new ButtonControl
            {
                Text = "OK",
                Bounds = new UniRectangle(new UniScalar(0f, ((AlertPanelWidth - cancelButtonWidth) / 2)), new UniScalar(1f, -40f), cancelButtonWidth, 30)
            };
            cancelButton.Pressed += (sender, args) => CancelAlertBox();

            alertBoxPanel.Children.Add(cancelButton);

            Screen.Desktop.Children.Add(alertBoxPanel);
            alertBoxPanel.BringToFront();
        }

        protected virtual void ShowDeletionAlertBox(Action deleteAction = null, string deletionText = "")
        {
            DisableControls();
            Action endDeleteAction = deleteAction ?? Delete;

            MakeAlertBoxPanel();

            var textLabel = new LabelControl(deletionText == "" ? DeletionText : deletionText)
            {
                Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(0f, 10f), 150, 100)
            };

            alertBoxPanel.Children.Add(textLabel);

            var cancelButton = new ButtonControl
            {
                Text = "Cancel",
                Bounds = new UniRectangle(new UniScalar(1f, -100f), new UniScalar(1f, -40f), 90, ButtonHeight)
            };
            cancelButton.Pressed += (sender, args) => CancelAlertBox();

            alertBoxPanel.Children.Add(cancelButton);

            var deleteButton = new ButtonControl
            {
                Text = "Delete",
                Bounds = new UniRectangle(cancelButton.Bounds.Left - 120f, new UniScalar(1f, -40f), 110, ButtonHeight)
            };
            deleteButton.Pressed += (sender, args) => endDeleteAction();

            alertBoxPanel.Children.Add(deleteButton);

            Screen.Desktop.Children.Add(alertBoxPanel);
            alertBoxPanel.BringToFront();
        }

        protected virtual void ShowOKCancelAlertBox(Action OKAction, Action cancelAction, string text)
        {
            DisableControls();

            MakeAlertBoxPanel();

            alertBoxLabel = new LabelControl(text)
            {
                Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(0f, 10f), 150, 100)
            };

            alertBoxPanel.Children.Add(alertBoxLabel);

            var OKButton = new ButtonControl
            {
                Text = "OK",
                Bounds = new UniRectangle(new UniScalar(1f, -100f), new UniScalar(1f, -40f), 90, ButtonHeight)
            };
            OKButton.Pressed += (sender, args) => OKAction();

            alertBoxPanel.Children.Add(OKButton);

            var cancelButton = new ButtonControl
            {
                Text = "Cancel",
                Bounds = new UniRectangle(OKButton.Bounds.Left - 120f, new UniScalar(1f, -40f), 110, ButtonHeight)
            };
            cancelButton.Pressed += (sender, args) => cancelAction();

            alertBoxPanel.Children.Add(cancelButton);

            Screen.Desktop.Children.Add(alertBoxPanel);
            alertBoxPanel.BringToFront();
        }

        private void MakeAlertBoxPanel(int additionalHeight = 0)
        {
            int height = ALERT_PANEL_HEIGHT + additionalHeight;
            var alertPanelDistanceFromTop = (int)((Screen.Height - height) / 2);
            var alertPanelDistanceFromLeft = (int)((Screen.Width - AlertPanelWidth) / 2);

            alertBoxPanel = new PanelControl
            {
                Bounds =
                    new UniRectangle(new UniScalar(0f, alertPanelDistanceFromLeft),
                                        new UniScalar(0f, alertPanelDistanceFromTop), AlertPanelWidth,
                                        height)
            };
        }

        protected bool IsFileNameOK(string name)
        {
            return name.All(c => Char.IsLetterOrDigit(c) || c == '_' || (Char.IsWhiteSpace(c) && c != '\t' && c != '\r'));
        }

        protected void CancelAlertBox()
        {
            EnableControls();
            Screen.Desktop.Children.Remove(alertBoxPanel);
        }

        internal virtual void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }

        internal virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }

        internal virtual void Start()
        {
            CreateControls();
            LoadData();
        }

        public virtual void Dispose()
        {
        }
    }
}