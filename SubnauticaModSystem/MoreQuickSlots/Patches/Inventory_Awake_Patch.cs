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
            /*typeof(QuickSlots).GetField("slotNames", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, new string[]
            {
                "QuickSlot0",
                "QuickSlot1",
                "QuickSlot2",
                "QuickSlot3",
                "QuickSlot4",
                "QuickSlot5",
                "QuickSlot6",
                "QuickSlot7",
                "QuickSlot8",
                "QuickSlot9"
            });*/

            Player player = __instance.GetComponent<Player>();

            Console.WriteLine("[MoreQuickSlots] Inventory Quick Slots (before): {0}", __instance.quickSlots.slotCount);

            var quickSlots = __instance.GetType().GetField("quickSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Console.WriteLine("[MoreQuickSlots] quickSlots = {0}", quickSlots);
            quickSlots.SetValue(__instance, new QuickSlots(__instance.gameObject, __instance.toolSocket, __instance.cameraSocket, __instance, player.rightHandSlot, Player.quickSlotButtonsCount));

            Console.WriteLine("[MoreQuickSlots] Inventory Quick Slots (after): {0}", __instance.quickSlots.slotCount);
        }
    }
}
