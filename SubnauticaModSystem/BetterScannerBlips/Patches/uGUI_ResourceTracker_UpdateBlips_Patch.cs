using Common.Mod;
using HarmonyLib;
#if SN1
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace BetterScannerBlips.Patches
{
	[HarmonyPatch(typeof(uGUI_ResourceTracker))]
	internal class uGUI_ResourceTracker_UpdateBlips_Patch
	{
		private static readonly FieldInfo uGUI_ResourceTracker_blips = typeof(uGUI_ResourceTracker).GetField("blips", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo uGUI_ResourceTracker_nodes = typeof(uGUI_ResourceTracker).GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
		private static FieldInfo Blip_gameObject;

		private static bool hide = false;

		[HarmonyPrefix]
		[HarmonyPatch("UpdateBlips")]
		private static bool Prefix(uGUI_ResourceTracker __instance)
		{
			if (__instance != null && __instance.blip != null)
			{
				if (__instance.blip.GetComponent<CustomBlip>() == null)
				{
					Logger.Log("Adding CustomBlip to blip prefab");
					__instance.blip.AddComponent<CustomBlip>();
				}
			}

			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch("UpdateBlips")]
		private static void Postfix(uGUI_ResourceTracker __instance)
		{
			if (Blip_gameObject == null)
			{
				Type BlipT = typeof(uGUI_ResourceTracker).GetNestedType("Blip", BindingFlags.NonPublic);
				Blip_gameObject = BlipT.GetField("gameObject", BindingFlags.Public | BindingFlags.Instance);
			}

			if (Input.GetKeyDown(Mod.config.ToggleKey))
			{
				hide = !hide;
				ErrorMessage.AddDebug(string.Format("Scanner Blips Toggled: {0}", hide ? $"OFF (Press {Mod.config.ToggleKey} to show)" : "ON"));
			}

#if SN1
			HashSet<ResourceTracker.ResourceInfo> nodes = (HashSet<ResourceTracker.ResourceInfo>)uGUI_ResourceTracker_nodes.GetValue(__instance); 
#elif BELOWZERO
			HashSet<ResourceTrackerDatabase.ResourceInfo> nodes = (HashSet<ResourceTrackerDatabase.ResourceInfo>)uGUI_ResourceTracker_nodes.GetValue(__instance);
#endif
			IList blips = (IList)uGUI_ResourceTracker_blips.GetValue(__instance);

			Camera camera = MainCamera.camera;
			Vector3 position = camera.transform.position;
			Vector3 forward = camera.transform.forward;
			int i = 0;
#if SN1
			foreach (ResourceTracker.ResourceInfo resourceInfo in nodes)
#elif BELOWZERO
			foreach (ResourceTrackerDatabase.ResourceInfo resourceInfo in nodes)
#endif
			{
				Vector3 lhs = resourceInfo.position - position;
				if (Vector3.Dot(lhs, forward) > 0f)
				{
					var blipObject = (GameObject)Blip_gameObject.GetValue(blips[i]);
					var customBlip = blipObject.GetComponent<CustomBlip>();

					customBlip.Refresh(resourceInfo);

					i++;
				}
			}

			for (var j = 0; j < blips.Count; ++j)
			{
				var blipObject = (GameObject)Blip_gameObject.GetValue(blips[j]);
				if (hide)
				{
					blipObject.SetActive(false);
				}
			}
		}
	}
}