using Microsoft.Xna.Framework;

using WorldsGame.Editors.Textures;
using WorldsGame.GUI;
using WorldsGame.GUI.ModelEditor;
using WorldsGame.Saving;

namespace WorldsGame.Gamestates
{
    //TODO !!!: Memory leak when going in and out many times to this state
    // UPD: Not anymore. (But check for it anyway)

    internal enum ModelEditorType
    {
        None,
        Block,
        Character,
        Item
    }

    internal class TextureDrawingState : WorldsGameState
    {
        private TextureEditor _textureEditor;
        private readonly WorldSettings _worldSettings;
        private readonly Color[] _colors;
        private readonly Texture _texture;
        private readonly bool _isIcon;
        private readonly CharacterAttributeEditorGUI _characterAttributeEditorGUI;
        private readonly ItemOptionsPopupGUI _itemOptionsPopupGUI;

        internal ModelEditorType EditorType { get; private set; }

        internal TextureDrawingState(WorldsGame game, WorldSettings worldSettings)
            : base(game)
        {
            Game.IsMouseVisible = true;
            _worldSettings = worldSettings;
        }

        internal TextureDrawingState(WorldsGame game, WorldSettings worldSettings, Texture texture)
            : base(game)
        {
            Game.IsMouseVisible = true;
            _worldSettings = worldSettings;
            _texture = texture;
            _isIcon = false;
        }

        internal TextureDrawingState(WorldsGame game, WorldSettings worldSettings, Color[] colors, CharacterAttributeEditorGUI characterAttributeEditorGUI)
            : base(game)
        {
            Game.IsMouseVisible = true;
            _worldSettings = worldSettings;
            _colors = colors;
            _isIcon = true;
            _characterAttributeEditorGUI = characterAttributeEditorGUI;
        }

        internal TextureDrawingState(WorldsGame game, WorldSettings worldSettings, Color[] colors, ItemOptionsPopupGUI itemOptionsPopupGUI)
            : base(game)
        {
            Game.IsMouseVisible = true;
            _worldSettings = worldSettings;
            _colors = colors;
            _isIcon = true;
            _itemOptionsPopupGUI = itemOptionsPopupGUI;
        }

        internal TextureDrawingState(WorldsGame game, WorldSettings worldSettings, ModelEditorType editorType)
            : base(game)
        {
            Game.IsMouseVisible = true;
            _worldSettings = worldSettings;
            EditorType = editorType;
        }

        protected override void Initialize()
        {
            if (!_isIcon)
            {
                _textureEditor = new TextureEditor(Game, GraphicsDevice, _worldSettings, _texture);
            }
            else
            {
                if (_characterAttributeEditorGUI != null)
                {
                    _textureEditor = new TextureEditor(
                        Game, GraphicsDevice, _worldSettings, _colors, _characterAttributeEditorGUI);
                }
                else if (_itemOptionsPopupGUI != null)
                {
                    _textureEditor = new TextureEditor(
                        Game, GraphicsDevice, _worldSettings, _colors, _itemOptionsPopupGUI);
                }
            }

            _textureEditor.Initialize();
        }

        protected override void LoadContent()
        {
            _textureEditor.LoadContent();
        }

        protected override void UnloadContent()
        {
            _textureEditor.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            _textureEditor.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _textureEditor.Draw(gameTime);

            base.Draw(gameTime);
        }

        public override void Dispose()
        {
            base.Dispose();
            _textureEditor.Dispose();
        }
    }
}