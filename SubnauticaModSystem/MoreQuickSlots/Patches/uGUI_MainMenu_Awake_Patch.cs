using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoreQuickSlots.Patches
{
    [HarmonyPatch(typeof(uGUI_MainMenu))]
    [HarmonyPatch("Awake")]
    class uGUI_MainMenu_Awake_Patch
    {
        static void PostFix(uGUI_MainMenu __instance)
        {
            Logger.Log("MainMenu.Awake PostFix");
        }
    }
}
