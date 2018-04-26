using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Entities.Components
{
    internal class CustomModelComponent : IEntityComponent
    {
        internal Matrix AdditionalTranslationMatrix { get; set; }

        //        internal Matrix AdditionalRotationMatrix { get; set; }

        //        internal bool IsFirstPersonItemModel { get; set; }

        internal CustomModelComponent()
        {
            AdditionalTranslationMatrix = Matrix.Identity;
            //            IsFirstPersonItemModel = false;
        }

        public void Dispose()
        {
        }
    }
}