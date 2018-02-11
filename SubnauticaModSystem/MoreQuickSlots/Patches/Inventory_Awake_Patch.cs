using Harmony;
using System;
using System.Reflection;

namespace MoreQuickSlots.Patches
{
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch("Awake")]
    class Inventory_Awake_Patch
    {
        static void Postfix(Inventory __instance)
        {
            string[] newSlotNames = new string[Config.SlotCount];
            for (int i = 0; i < Config.SlotCount; ++i)
            {
                newSlotNames[i] = "QuickSlot" + i;
            }
            typeof(QuickSlots).GetField("slotNames", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, newSlotNames);

            Player player = __instance.GetComponent<Player>();

            Logger.Log("Inventory Quick Slots (before): {0}", __instance.quickSlots.slotCount);

            var setQuickSlots = __instance.GetType().GetMethod("set_quickSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            setQuickSlots.Invoke(__instance, new object[]{ new QuickSlots(__instance.gameObject, __instance.toolSocket, __instance.cameraSocket, __instance, player.rightHandSlot, Config.SlotCount) });

            Logger.Log("Inventory Quick Slots (after): {0}", __instance.quickSlots.slotCount);
        }
    }
}
