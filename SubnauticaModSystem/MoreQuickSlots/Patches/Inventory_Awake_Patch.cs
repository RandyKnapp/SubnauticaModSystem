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
            int slotCount = Mod.config.SlotCount;

            string[] newSlotNames = new string[slotCount];
            for (int i = 0; i < slotCount; ++i)
            {
                newSlotNames[i] = "QuickSlot" + i;
            }
            typeof(QuickSlots).GetField("slotNames", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, newSlotNames);

            Player player = __instance.GetComponent<Player>();
            QuickSlots newQuickSlots = new QuickSlots(__instance.gameObject, __instance.toolSocket, __instance.cameraSocket, __instance, player.rightHandSlot, slotCount);

            var setQuickSlots = __instance.GetType().GetMethod("set_quickSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            setQuickSlots.Invoke(__instance, new object[]{ newQuickSlots });

            Logger.Log("Inventory Quick Modified, new slot count: {0}", __instance.quickSlots.slotCount);
        }
    }
}
