﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.UI;

namespace CustomBeacons.Patches
{
	[HarmonyPatch(typeof(uGUI_PingEntry))]
	[HarmonyPatch("Initialize")]
	class uGUI_PingEntry_Initialize_Patch
	{
		private static readonly FieldInfo uGUI_PingEntry_id = typeof(uGUI_PingEntry).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);

		private static bool Prefix(uGUI_PingEntry __instance, int id, bool visible, PingType type, string name, ref int colorIndex)
		{
			CustomPings.Initialize();

			__instance.gameObject.SetActive(true);
			uGUI_PingEntry_id.SetValue(__instance, id);

			__instance.visibility.isOn = visible;
			__instance.visibilityIcon.sprite = ((!visible) ? __instance.spriteHidden : __instance.spriteVisible);
			__instance.UpdateLabel(type, name);

			for (int i = 0; i < 5; ++i)
			{
				Toggle toggle = __instance.colorSelectors[i];
				toggle.onValueChanged.RemoveAllListeners();
				toggle.isOn = false;
				toggle.gameObject.SetActive(false);
			}
			__instance.colorSelectionIndicator.gameObject.SetActive(false);

			var controller = __instance.gameObject.GetComponent<PingEntryController>();
			if (controller == null)
			{
				controller = __instance.gameObject.AddComponent<PingEntryController>();
			}
			controller.OnInitialize(id, type, colorIndex);

			__instance.icon.sprite = SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(type));
			__instance.icon.color = CustomPings.GetColor(colorIndex);
			return false;
		}
	}

	[HarmonyPatch(typeof(uGUI_PingEntry))]
	[HarmonyPatch("SetColor")]
	class uGUI_PingEntry_SetColor_Patch
	{
		private static bool Prefix()
		{
			return false;
		}
	}

	[HarmonyPatch(typeof(uGUI_PingEntry))]
	[HarmonyPatch("UpdateLabel")]
	class uGUI_PingEntry_UpdateLabel_Patch
	{
		private static bool Prefix(uGUI_PingEntry __instance, PingType type, string name)
		{
			string text = Language.main.Get(PingManager.sCachedPingTypeTranslationStrings.Get(type));
			if (!string.IsNullOrEmpty(name))
			{
				text = string.Format("{1} ({0})", text, name);
			}
			__instance.label.text = text;
			return false;
		}
	}

	[HarmonyPatch(typeof(uGUI_PingTab))]
	[HarmonyPatch("UpdateEntries")]
	class uGUI_PingTab_UpdateEntries_Patch
	{
		private static readonly FieldInfo uGUI_PingTab_entries = typeof(uGUI_PingTab).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance);

		private static void Postfix(uGUI_PingTab __instance)
		{
			CustomPings.Initialize();
			var entries = (Dictionary<int, uGUI_PingEntry>)uGUI_PingTab_entries.GetValue(__instance);
			foreach (var entry in entries)
			{
				var id = entry.Key;
				var pingEntry = entry.Value;
				var pingInstance = PingManager.Get(id);

				pingEntry.icon.sprite = SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(pingInstance.pingType));
				pingEntry.icon.color = CustomPings.GetColor(pingInstance.colorIndex);
				pingEntry.UpdateLabel(pingInstance.pingType, pingInstance.GetLabel());
				pingEntry.visibility.isOn = pingInstance.visible;
				pingEntry.visibilityIcon.sprite = ((!pingInstance.visible) ? pingEntry.spriteHidden : pingEntry.spriteVisible);
			}
		}
	}
}
