using Harmony;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace MoreQuickSlots.Patches
{
	[HarmonyPatch(typeof(uGUI_QuickSlots))]
	[HarmonyPatch("Init")]
	class uGUI_QuickSlots_Init_Patch
	{
		private static void Postfix(uGUI_QuickSlots __instance)
		{
			InstantiateGameController(__instance);
		}

		private static void InstantiateGameController(uGUI_QuickSlots instance)
		{
			if (instance.gameObject.GetComponent<GameController>() == null)
			{
				instance.gameObject.AddComponent<GameController>();
			}
		}
	}
}
