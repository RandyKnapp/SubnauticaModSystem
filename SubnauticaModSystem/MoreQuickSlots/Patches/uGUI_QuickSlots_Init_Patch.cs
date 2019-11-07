using Harmony;

namespace MoreQuickSlots.Patches
{
	[HarmonyPatch(typeof(uGUI_QuickSlots))]
	[HarmonyPatch("Init")]
	class uGUI_QuickSlots_Init_Patch
	{
		private static void Postfix(uGUI_QuickSlots __instance)
		{
			Logger.Log("QuickSlots Init");
			InstantiateGameController(__instance);
		}

		private static void InstantiateGameController(uGUI_QuickSlots instance)
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
