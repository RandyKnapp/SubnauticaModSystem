using Common.Mod;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HabitatControlPanel.Patches
{
	[HarmonyPatch(typeof(TechTypeExtensions))]
	[HarmonyPatch("Contains")]
	class TechTypeExtensions_Contains_Patch
	{
		private static bool Prefix(ref bool __result, TechType techType)
		{
			if (BuilderUtils.HasCustomTechData(techType))
			{
				__result = true;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(TechTypeExtensions))]
	[HarmonyPatch("TryGet")]
	class TechTypeExtensions_TryGet_Patch
	{
		private static bool Prefix(ref bool __result, TechType techType, ref string result)
		{
			var techData = BuilderUtils.GetCustomTechData(techType);
			if (techData != null)
			{
				result = techData.displayString;
				__result = true;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(TechTypeExtensions))]
	[HarmonyPatch("Get")]
	class TechTypeExtensions_Get_Patch
	{
		private static bool Prefix(ref string __result, TechType techType)
		{
			var techData = BuilderUtils.GetCustomTechData(techType);
			if (techData != null)
			{
				__result = techData.displayString;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(TechTypeExtensions))]
	[HarmonyPatch("GetOrFallback")]
	[HarmonyPatch(new Type[] { typeof(Language), typeof(TechType), typeof(TechType)})]
	class TechTypeExtensions_GetOrFallback1_Patch
	{
		private static bool Prefix(ref string __result, TechType techType)
		{
			var techData = BuilderUtils.GetCustomTechData(techType);
			if (techData != null)
			{
				__result = techData.displayString;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(TechTypeExtensions))]
	[HarmonyPatch("GetOrFallback")]
	[HarmonyPatch(new Type[] { typeof(Language), typeof(string), typeof(TechType) })]
	class TechTypeExtensions_GetOrFallback2_Patch
	{
		private static bool Prefix(ref string __result, string key)
		{
			var techData = BuilderUtils.GetCustomTechDataByKey(key);
			if (techData != null)
			{
				__result = techData.displayString;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(TechTypeExtensions))]
	[HarmonyPatch("GetOrFallback")]
	[HarmonyPatch(new Type[] { typeof(Language), typeof(TechType), typeof(string) })]
	class TechTypeExtensions_GetOrFallback3_Patch
	{
		private static bool Prefix(ref string __result, TechType techType)
		{
			var techData = BuilderUtils.GetCustomTechData(techType);
			if (techData != null)
			{
				__result = techData.displayString;
				return false;
			}
			return true;
		}
	}
}
