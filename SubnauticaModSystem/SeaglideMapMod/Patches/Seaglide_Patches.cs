using Common.Mod;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SeaglideMapControls.Patches
{
	[HarmonyPatch(typeof(PlayerTool))]
	[HarmonyPatch("GetCustomUseText")]
	class PlayerTool_GetCustomUseText_Patch
	{
		private static bool Prefix(PlayerTool __instance, ref string __result)
		{
			if (__instance is Seaglide)
			{
				var seaglide = __instance as Seaglide;
				string altKey = GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary);
				bool mapShowing = seaglide.toggleLights.lightState != 2;
				__result = string.Format("{0} Map (<color=#ADF8FFFF>{1}</color>)", mapShowing ? "Hide" : "Show", altKey);
				return false;
			}
			else if (Mod.config.FixScannerToolTextBug && __instance is ScannerTool)
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
				var seaglide = __instance as Seaglide;
				var lightState = seaglide.toggleLights.lightState;
				if (lightState == 2)
				{
					seaglide.toggleLights.lightState = 0;
				}
				else
				{
					seaglide.toggleLights.lightState = 2;
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Seaglide))]
	[HarmonyPatch("Start")]
	class Seaglide_Start_Patch
	{
		private static bool Prefix(Seaglide __instance)
		{
			__instance.toggleLights.maxLightStates = 1;
			__instance.toggleLights.lightState = Mod.config.SeaglideMapStartOn ? 0 : 2;
			return true;
		}
	}

	[HarmonyPatch(typeof(ToggleLights))]
	[HarmonyPatch("CheckLightToggle")]
	class ToggleLights_CheckLightToggle_Patch
	{
		private static readonly FieldInfo ToggleLights_timeLastPlayerModeChange = typeof(ToggleLights).GetField("timeLastPlayerModeChange", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo ToggleLights_timeLastLightToggle = typeof(ToggleLights).GetField("timeLastLightToggle", BindingFlags.NonPublic | BindingFlags.Instance);

		private static bool Prefix(ToggleLights __instance)
		{
			float timeLastPlayerModeChange = (float)ToggleLights_timeLastPlayerModeChange.GetValue(__instance);
			float timeLastLightToggle = (float)ToggleLights_timeLastLightToggle.GetValue(__instance);
			if (Player.main.GetRightHandDown() && !Player.main.GetPDA().isInUse && Time.time > timeLastPlayerModeChange + 1f && timeLastLightToggle + 0.25f < Time.time)
			{
				__instance.SetLightsActive(!__instance.lightsActive);
				ToggleLights_timeLastLightToggle.SetValue(__instance, Time.time);
			}
			return true;
		}
	}
}
 