using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BlueprintTracker.Patches
{
	[HarmonyPatch(typeof(uGUI_PowerIndicator))]
	[HarmonyPatch("Initialize")]
	class uGUI_PowerIndicator_Initialize_Patch
	{
		private static BlueprintTracker tracker;

		private static void Postfix(uGUI_PowerIndicator __instance)
		{
			if (tracker != null)
			{
				return;
			}

			if (Inventory.main == null)
			{
				return;
			}

			InitializeTracker(__instance);
		}

		private static void InitializeTracker(uGUI_PowerIndicator __instance)
		{
			if (tracker == null)
			{
				tracker = GameObject.FindObjectOfType<BlueprintTracker>();
				if (tracker == null)
				{
					CreateTracker(__instance);
				}
			}
		}

		private static void CreateTracker(uGUI_PowerIndicator __instance)
		{
			var parent = __instance.text.transform.parent;
			if (parent == null)
			{
				Logger.Error("Tried to create tracker but hud was null!");
			}

			tracker = BlueprintTracker.Create(parent);
		}
	}
}
