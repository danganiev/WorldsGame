using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WorldsGame.Utils
{
    internal class TaskHelper
    {
        internal static Task Delay(double milliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            var timer = new Timer();
            timer.Elapsed += (obj, args) => tcs.TrySetResult(true);
            timer.Interval = milliseconds;
            timer.AutoReset = false;
            timer.Start();
            return tcs.Task;
        }
    }
}
