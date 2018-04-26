using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsGame.Saving.DataClasses.Items
{
    internal static class ItemUtils
    {
        //        internal static List<ItemType> GetSelectableItemTypes()
        //        {
        //            return new List<ItemType>
        //            {
        //                ItemType.Usable
        //            };
        //        }

        internal static List<ItemActionType> GetSelectableItemActionTypes()
        {
            return new List<ItemActionType>
            {
                ItemActionType.Nothing,
                ItemActionType.Consume,
                ItemActionType.Swing,
            };
        }
    }

    internal static class ItemEnumNaming
    {
        private static Dictionary<ItemQuality, string> _itemQualityNames;
        private static Dictionary<ItemQuality, Tuple<string, string>> _itemQualityNamesWithPopups;

        internal static Dictionary<ItemQuality, string> GetItemQualityNames()
        {
            if (_itemQualityNames == null)
            {
                _itemQualityNames = new Dictionary<ItemQuality, string>
                {
                    {ItemQuality.Consumable, "Consumable"},
                    {ItemQuality.Unbreakable, "Unbreakable"},
                };
            }
            return _itemQualityNames;
        }

        internal static Dictionary<ItemQuality, Tuple<string, string>> GetItemQualityNamesWithPopups()
        {
            if (_itemQualityNamesWithPopups == null)
            {
                _itemQualityNamesWithPopups = new Dictionary<ItemQuality, Tuple<string, string>>
                {
                    {ItemQuality.Consumable, new Tuple<string, string>("Consumable", "Can only be consumed or thrown once")},
                    {ItemQuality.Unbreakable, new Tuple<string, string>("Unbreakable", "")},
                };
            }
            return _itemQualityNamesWithPopups;
        }
    }
}