using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;

namespace WorldsGame.Playing.Terrain.Worlds.Time
{
    internal abstract class BaseTimeUpdater : IUpdateable
    {
        internal const long TIME_PASSING_SPEED_DIVIDER = 20000;

        protected bool isRealTime;

        internal float TimeOfDay { get; set; }

        internal bool IsDayMode { get; set; }

        internal bool IsNightMode { get; set; }

        internal short TextureAnimationFrame { get; set; }

        internal virtual Color CurrentAtmosphereColor { get { return Color.White; } }

        internal virtual int PreviousHour
        {
            get { return 0; }
        }

        internal virtual int NextHour
        {
            get { return 0; }
        }

        internal virtual Color NextAtmosphereColor
        {
            get { return Color.White; }
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Update1000(GameTime gameTime)
        {
        }

        internal virtual void Initialize(CompiledGameBundle gameBundle)
        {
        }
    }
}