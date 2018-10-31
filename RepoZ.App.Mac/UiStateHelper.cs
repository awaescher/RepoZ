using System;
namespace RepoZ.App.Mac
{
    public static class UiStateHelper
    {
        public static bool CommandKeyDown { get; set; } 

        public static bool OptionKeyDown { get; set; } 

        public static bool ControlKeyDown { get; set; } 

        public static bool ShiftKeyDown { get; set; }

        public static void Reset()
        {
            CommandKeyDown = false;
            OptionKeyDown = false;
            ControlKeyDown = false;
            ShiftKeyDown = false;
        }
    }
}
