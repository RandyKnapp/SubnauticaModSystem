using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HudConfig.Patches
{
	[HarmonyPatch(typeof(uGUI_SceneHUD))]
	[HarmonyPatch("Initialize")]
	public static class uGUI_SceneHUD_Initialize_Patch
	{
		private static readonly FieldInfo SceneHud_initialized = typeof(uGUI_SceneHUD).GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Instance);

		private static bool preInitialized;
		private static bool initialized;

		private static bool Prefix(uGUI_SceneHUD __instance)
		{
			preInitialized = (bool)SceneHud_initialized.GetValue(__instance);
			return true;
		}

		private static void Postfix(uGUI_SceneHUD __instance)
		{
			initialized = (bool)SceneHud_initialized.GetValue(__instance);
			if (!preInitialized && initialized)
			{
				Logger.Log("HUD Initialize");

				var hudContentRoot = __instance.transform.Find("Content");
				if (hudContentRoot == null)
				{
					Logger.Error("Could not find HUD Content Root!");
					return;
				}

				foreach (var entry in Mod.config.HudElements)
				{
					var element = hudContentRoot.Find(entry.Name) as RectTransform;
					if (element == null)
					{
						Logger.Error($"Could not find HUD element named ({entry.Name})");
						continue;
					}

					element.localScale = new Vector3(entry.Scale, entry.Scale);
					element.anchoredPosition += new Vector2(entry.XOffset, entry.YOffset);
				}
			}
		}
	}
}
