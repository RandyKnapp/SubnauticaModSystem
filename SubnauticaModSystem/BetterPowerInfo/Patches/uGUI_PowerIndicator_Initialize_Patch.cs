using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;

namespace BetterPowerInfo.Patches
{
	[HarmonyPatch(typeof(uGUI_PowerIndicator))]
	[HarmonyPatch("Initialize")]
	class uGUI_PowerIndicator_Initialize_Patch
	{
		private static FieldInfo initializedField;
		private static bool initialized;

		private static void Postfix(uGUI_PowerIndicator __instance)
		{
			bool indicatorInitialized = IsInitialized(__instance);
			if (!indicatorInitialized || __instance.text == null)
			{
				initialized = false;
				return;
			}

			if (initialized)
			{
				return;
			}

			if (__instance.gameObject.GetComponent<GameController>() == null)
			{
				__instance.gameObject.AddComponent<GameController>();
				initialized = true;
			}
		}

		private static bool IsInitialized(uGUI_PowerIndicator instance)
		{
			if (initializedField == null)
			{
				initializedField = typeof(uGUI_PowerIndicator).GetField("initialized", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return (bool)initializedField.GetValue(instance);
		}
	}
}
