using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsGame.Utils
{
    internal static class RandomNumberGenerator
    {
        internal static readonly MersenneTwister MERSENNE_TWISTER = new MersenneTwister((ulong)Environment.TickCount);
        internal static readonly Random RANDOM = new Random();

        /// <summary>
        /// Generates a random integer number from 0 to N-1
        /// </summary>
        /// <param name="maxInt"></param>
        /// <returns></returns>
        internal static int GetInt(int maxInt)
        {
            return MERSENNE_TWISTER.GetInt(maxInt);
        }

        internal static int GetInt(int minInt, int maxInt)
        {
            return RANDOM.Next(minInt, maxInt);            
        }

        /// <summary>
        /// Generates a random floating point number on [0,1]
        /// </summary>
        /// <returns></returns>
        internal static float GetFloat()
        {
            return (float)MERSENNE_TWISTER.GetDouble();
        }

        internal static int Next(int minInt, int maxInt)
        {
            return GetInt(minInt, minInt);
        }

        /// <summary>
        /// Checks if an event happened with set probability
        /// </summary>
        /// <param name="probability"></param>
        /// <returns></returns>
        internal static bool CheckProbabilityOnce(float probability)
        {
            return GetFloat() * 100 <= probability;
        }
    }
}