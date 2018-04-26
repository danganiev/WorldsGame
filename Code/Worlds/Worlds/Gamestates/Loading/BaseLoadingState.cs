using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Gamestates
{
    internal class BaseLoadingState : WorldsGameState
    {
        private const int FAULT_SECONDS = 5;

        protected TimeSpan FaultTime { get; set; }

        internal BaseLoadingState(WorldsGame game)
            : base(game)
        {
            FaultTime = TimeSpan.MinValue;
        }

        protected virtual void ToMainMenu()
        {
        }

        protected bool CheckTaskForExceptions(GameTime gameTime, Task task)
        {
            if (FaultTime != TimeSpan.MinValue)
            {
                TimeSpan timeAfterFault = gameTime.TotalGameTime - FaultTime;

                if (timeAfterFault.Seconds < FAULT_SECONDS)
                {
                    if (task.Exception != null)
                    {
                        string message = string.Format(
                            "There was some error while loading, \n if you're using screen capture software such as FRAPS please stop recording while game loads: \n Returning to main menu in {0} seconds",
                            FAULT_SECONDS - timeAfterFault.Seconds);

                        Messenger.Invoke("LoadingMessageChange", message);
                    }
                }
                else
                {
                    task.Dispose();
                    ToMainMenu();
                }
                return false;
            }

            if (task.IsFaulted && task.Exception != null)
            {
                FaultTime = gameTime.TotalGameTime;
                return false;
            }

            return true;
        }
    }
}