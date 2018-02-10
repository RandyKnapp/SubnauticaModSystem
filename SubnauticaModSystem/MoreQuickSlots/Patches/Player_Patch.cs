using Harmony;
using UnityEngine;

namespace MoreQuickSlots.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetQuickSlotKeyDown")]
    class Player_GetQuickSlotKeyDown_Patch
    {
        public static void PostFix(Player __instance, ref bool __result, ref int slotID)
        {
            if (!__result && slotID >= Player.quickSlotButtonsCount && slotID < Config.SlotCount)
            {
                __result = Input.GetKeyDown(KeyCode.Alpha0 + slotID);
            }
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetQuickSlotKeyHeld")]
    class Player_GetQuickSlotKeyHeld_Patch
    {
        public static void PostFix(Player __instance, ref bool __result, ref int slotID)
        {
            if (!__result && slotID >= Player.quickSlotButtonsCount && slotID < Config.SlotCount)
            {
                __result = Input.GetKey(KeyCode.Alpha0 + slotID);
            }
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetQuickSlotKeyUp")]
    class Player_GetQuickSlotKeyUp_Patch
    {
        public static void PostFix(Player __instance, ref bool __result, ref int slotID)
        {
            if (!__result && slotID >= Player.quickSlotButtonsCount && slotID < Config.SlotCount)
            {
                __result = Input.GetKeyUp(KeyCode.Alpha0 + slotID);
            }
        }
    }
}
