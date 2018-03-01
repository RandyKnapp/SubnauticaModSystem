using Common.Mod;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PrawnsuitLightswitch.Patches
{
	[HarmonyPatch(typeof(Exosuit))]
	[HarmonyPatch("Awake")]
	class Exosuit_Awake_Patch
	{
		private static void Postfix(Exosuit __instance)
		{
			Logger.Log("Exosuit Awake");
			var toggleLights = __instance.GetComponent<ToggleLights>();
			var seamothPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth");
			var toggleLightsPrefab = seamothPrefab.GetComponent<SeaMoth>().toggleLights;

			if (toggleLights == null)
			{
				toggleLights = ModUtils.CopyComponent(toggleLightsPrefab, __instance.gameObject);
			}
			else
			{
				toggleLights.lightsOnSound = toggleLightsPrefab.lightsOnSound;
				toggleLights.lightsOffSound = toggleLightsPrefab.lightsOffSound;
				toggleLights.onSound = toggleLightsPrefab.onSound;
				toggleLights.offSound = toggleLightsPrefab.offSound;
				toggleLights.energyPerSecond = toggleLightsPrefab.energyPerSecond;
			}
			
			toggleLights.lightsParent = __instance.transform.Find("lights_parent").gameObject;
			toggleLights.energyMixin = __instance.GetComponent<EnergyMixin>();
		}
	}

	[HarmonyPatch(typeof(Exosuit))]
	[HarmonyPatch("Update")]
	class Exosuit_Update_Patch
	{
		private static void Postfix(Exosuit __instance)
		{
			var toggleLights = __instance.GetComponent<ToggleLights>();
			if (toggleLights != null)
			{
				if (Mod.config.PrawnsuitLightsUseEnergy)
				{
					toggleLights.UpdateLightEnergy();
				}

				if (__instance.GetPilotingMode() && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
				{
					toggleLights.SetLightsActive(!toggleLights.lightsActive);
				}
			}
		}
	}

	[HarmonyPatch(typeof(Exosuit))]
	[HarmonyPatch("UpdateUIText")]
	class Exosuit_UpdateUIText_Patch
	{
		private static readonly FieldInfo Exosuit_uiStringPrimary = typeof(Exosuit).GetField("uiStringPrimary", BindingFlags.NonPublic | BindingFlags.Instance);

		private static void Postfix(Exosuit __instance)
		{
			var primaryString = (string)Exosuit_uiStringPrimary.GetValue(__instance);
			var keyName = GameInput.GetBindingName(GameInput.Button.Deconstruct, GameInput.BindingSet.Primary);
			var secondaryString = string.Format("Toggle Lights (<color=#ADF8FFFF>{0}</color>)", keyName);
			HandReticle.main.SetUseTextRaw(primaryString, secondaryString);
		}
	}
}
