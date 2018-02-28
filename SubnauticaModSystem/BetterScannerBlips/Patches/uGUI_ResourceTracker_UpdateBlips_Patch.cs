using Common.Mod;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BetterScannerBlips.Patches
{
	[HarmonyPatch(typeof(uGUI_ResourceTracker))]
	[HarmonyPatch("UpdateBlips")]
	class uGUI_ResourceTracker_UpdateBlips_Patch
	{
		private static readonly FieldInfo uGUI_ResourceTracker_blips = typeof(uGUI_ResourceTracker).GetField("blips", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo uGUI_ResourceTracker_nodes = typeof(uGUI_ResourceTracker).GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
		private static FieldInfo Blip_gameObject;

		private static bool hide = false;

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
				Logger.Log("Toggle " + (hide ? "Hide" : "Show"));
			}

			HashSet<ResourceTracker.ResourceInfo> nodes = (HashSet<ResourceTracker.ResourceInfo>)uGUI_ResourceTracker_nodes.GetValue(__instance);
			IList blips = (IList)uGUI_ResourceTracker_blips.GetValue(__instance);

			Camera camera = MainCamera.camera;
			Vector3 position = camera.transform.position;
			Vector3 forward = camera.transform.forward;
			int i = 0;
			foreach (ResourceTracker.ResourceInfo resourceInfo in nodes)
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