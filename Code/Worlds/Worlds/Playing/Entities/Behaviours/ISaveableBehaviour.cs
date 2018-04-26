using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Entities
{
    public interface ISaveableBehaviour: IEntityBehaviour
    {
        string GetJSONForSave(Entity entity);
    }

    /// <summary>
    /// General saveable behaviour, doesn't save by default
    /// </summary>
    internal class SaveableBehaviour : EntityBehaviour
    {
        public override bool IsDrawable
        {
            get { return false; }
        }

        internal virtual string GetJSONForSave(Entity entity)
        {
            return null;
        }

        internal virtual Entity LoadFromJSON(EntityWorld entityWorld, string json)
        {
            return null;
        }
    }
}