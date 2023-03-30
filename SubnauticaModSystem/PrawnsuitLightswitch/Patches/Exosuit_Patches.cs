using Common.Mod;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PrawnsuitLightswitch.Patches
{
	class Exosuit_ToggleLights
	{
		private static GameObject seamoth;
		private static IEnumerator SetupToggle(Exosuit __instance)
		{
			Logger.Log("Setup Lights Toggle Start");
			CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Seamoth);
			yield return request;
			seamoth = request.GetResult();
			if (seamoth is null)
			{
				Logger.Log("Couldn't get seamoth prefab");
				yield break;
			}

			var toggleLights = __instance.GetComponent<ToggleLights>();
			var toggleLightsPrefab = seamoth.GetComponent<SeaMoth>().toggleLights;

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
			Logger.Log("Setup Lights Toggle Finish");
		}

		[HarmonyPatch(typeof(Exosuit))]
		[HarmonyPatch("Awake")]
		class Exosuit_Awake_Patch
		{
			private static void Postfix(Exosuit __instance)
			{
				Logger.Log("Exosuit Awake");
				__instance.StartCoroutine(SetupToggle(__instance));
			}
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

				if (__instance.GetPilotingMode() && GameInput.GetButtonDown(Exosuit_UpdateUIText_Patch.lightsBinding) && !(Player.main.GetPDA().isOpen || !AvatarInputHandler.main.IsEnabled()))
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
		private static readonly FieldInfo Exosuit_rightArm = typeof(Exosuit).GetField("rightArm", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo Exosuit_leftArm = typeof(Exosuit).GetField("leftArm", BindingFlags.NonPublic | BindingFlags.Instance);
        public static GameInput.Button lightsBinding;

		private static void Postfix(Exosuit __instance)
		{
            bool hasPropCannon = Exosuit_rightArm.GetValue(__instance) is ExosuitPropulsionArm || Exosuit_leftArm.GetValue(__instance) is ExosuitPropulsionArm;
            var toggleLights = __instance.GetComponent<ToggleLights>();
			if (toggleLights != null)
			{
				string lightsString = LanguageCache.GetButtonFormat((!toggleLights.lightsActive) ? "Lights On (<color=#ADF8FFFF>{0}</color>)" : "Lights Off (<color=#ADF8FFFF>{0}</color>)", lightsBinding);
				string exitString = string.Join("\n", ((string)Exosuit_uiStringPrimary.GetValue(__instance)).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Take(1).ToArray());
				var primaryString = string.Join("\n", ((string)Exosuit_uiStringPrimary.GetValue(__instance)).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray()) + System.Environment.NewLine + lightsString;
				var secondaryString = string.Empty;
				if (hasPropCannon)
				{
					lightsBinding = GameInput.Button.Deconstruct;
					secondaryString = exitString;
				}
				else
				{
					lightsBinding = GameInput.Button.AltTool;
					primaryString = primaryString + System.Environment.NewLine + exitString;
				}
				HandReticle.main.SetTextRaw(HandReticle.TextType.Use, primaryString);
				HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, secondaryString);
			}
		}
	}
}
