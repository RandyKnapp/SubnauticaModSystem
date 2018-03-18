using Common.Mod;
using Common.Utility;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomBeacons.Patches
{
	[HarmonyPatch(typeof(SpriteManager))]
	[HarmonyPatch("Get")]
	[HarmonyPatch(new Type[] { typeof(SpriteManager.Group), typeof(string) })]
	class SpriteManager_Get_Patch
	{
		private static bool Prefix(ref Atlas.Sprite __result, SpriteManager.Group group, string name)
		{
			if (group == SpriteManager.Group.Pings)
			{
				if (CustomPings.PingExists(name))
				{
					__result = CustomPings.GetSprite(name);
					return false;
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Enum))]
	[HarmonyPatch("GetValues")]
	class Enum_GetValues_Patch
	{
		private static readonly Type PingTypeT = typeof(PingType);

		private static bool Prefix(ref Array __result, Type enumType)
		{
			if (enumType == PingTypeT)
			{
				List<PingType> values = new List<PingType>();
				for (int i = 0; i < (int)PingType.Sunbeam; ++i)
				{
					values.Add((PingType)i);
				}
				foreach (var customType in CustomPings.CustomPingNames)
				{
					values.Add((PingType)customType.Key);
				}
				__result = values.ToArray();
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Enum))]
	[HarmonyPatch("GetNames")]
	class Enum_GetNames_Patch
	{
		private static readonly Type PingTypeT = typeof(PingType);

		private static bool Prefix(ref string[] __result, Type enumType)
		{
			if (enumType == PingTypeT)
			{
				List<string> names = new List<string>();
				for (int i = 0; i < (int)PingType.Sunbeam; ++i)
				{
					names.Add(((PingType)i).ToString());
				}
				foreach (var customType in CustomPings.CustomPingNames)
				{
					names.Add(customType.Value);
				}
				__result = names.ToArray();
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Enum))]
	[HarmonyPatch("GetName")]
	class Enum_GetName_Patch
	{
		private static readonly Type PingTypeT = typeof(PingType);

		private static bool Prefix(ref string __result, Type enumType, object value)
		{
			if (enumType == PingTypeT)
			{
				int v = (int)value;
				if (v > (int)PingType.Sunbeam && CustomPings.PingExists(v))
				{
					__result = CustomPings.GetPingName(v);
					return false;
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PingInstance))]
	[HarmonyPatch("OnEnable")]
	class PingInstance_OnEnable_Patch
	{
		private static void Postfix(PingInstance __instance)
		{
			var saver = __instance.gameObject.GetComponent<PingInstanceSaver>();
			if (saver == null)
			{
				saver = __instance.gameObject.AddComponent<PingInstanceSaver>();
			}
		}
	}

	[HarmonyPatch(typeof(uGUI_Pings))]
	[HarmonyPatch("OnWillRenderCanvases")]
	class uGUI_Pings_OnWillRenderCanvases_Patch
	{
		private static readonly FieldInfo uGUI_Pings_pings = typeof(uGUI_Pings).GetField("pings", BindingFlags.NonPublic | BindingFlags.Instance);

		private static uGUI_Pings staticInstance;
		private static Dictionary<int, uGUI_Ping> pings;

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

				guiPing.SetColor(CustomPings.GetColor(ping.colorIndex));

				var sprite = SpriteManager.Get(SpriteManager.Group.Pings, Enum.GetName(typeof(PingType), ping.pingType));
				guiPing.SetIcon(sprite);
			}
		}
	}
}
