using Common.Mod;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UWE;

namespace AutosortLockers.Patches
{
	[HarmonyPatch(typeof(PrefabDatabase))]
	[HarmonyPatch("LoadPrefabDatabase")]
	class PrefabDatabase_LoadPrefabDatabase_Patch
	{
		private const string PrefabPrefix = "Submarine/Build/";

		private static void Postfix()
		{
			BuilderUtils.OnPrefabDatabaseInitialized();
		}
	}

	[HarmonyPatch(typeof(PrefabDatabase))]
	[HarmonyPatch("GetPrefabForFilename")]
	class PrefabDatabase_GetPrefabForFilename_Patch
	{
		private static bool Prefix(ref GameObject __result, string filename)
		{
			var prefab = BuilderUtils.GetPrefab(filename);
			if (prefab != null)
			{
				__result = prefab;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PrefabDatabase))]
	[HarmonyPatch("GetPrefabAsync")]
	class PrefabDatabase_GetPrefabAsync_Patch
	{
		private static bool Prefix(ref IPrefabRequest __result, string classId)
		{
			var prefab = BuilderUtils.GetPrefab(classId);
			if (prefab != null)
			{
				__result = new LoadedPrefabRequest(prefab);
				return false;
			}
			return true;
		}
	}
}
