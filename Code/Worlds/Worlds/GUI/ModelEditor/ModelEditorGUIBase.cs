using WorldsGame.GUI.TexturePackEditor;

namespace WorldsGame.GUI.ModelEditor
{
    internal class ModelEditorGUIBase : View.GUI.GUI
    {
        protected TextureListPopupGUI textureListPopupGUI;

        internal ModelEditorGUIBase(WorldsGame game)
            : base(game)
        {
        }

        internal void HideTextureSelectionPanel()
        {
            Screen.Desktop.Children.Remove(textureListPopupGUI.Panel);
            textureListPopupGUI = null;
            EnableControls();
        }

        internal virtual void HideOptionsPanel(bool isKeyframeEdited = false)
        {
        }

        internal virtual void ShowAnimationPlayMenu()
        {
        }

        internal virtual void ShowKeyframeMenu()
        {
        }
    }
}