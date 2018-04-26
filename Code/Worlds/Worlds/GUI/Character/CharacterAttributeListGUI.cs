using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using WorldsGame.Saving;

namespace WorldsGame.GUI
{
    internal class CharacterAttributeListGUI : BaseWorldSublistGUI<CharacterAttribute>
    {
        protected override bool IsFilterNeeded { get { return false; } }

        internal CharacterAttributeListGUI(WorldsGame game, WorldSettings worldSettings, SaverHelper<CharacterAttribute> saverHelper, string preselectedValue)
            : base(game, worldSettings, saverHelper, preselectedValue)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();

            if (!FullList.Contains("Health"))
            {
                CharacterAttribute.CreateHealthAttribute(Game, WorldSettings.Name);
                LoadData();
            }
        }

        protected override void Create()
        {
            if (FullList.Count < CharacterAttribute.MAX_ATTRIBUTES)
            {
                CreateAction(Game, WorldSettings);
            }
            else
            {
                ShowAlertBox("No more than 8 attributes could be created for the world.");
            }
        }
    }
}