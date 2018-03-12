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

namespace CustomPings.Patches
{
	[HarmonyPatch(typeof(uGUI_Pings))]
	[HarmonyPatch("OnAdd")]
	class uGUI_Pings_OnAdd_Patch
	{
		private static bool Prefix(uGUI_Pings __instance, int id, PingInstance instance)
		{
			CustomPings.Initialize();
			return true;
		}
	}

	[HarmonyPatch(typeof(PingInstance))]
	[HarmonyPatch("SetColor")]
	class PingInstance_SetColor_Patch
	{
		private static bool Prefix(PingInstance __instance, int index)
		{
			CustomPings.Initialize();
			return true;
		}
	}

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

	[HarmonyPatch(typeof(uGUI_Pings))]
	[HarmonyPatch("OnVisible")]
	class uGUI_Pings_OnVisible_Patch
	{
		private static readonly FieldInfo uGUI_Pings_pings = typeof(uGUI_Pings).GetField("pings", BindingFlags.NonPublic | BindingFlags.Instance);

		private static void Postfix(uGUI_Pings __instance, int id, bool visible)
		{
			var pings = (Dictionary<int, uGUI_Ping>)uGUI_Pings_pings.GetValue(__instance);
			if (pings.TryGetValue(id, out uGUI_Ping guiPing))
			{
				var ping = PingManager.Get(id);
				var sprite = SpriteManager.Get(SpriteManager.Group.Pings, Enum.GetName(typeof(PingType), ping.pingType));
				guiPing.SetIcon(sprite);
			}
		}
	}
}
