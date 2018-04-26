using System;

using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using WorldsGame.Gamestates;
using WorldsGame.Saving;
using WorldsGame.Saving.World;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.Textures;
using WorldsGame.Utils.UIControls;
using Texture = WorldsGame.Saving.Texture;

namespace WorldsGame.GUI
{
    internal class WorldEditorGUI : View.GUI.GUI
    {
        private BaseWorldsTextControl _nameInput;

        private ButtonControl _texturesButton;
        private ButtonControl _blocksButton;
        private ButtonControl _objectsButton;
        private ButtonControl _noisesButton;
        private ButtonControl _rulesButton;
        private ButtonControl _parseImagesButton;
        private ButtonControl _characterAttributesButton;
        private ButtonControl _characterEditorButton;
        private ButtonControl _itemEditorButton;
        private ButtonControl _recipeEditorButton;
        private ButtonControl _constantsEditorButton;
        private ButtonControl _atmosphereEditorButton;

        private readonly SpriteFont _defaultFont;
        private LabelControl _middleTextControl;

        private ImageParser _imageParser;

        private const int HORIZONTAL_BUTTON_DISTANCE = 30;

        private bool _isBlockEditorLoadingStarted = false;
        private bool _isCharacterEditorLoadingStarted = false;
        private bool _isItemEditorLoadingStarted = false;

        protected override int LabelWidth { get { return 40; } }

        internal WorldSettings WorldSettings { get; set; }

        internal bool IsNew
        {
            get { return WorldSettings == null; }
        }

        protected override string LabelText
        {
            get { return IsNew ? "Create world" : "Edit world"; }
        }

        protected override bool IsBackable { get { return true; } }

        protected override bool IsSaveable { get { return true; } }

        protected override int ButtonWidth { get { return 200; } }

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

        internal WorldEditorGUI(WorldsGame game)
            : base(game)
        {
            InitializeImageParser();

            _defaultFont = game.Content.Load<SpriteFont>("Fonts/DefaultFont");

            Messenger.On("EscapeKeyPressed", Back);
        }

        internal WorldEditorGUI(WorldsGame game, WorldSettings world)
            : base(game)
        {
            WorldSettings = world;

            InitializeImageParser();

            _defaultFont = game.Content.Load<SpriteFont>("Fonts/DefaultFont");

            Messenger.On("EscapeKeyPressed", Back);
        }

        private void InitializeImageParser()
        {
            _imageParser = new ImageParser(Game.GraphicsDevice, WorldSettings);
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddName();
            AddButtons();
            CreateMiddleText();
        }

        private void AddName()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            var nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Name:"
            };

            _nameInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(nameLabel.Bounds.Right + 10, Y, ButtonWidth * 2 - LabelWidth + HORIZONTAL_BUTTON_DISTANCE - 10, 30),
            };

            if (!IsNew)
                _nameInput.Text = WorldSettings.Name;

            BaseWorldsTextControls.Add(_nameInput);

            Screen.Desktop.Children.Add(nameLabel);
            Screen.Desktop.Children.Add(_nameInput);
        }

        private void AddButtons()
        {
            AddTexturesButton();
            AddImportTexturesButton();
            AddBlocksButton();
            AddObjectsButton();
            AddNoisesButton();
            AddRulesButton();
            AddCharacterAttributesButton();
            AddCharacterEditorButton();
            AddItemEditorButton();
            AddRecipeEditorButton();
            AddConstantsEditorButton();
            AddAtmosphereEditorButton();
        }

        private void AddTexturesButton()
        {
            _texturesButton = new ButtonControl
            {
                Text = "Textures",
                Bounds = new UniRectangle(FirstRowLabelX, _nameInput.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _texturesButton.Pressed += (sender, arguments) => ToTextureList();
            Screen.Desktop.Children.Add(_texturesButton);
        }

        private void AddImportTexturesButton()
        {
            _parseImagesButton = new ButtonControl
            {
                Text = "Import textures",
                Bounds = new UniRectangle(_texturesButton.Bounds.Right + HORIZONTAL_BUTTON_DISTANCE, _nameInput.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _parseImagesButton.Pressed += (sender, args) => ParseImages();
            Screen.Desktop.Children.Add(_parseImagesButton);
        }

        private void AddBlocksButton()
        {
            _blocksButton = new ButtonControl
            {
                Text = "Open block editor",
                Bounds = new UniRectangle(FirstRowLabelX, _texturesButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _blocksButton.Pressed += (sender, arguments) => GoToBlockEditor();
            Screen.Desktop.Children.Add(_blocksButton);
        }

        private void AddObjectsButton()
        {
            _objectsButton = new ButtonControl
            {
                Text = "Open object editor",
                Bounds = new UniRectangle(
                    _blocksButton.Bounds.Right + HORIZONTAL_BUTTON_DISTANCE, _texturesButton.Bounds.Bottom + 10,
                    ButtonWidth, ButtonHeight),
            };
            _objectsButton.PopupText =
                "Object is anything made from more than one block. Like a tree, or a big pizza-themed castle";
            _objectsButton.Pressed += (sender, arguments) => StartObjectEditor();
            Screen.Desktop.Children.Add(_objectsButton);
        }

        private void AddNoisesButton()
        {
            _noisesButton = new ButtonControl
            {
                Text = "Noises",
                Bounds = new UniRectangle(FirstRowLabelX, _objectsButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _noisesButton.PopupText =
                "Noise is a formula that helps rules to decide which blocks or other stuff to spawn in the world";
            _noisesButton.Pressed += (sender, arguments) => ToNoiseList();
            Screen.Desktop.Children.Add(_noisesButton);
        }

        private void AddRulesButton()
        {
            _rulesButton = new ButtonControl
            {
                Text = "Rules",
                Bounds = new UniRectangle(
                    _noisesButton.Bounds.Right + HORIZONTAL_BUTTON_DISTANCE, _objectsButton.Bounds.Bottom + 10,
                    ButtonWidth, ButtonHeight)
            };
            _rulesButton.PopupText = "Rule is a logical statement which uses noise values, and commands the world with their help";
            _rulesButton.Pressed += (sender, arguments) => ToRuleList();
            Screen.Desktop.Children.Add(_rulesButton);
        }

        private void AddCharacterAttributesButton()
        {
            _characterAttributesButton = new ButtonControl
            {
                Text = "Character attributes",
                Bounds = new UniRectangle(FirstRowLabelX, _rulesButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _characterAttributesButton.Pressed += (sender, arguments) => ToCharacterAttributesList();
            Screen.Desktop.Children.Add(_characterAttributesButton);
        }

        private void AddCharacterEditorButton()
        {
            _characterEditorButton = new ButtonControl
            {
                Text = "Open character editor",
                Bounds = new UniRectangle(_characterAttributesButton.Bounds.Right + HORIZONTAL_BUTTON_DISTANCE,
                    _rulesButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _characterEditorButton.Pressed += (sender, arguments) => ToCharacterEditor();
            Screen.Desktop.Children.Add(_characterEditorButton);
        }

        private void AddItemEditorButton()
        {
            _itemEditorButton = new ButtonControl
            {
                Text = "Open item editor",
                Bounds = new UniRectangle(FirstRowLabelX, _characterAttributesButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _itemEditorButton.Pressed += (sender, arguments) => ToItemEditor();
            Screen.Desktop.Children.Add(_itemEditorButton);
        }

        private void AddRecipeEditorButton()
        {
            _recipeEditorButton = new ButtonControl
            {
                Text = "Recipes",
                Bounds = new UniRectangle(
                    _itemEditorButton.Bounds.Right + HORIZONTAL_BUTTON_DISTANCE,
                    _characterAttributesButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _recipeEditorButton.Pressed += (sender, arguments) => ToRecipeEditor();
            Screen.Desktop.Children.Add(_recipeEditorButton);
        }

        private void AddConstantsEditorButton()
        {
            _constantsEditorButton = new ButtonControl
            {
                Text = "World constants",
                Bounds = new UniRectangle(
                    FirstRowLabelX, _itemEditorButton.Bounds.Bottom + 10,
                    ButtonWidth, ButtonHeight)
            };
            _constantsEditorButton.Pressed += (sender, arguments) => ToConstantsEditor();
            Screen.Desktop.Children.Add(_constantsEditorButton);
        }

        private void AddAtmosphereEditorButton()
        {
            _atmosphereEditorButton = new ButtonControl
            {
                Text = "Atmosphere and weather",
                Bounds = new UniRectangle(
                    _constantsEditorButton.Bounds.Right + HORIZONTAL_BUTTON_DISTANCE,
                    _itemEditorButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };
            _atmosphereEditorButton.Pressed += (sender, arguments) => ToAtmosphereEditor();
            Screen.Desktop.Children.Add(_atmosphereEditorButton);
        }

        private void CreateMiddleText()
        {
            _middleTextControl = new LabelControl("");

            MiddleText = "Loading...";
        }

        private void ShowLoadingScreen()
        {
            Screen.Desktop.Children.Clear();
            Screen.Desktop.Children.Add(_middleTextControl);
        }

        private void ToTextureList()
        {
            if (IsSaved())
            {
                Messenger.Off("EscapeKeyPressed");

                var texturesGUI = Texture.GetListGUI(Game, WorldSettings);
                MenuState.SetGUI(texturesGUI);
            }
        }

        private void GoToBlockEditor()
        {
            if (IsSaved())
            {
                _isBlockEditorLoadingStarted = true;
                ShowLoadingScreen();
            }
        }

        private void StartObjectEditor()
        {
            if (IsSaved())
            {
                Messenger.Off("EscapeKeyPressed");

                Game.ChangeState(new LoadingState(Game, WorldSettings, WorldSave.OBJECT_CREATION_WORLD_NAME,
                                                            isObjectCreationState: true));
            }
        }

        private void ToNoiseList()
        {
            if (IsSaved())
            {
                Messenger.Off("EscapeKeyPressed");

                var noisesGUI = Noise.GetListGUI(Game, WorldSettings, MenuState);
                MenuState.SetGUI(noisesGUI);
            }
        }

        private void ToRuleList()
        {
            if (IsSaved())
            {
                Messenger.Off("EscapeKeyPressed");

                var rulesGUI = Rule.GetListGUI(Game, WorldSettings, MenuState);
                MenuState.SetGUI(rulesGUI);
            }
        }

        private void ToCharacterAttributesList()
        {
            if (IsSaved())
            {
                Messenger.Off("EscapeKeyPressed");

                var characterAttributesGUI = CharacterAttribute.GetListGUI(Game, WorldSettings, MenuState);
                MenuState.SetGUI(characterAttributesGUI);
            }
        }

        private void ToCharacterEditor()
        {
            if (IsSaved())
            {
                _isCharacterEditorLoadingStarted = true;
                ShowLoadingScreen();
            }
        }

        private void ToItemEditor()
        {
            if (IsSaved())
            {
                _isItemEditorLoadingStarted = true;
                ShowLoadingScreen();
            }
        }

        private void ToRecipeEditor()
        {
            if (IsSaved())
            {
                Messenger.Off("EscapeKeyPressed");

                var recipeEditorGUI = new RecipeEditorGUI(Game, WorldSettings);
                MenuState.SetGUI(recipeEditorGUI);
            }
        }

        private void ToConstantsEditor()
        {
            if (IsSaved())
            {
                Messenger.Off("EscapeKeyPressed");

                var constantsEditorGUI = new WorldConstantsEditorGUI(Game, WorldSettings);
                MenuState.SetGUI(constantsEditorGUI);
            }
        }

        private void ToAtmosphereEditor()
        {
            if (IsSaved())
            {
                Messenger.Off("EscapeKeyPressed");

                var atmosphereEditorGUI = new AtmosphereEditorGUI(Game, WorldSettings);
                MenuState.SetGUI(atmosphereEditorGUI);
            }
        }

        private void ParseImages()
        {
            if (IsSaved())
            {
                _imageParser.Parse();
                ToTextureList();
            }
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("World needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in world name.");
                return false;
            }

            return true;
        }

        private bool IsSaved()
        {
            if (!IsNameInputOK())
            {
                return false;
            }

            Save(false);

            return true;
        }

        private void DoSave()
        {
            var worldSave = IsNew ? new WorldSettings() : WorldSettings;
            worldSave.Name = _nameInput.Text;

            worldSave.PrepareDefaultSettings(Game);

            worldSave.Save();

            WorldSettings = worldSave;
        }

        internal override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);
            if (_isBlockEditorLoadingStarted)
            {
                Messenger.Off("EscapeKeyPressed");

                Game.GameStateManager.Push(new BlockEditorState(Game, WorldSettings));
            }
            else if (_isCharacterEditorLoadingStarted)
            {
                Messenger.Off("EscapeKeyPressed");

                Game.GameStateManager.Push(new CharacterEditorState(Game, WorldSettings));
            }
            else if (_isItemEditorLoadingStarted)
            {
                Messenger.Off("EscapeKeyPressed");

                Game.GameStateManager.Push(new ItemEditorState(Game, WorldSettings));
            }
        }

        protected override void Save()
        {
            if (!IsNameInputOK())
            {
                return;
            }

            DoSave();

            Back();
        }

        protected override void Save(bool goBack)
        {
            if (!IsNameInputOK())
            {
                return;
            }

            DoSave();

            if (goBack)
            {
                Back();
            }
        }

        protected override void Back()
        {
            Messenger.Off("EscapeKeyPressed", Back);

            var worldsListGUI = new WorldsListGUI(Game);
            MenuState.SetGUI(worldsListGUI);
        }
    }
}