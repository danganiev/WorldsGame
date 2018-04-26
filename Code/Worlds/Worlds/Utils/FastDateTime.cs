﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsGame.Utils
{
    /// <summary>
    /// <para>The fast date time.</para>
    /// <para>The standard DateTime.Now call produces more garbage</para>
    /// <para>and is slower. For Windows NT 3.5 and later the timer</para>
    /// <para>resolution is approximately 10 milliseconds.</para>
    /// </summary>
    public static class FastDateTime
    {
        /// <summary>The local UTC offset.</summary>
#if !PORTABLE
        private static readonly TimeSpan LocalUtcOffset = TimeZoneInfo.Utc.GetUtcOffset(DateTime.Now);
#else
        private static readonly TimeSpan LocalUtcOffset = DateTime.UtcNow - DateTime.Now;
#endif

        /// <summary>Gets the now.</summary>
        public static DateTime Now
        {
            get { return DateTime.UtcNow + LocalUtcOffset; }
        }

        /// <summary>Gets the micro seconds from ticks.</summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns>The <c>µs</c> as System.Double.</returns>
        public static double GetMicroSeconds(long ticks)
        {
            return ticks * 0.1;
        }

        /// <summary>Gets the nano seconds.</summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns>The <c>ns</c> as System.Double.</returns>
        public static double GetNanoSeconds(long ticks)
        {
            return ticks * 100.0;
        }

        /// <summary>Returns a <see cref="string" /> that formats a time span.</summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>A <see cref="string" /> that formats a time span.</returns>
        public static string ToString(TimeSpan timeSpan)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds * 0.1);
        }

        /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public static new string ToString()
        {
            return Now.ToString("HH:mm:ss.ffffff");
        }
    }
}