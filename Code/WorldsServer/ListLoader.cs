using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WorldsGame.Saving;

namespace WorldsServer
{
    internal static class ListLoader
    {
        internal static void LoadList<T>(ListBox list, SaverHelper<T> saverHelper, string preselectedValue = "",
            List<string> extensions = null, List<string> additionalItems = null) where T : class, ISaveDataSerializable<T>
        {
            if (extensions == null)
            {
                extensions = new List<string> { "sav" };
            }
            list.Items.Clear();

            if (additionalItems != null)
            {
                foreach (string additionalItem in additionalItems)
                {
                    AddItem(list, preselectedValue, additionalItem);
                }
            }

            foreach (var listFileName in saverHelper.LoadNames())
            {
                var listNameArray = listFileName.Split('.').ToList();

                if (!extensions.Contains(listNameArray.Last()))
                {
                    continue;
                }

                var listNameQuery = listNameArray.Take(listNameArray.Count - 1);
                string value = string.Join(".", listNameQuery);

                AddItem(list, preselectedValue, value);
            }
        }

        private static void AddItem(ListBox list, string preselectedValue, string item)            
        {
            list.Items.Add(item);

            if (item == preselectedValue)
            {
                int index = list.Items.Count - 1;
                list.SelectedItems.Add(index);
            }
        }
    }
}
