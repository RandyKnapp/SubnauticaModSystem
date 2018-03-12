using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.UI;

namespace CustomPings.Patches
{
	[HarmonyPatch(typeof(uGUI_PingEntry))]
	[HarmonyPatch("Initialize")]
	class uGUI_PingEntry_Initialize_Patch
	{
		private static int realColorIndex;

		private static bool Prefix(uGUI_PingEntry __instance, int id, bool visible, PingType type, string name, ref int colorIndex)
		{
			realColorIndex = colorIndex;
			if (colorIndex >= 5)
			{
				colorIndex = 0;
			}

			for (int i = 0; i < 5; ++i)
			{
				Toggle toggle = __instance.colorSelectors[i];
				toggle.onValueChanged.RemoveAllListeners();
			}

			return true;
		}

		private static void Postfix(uGUI_PingEntry __instance, int id, bool visible, PingType type, string name, int colorIndex)
		{
			__instance.icon.sprite = SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(type));
			__instance.icon.color = PingManager.colorOptions[realColorIndex];

			for (int i = 0; i < 5; ++i)
			{
				Toggle toggle = __instance.colorSelectors[i];
				toggle.isOn = false;
				toggle.gameObject.SetActive(false);
			}
			__instance.colorSelectionIndicator.gameObject.SetActive(false);

			PingManager.SetColor(id, realColorIndex);
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
	class uGUI_PingTab_OnEnable_Patch
	{
		private static readonly FieldInfo uGUI_PingTab_entries = typeof(uGUI_PingTab).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance);

		private static void Postfix(uGUI_PingTab __instance)
		{
			var entries = (Dictionary<int, uGUI_PingEntry>)uGUI_PingTab_entries.GetValue(__instance);
			foreach (var entry in entries)
			{
				var id = entry.Key;
				var pingEntry = entry.Value;
				var pingInstance = PingManager.Get(id);

				pingEntry.icon.sprite = SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(pingInstance.pingType));
				pingEntry.icon.color = PingManager.colorOptions[pingInstance.colorIndex];
				pingEntry.UpdateLabel(pingInstance.pingType, pingInstance.GetLabel());
				pingEntry.visibility.isOn = pingInstance.visible;
				pingEntry.visibilityIcon.sprite = ((!pingInstance.visible) ? pingEntry.spriteHidden : pingEntry.spriteVisible);

				for (int i = 0; i < 5; ++i)
				{
					Toggle toggle = pingEntry.colorSelectors[i];
					toggle.isOn = false;
				}
			}
		}
	}
}
