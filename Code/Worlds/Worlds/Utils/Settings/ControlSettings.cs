using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Input;

namespace WorldsGame.Utils.Settings
{
    public enum PossiblePlayerContiniousActions
    {
        MoveForward,
        MoveBack,
        StrafeLeft,
        StrafeRight,
    }

    public enum PossiblePlayerSingleActions
    {
        Jump,
        ChangeCameraView,
        PrimaryAction,
        SecondaryAction,
        Say,
        InventoryToggle,
        InventoryItem1,
        InventoryItem2,
        InventoryItem3,
        InventoryItem4,
        InventoryItem5,
        InventoryItem6,
        InventoryItem7,
        InventoryItem8,
        InventoryItem9,
        InventoryItem10,
    }

    public enum AllPlayerActions
    {
        MoveForward,
        MoveBack,
        StrafeLeft,
        StrafeRight,
        Jump,
        ChangeCameraView,
        PrimaryAction,
        SecondaryAction,
        Say,
        InventoryToggle,
        InventoryItem1,
        InventoryItem2,
        InventoryItem3,
        InventoryItem4,
        InventoryItem5,
        InventoryItem6,
        InventoryItem7,
        InventoryItem8,
        InventoryItem9,
        InventoryItem10,
    }

    public class ControlSettings
    {
        public static Dictionary<string, Keys> KeyMap = new Dictionary<string, Keys>();

        // This one is used for nice names
        public static Dictionary<Keys, string> ReverseKeyMap = new Dictionary<Keys, string>();

        public string MoveForward;
        public string MoveBack;
        public string StrafeLeft;
        public string StrafeRight;
        public string Jump;
        public string ChangeCameraView;

        public string PrimaryAction;
        public string SecondaryAction;
        public string Say;
        public string Inventory;

        public string InventoryItem1;
        public string InventoryItem2;
        public string InventoryItem3;
        public string InventoryItem4;
        public string InventoryItem5;
        public string InventoryItem6;
        public string InventoryItem7;
        public string InventoryItem8;
        public string InventoryItem9;
        public string InventoryItem10;

        // These react both to pressing and releasing
        public Dictionary<Keys, PossiblePlayerContiniousActions> GetContiniousActionsKeys()
        {
            return new Dictionary<Keys, PossiblePlayerContiniousActions>
                   {
                       {KeyMap[MoveForward], PossiblePlayerContiniousActions.MoveForward},
                       {KeyMap[MoveBack], PossiblePlayerContiniousActions.MoveBack},
                       {KeyMap[StrafeLeft], PossiblePlayerContiniousActions.StrafeLeft},
                       {KeyMap[StrafeRight], PossiblePlayerContiniousActions.StrafeRight},
                   };
        }

        // These react only to pressing only once
        public Dictionary<Keys, PossiblePlayerSingleActions> GetSingleActionsKeys()
        {
            return new Dictionary<Keys, PossiblePlayerSingleActions>
                   {
                       {KeyMap[Jump], PossiblePlayerSingleActions.Jump},
                       {KeyMap[ChangeCameraView], PossiblePlayerSingleActions.ChangeCameraView},
                       {KeyMap[PrimaryAction], PossiblePlayerSingleActions.PrimaryAction},
                       {KeyMap[SecondaryAction], PossiblePlayerSingleActions.SecondaryAction},
                       {KeyMap[Say], PossiblePlayerSingleActions.Say},
                       {KeyMap[Inventory], PossiblePlayerSingleActions.InventoryToggle},
                       {KeyMap[InventoryItem1], PossiblePlayerSingleActions.InventoryItem1},
                       {KeyMap[InventoryItem2], PossiblePlayerSingleActions.InventoryItem2},
                       {KeyMap[InventoryItem3], PossiblePlayerSingleActions.InventoryItem3},
                       {KeyMap[InventoryItem4], PossiblePlayerSingleActions.InventoryItem4},
                       {KeyMap[InventoryItem5], PossiblePlayerSingleActions.InventoryItem5},
                       {KeyMap[InventoryItem6], PossiblePlayerSingleActions.InventoryItem6},
                       {KeyMap[InventoryItem7], PossiblePlayerSingleActions.InventoryItem7},
                       {KeyMap[InventoryItem8], PossiblePlayerSingleActions.InventoryItem8},
                       {KeyMap[InventoryItem9], PossiblePlayerSingleActions.InventoryItem9},
                       {KeyMap[InventoryItem10], PossiblePlayerSingleActions.InventoryItem10},
                   };
        }

        public Dictionary<AllPlayerActions, Keys> GetAllActions()
        {
            return new Dictionary<AllPlayerActions, Keys>
                   {
                       {AllPlayerActions.MoveForward, KeyMap[MoveForward]},
                       {AllPlayerActions.MoveBack, KeyMap[MoveBack]},
                       {AllPlayerActions.StrafeLeft, KeyMap[StrafeLeft]},
                       {AllPlayerActions.StrafeRight, KeyMap[StrafeRight]},
                       {AllPlayerActions.Jump, KeyMap[Jump]},
                       {AllPlayerActions.ChangeCameraView, KeyMap[ChangeCameraView]},
                       {AllPlayerActions.PrimaryAction, KeyMap[PrimaryAction]},
                       {AllPlayerActions.SecondaryAction, KeyMap[SecondaryAction]},
                       {AllPlayerActions.Say, KeyMap[Say]},
                       {AllPlayerActions.InventoryToggle, KeyMap[Inventory]},
                       {AllPlayerActions.InventoryItem1, KeyMap[InventoryItem1]},
                       {AllPlayerActions.InventoryItem2, KeyMap[InventoryItem2]},
                       {AllPlayerActions.InventoryItem3, KeyMap[InventoryItem3]},
                       {AllPlayerActions.InventoryItem4, KeyMap[InventoryItem4]},
                       {AllPlayerActions.InventoryItem5, KeyMap[InventoryItem5]},
                       {AllPlayerActions.InventoryItem6, KeyMap[InventoryItem6]},
                       {AllPlayerActions.InventoryItem7, KeyMap[InventoryItem7]},
                       {AllPlayerActions.InventoryItem8, KeyMap[InventoryItem8]},
                       {AllPlayerActions.InventoryItem9, KeyMap[InventoryItem9]},
                       {AllPlayerActions.InventoryItem10, KeyMap[InventoryItem10]},
                   };
        }

        static ControlSettings()
        {
            FillKeyMap();
            FillReverseKeyMap();
        }

        public ControlSettings()
        {
            // Create our default settings
            MoveForward = "W";
            MoveBack = "S";
            StrafeLeft = "A";
            StrafeRight = "D";
            Jump = "Space";

            ChangeCameraView = "V";

            PrimaryAction = "LeftClick";
            SecondaryAction = "RightClick";
            Say = "T";
            Inventory = "I";

            InventoryItem1 = "D1";
            InventoryItem2 = "D2";
            InventoryItem3 = "D3";
            InventoryItem4 = "D4";
            InventoryItem5 = "D5";
            InventoryItem6 = "D6";
            InventoryItem7 = "D7";
            InventoryItem8 = "D8";
            InventoryItem9 = "D9";
            InventoryItem10 = "D0";
        }

        private static void FillKeyMap()
        {
            KeyMap.Clear();

            foreach (var key in Enum.GetValues(typeof(Keys)))
            {
                KeyMap.Add(key.ToString(), (Keys)key);
            }

            // We use the unused to mark mouse events
            KeyMap.Add("LeftClick", Keys.F22);
            KeyMap.Add("RightClick", Keys.F23);
            KeyMap.Add("MiddleClick", Keys.F24);
        }

        private static void FillReverseKeyMap()
        {
            ReverseKeyMap.Clear();

            foreach (var key in Enum.GetValues(typeof(Keys)))
            {
                ReverseKeyMap.Add((Keys)key, key.ToString());
            }

            ReverseKeyMap[Keys.OemTilde] = "`";
            ReverseKeyMap[Keys.D0] = "0";
            ReverseKeyMap[Keys.D1] = "1";
            ReverseKeyMap[Keys.D2] = "2";
            ReverseKeyMap[Keys.D3] = "3";
            ReverseKeyMap[Keys.D4] = "4";
            ReverseKeyMap[Keys.D5] = "5";
            ReverseKeyMap[Keys.D6] = "6";
            ReverseKeyMap[Keys.D7] = "7";
            ReverseKeyMap[Keys.D8] = "8";
            ReverseKeyMap[Keys.D9] = "9";
            ReverseKeyMap[Keys.F22] = "LeftClick";
            ReverseKeyMap[Keys.F23] = "RightClick";
            ReverseKeyMap[Keys.F24] = "MiddleClick";
        }
    }
}