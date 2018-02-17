using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlueprintTracker.Patches
{
	[HarmonyPatch(typeof(uGUI_BlueprintEntry))]
	[HarmonyPatch("Awake")]
	public static class uGUI_BlueprintEntry_Awake_Patch
	{
		private static void Postfix()
		{
			Logger.Log("uGUI_BlueprintEntry.Awake");
		}
	}

	[HarmonyPatch(typeof(uGUI_BlueprintsTab))]
	[HarmonyPatch("Awake")]
	public static class uGUI_BlueprintsTab_Awake_Patch
	{
		private static void Postfix()
		{
			Logger.Log("uGUI_BlueprintsTab.Awake");
		}
	}
}
