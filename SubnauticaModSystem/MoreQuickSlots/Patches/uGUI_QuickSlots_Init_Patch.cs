using HarmonyLib;

namespace MoreQuickSlots.Patches
{
	[HarmonyPatch(typeof(uGUI_QuickSlots), nameof(uGUI_QuickSlots.Init))]
	public static class uGUI_QuickSlots_Init_Patch
	{
		public static void Postfix(uGUI_QuickSlots __instance)
		{
			InstantiateGameController(__instance);
		}

        public static void InstantiateGameController(uGUI_QuickSlots instance)
		{
			var controller = instance.gameObject.GetComponent<GameController>();
			if (controller == null)
			{
				controller = instance.gameObject.AddComponent<GameController>();
			}
			controller.AddHotkeyLabels(instance);
		}
	}
}
