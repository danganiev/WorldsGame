using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;

namespace WorldsGame.Playing.Terrain.Worlds.Time
{
    internal class SimpleTimeUpdater : BaseTimeUpdater
    {
        private Dictionary<int, Color> _colorsPerTimeOfDay;

        private Color _currentAtmosphereColor;
        private Color _nextAtmosphereColor;
        private int _currentHour;
        private int _nextHour;
        private int _previousHour;

        // This is clunky and bad design but whatever
        internal override Color CurrentAtmosphereColor
        {
            get
            {
                if ((int)TimeOfDay == _currentHour)
                {
                    return _currentAtmosphereColor;
                }
                CalculateColorStuff();

                return _colorsPerTimeOfDay[_previousHour];
            }
        }

        internal override int PreviousHour
        {
            get { return _previousHour; }
        }

        internal override int NextHour
        {
            get { return _nextHour; }
        }

        internal override Color NextAtmosphereColor
        {
            get { return _nextAtmosphereColor; }
        }

        internal void CalculateColorStuff()
        {            
            int intTimeOfDay = (int) TimeOfDay;
            int i = intTimeOfDay;
            bool foundCurrent = false;
            bool foundPrevious = false;
            bool fullLoop = false;
            while (i <= 24)
            {
                if (_colorsPerTimeOfDay.ContainsKey(i))
                {
                    if (!foundCurrent)
                    {
                        if (i == intTimeOfDay)
                        {
                            _previousHour = i;
                            foundPrevious = true;
                        }
                        else
                        {
                            foundCurrent = true;
                            _nextHour = i;
                        }

                    }
                    if (!foundPrevious)
                    {
                        _previousHour = i;
                    }
                }

                i++;
                if (i > 24)
                {
                    i = 0;
                }
                if (i == intTimeOfDay && !fullLoop)
                {
                    fullLoop = true;
                }
                else if (i == intTimeOfDay && fullLoop)
                {
                    break;
                }
            }

            _currentHour = intTimeOfDay;
            _nextAtmosphereColor = _colorsPerTimeOfDay[_nextHour];
            _currentAtmosphereColor = _colorsPerTimeOfDay[_previousHour];
        }

        internal override void Initialize(CompiledGameBundle gameBundle)
        {
            _colorsPerTimeOfDay = gameBundle.ColorsPerTimeOfDay;
        }

        public override void Update(GameTime gameTime)
        {
            // TODO: Transform into class when it grows.
            if (!isRealTime)
                TimeOfDay += ((float)gameTime.ElapsedGameTime.Milliseconds / TIME_PASSING_SPEED_DIVIDER) * 20;
            else
                TimeOfDay = DateTime.Now.Hour + ((float)DateTime.Now.Minute) / 60 + (((float)DateTime.Now.Second) / 60) / 60;

            if (TimeOfDay >= 24)
                TimeOfDay = 0;

            if (IsDayMode)
            {
                TimeOfDay = 12;
                IsNightMode = false;
            }
            else if (IsNightMode)
            {
                TimeOfDay = 0;
                IsDayMode = false;
            }

            // Calculate the position of the sun based on the time of day.
            //            float x = 0;
            //            float y = 0;
            //            float z = 0;
            //
            //            if (TimeOfDay <= 12)
            //            {
            //                y = TimeOfDay / 12;
            //                x = 12 - TimeOfDay;
            //            }
            //            else
            //            {
            //                y = (24 - TimeOfDay) / 12;
            //                x = 12 - TimeOfDay;
            //            }
            //
            //            x /= 10;
            //
            //            SunPos = new Vector3(-x, y, z);
            //
            //            return SunPos;
        }

        public override void Update1000(GameTime gameTime)
        {
            TextureAnimationFrame += 1;

            if (TextureAnimationFrame == CompiledTexture.ANIMATED_FRAME_COUNT)
            {
                TextureAnimationFrame = 0;
            }
        }
    }
}