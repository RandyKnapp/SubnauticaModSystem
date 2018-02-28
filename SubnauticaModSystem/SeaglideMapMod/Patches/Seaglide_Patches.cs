using Common.Mod;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SeaglideMapMod.Patches
{
	[HarmonyPatch(typeof(PlayerTool))]
	[HarmonyPatch("GetCustomUseText")]
	class PlayerTool_GetCustomUseText_Patch
	{
		private static bool Prefix(PlayerTool __instance, ref string __result)
		{
			if (__instance is Seaglide)
			{
				string altKey = GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary);
				__result = string.Format("Toggle Map (<color=#ADF8FFFF>{0}</color>)", altKey);
				return false;
			}
			else if (Mod.config.FixeScannerToolTextBug && __instance is ScannerTool)
			{
				__result = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PlayerTool))]
	[HarmonyPatch("OnAltDown")]
	class PlayerTool_OnAltDown_Patch
	{
		private static bool Prefix(PlayerTool __instance)
		{
			if (__instance is Seaglide)
			{
				Logger.Log("OnAltDown Seaglide");
			}
			return true;
		}
	}
}