using WorldsGame.Playing.Entities;

namespace WorldsGame.Playing.NPCs
{
    internal class NPCComponent : IEntityComponent
    {
        internal string CharacterModelName { get; set; }

        internal NPCComponent(string characterModelName)
        {
            CharacterModelName = characterModelName;
        }

        public void Dispose()
        {
        }
    }
}