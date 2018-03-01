using Common.Mod;
using Harmony;
using UnityEngine;

namespace HabitatControlPanel.Patches
{
	[HarmonyPatch(typeof(CraftData))]
	[HarmonyPatch("GetPrefabForTechType")]
	class CraftData_GetPrefabForTechType_Patch
	{
		private static bool Prefix(ref GameObject __result, TechType techType, bool verbose)
		{
			var prefab = BuilderUtils.GetPrefab(techType);
			if (prefab != null)
			{
				Logger.Log("Getting prefab for " + techType + " = " + prefab);
				__result = prefab;
				return false;
			}

			return true;
		}
	}
}
