using Microsoft.Xna.Framework;

namespace WorldsGame.Saving.DataClasses
{
    public interface IItemLike
    {
        string Name { get; set; }

        string Description { get; }

        Color[] IconColors { get; }
    }
}