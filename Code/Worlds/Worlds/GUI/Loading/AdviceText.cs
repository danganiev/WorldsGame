using System.Collections.Generic;
using System.Linq;

using WorldsGame.Utils.ExtensionMethods;

namespace WorldsGame.GUI.Loading
{
    internal static class AdviceText
    {
        private static List<string> _adviceStringList = new List<string>
        {
           "Adventure time! C'mon grab up your friends!",
           "Loading times are compensated by power. Or my mind is trying to comfort me so.",
           "Yes, there will be multiplayer. UPD: It's here!",
           "Space is only noise if you can see.",
           "List of these messages will grow over time.",
           "You are still a good person.",
           "You have to give Cataclysm: Dark Days Ahead a look.",
           "Still better loading times than The Sims!",
           "Enter 'help' in console to see the list of available commands.",
           "I just wanted to say hi to Minecraft here. Hi!",
           "You shouldn't believe everything they write in loading messages."
        };

        private static List<string> _usedList = new List<string>();

        public static string GetNewAdvice()
        {
            if (_usedList.Count == 0)
            {
                _adviceStringList.Shuffle();
            }
            if (_adviceStringList.Count == 0)
            {
                var list = _adviceStringList;
                _adviceStringList = _usedList;
                _usedList = list;

                _adviceStringList.Shuffle();
            }

            string advice = _adviceStringList.Last();
            _adviceStringList.RemoveAt(_adviceStringList.Count - 1);
            _usedList.Add(advice);

            return advice;
        }
    }
}