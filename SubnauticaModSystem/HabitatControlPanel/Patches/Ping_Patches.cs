using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HabitatControlPanel.Patches
{
	[HarmonyPatch(typeof(uGUI_Pings))]
	[HarmonyPatch("OnWillRenderCanvases")]
	internal class uGUI_Pings_OnWillRenderCanvases_Patch
	{
		private static readonly FieldInfo uGUI_Pings_pings = typeof(uGUI_Pings).GetField("pings", BindingFlags.NonPublic | BindingFlags.Instance);

		private static uGUI_Pings staticInstance;
		private static Dictionary<int, uGUI_Ping> pings;

		[HarmonyPostfix]
		private static void Postfix(uGUI_Pings __instance)
		{
			if (staticInstance != __instance)
			{
				staticInstance = __instance;
				pings = (Dictionary<int, uGUI_Ping>)uGUI_Pings_pings.GetValue(__instance);
			}

			foreach (var entry in pings)
			{
				var ping = PingManager.Get(entry.Key);
				var guiPing = entry.Value;

				if (ping.colorIndex >= 0 && ping.colorIndex < PingManager.colorOptions.Length)
				{
					guiPing.SetColor(PingManager.colorOptions[ping.colorIndex]);
				}

				var sprite = SpriteManager.Get(SpriteManager.Group.Pings, Enum.GetName(typeof(PingType), ping.pingType));
				guiPing.SetIcon(sprite);
			}
		}
	}
}
