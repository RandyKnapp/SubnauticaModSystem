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
			GameObject prefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			GameObject newPrefab = GameObject.Instantiate(prefab);
			TechType techType = (TechType)CustomTechType.AutosortLocker;
			string classID = PrefabPrefix + CustomTechType.AutosortLocker;

			newPrefab.name = CustomTechType.AutosortLocker.ToString();
			var meshRenderers = newPrefab.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = new Color(1, 0, 0);
			}

			var constructable = newPrefab.GetComponent<Constructable>();
			constructable.techType = techType;

			var techTag = newPrefab.GetComponent<TechTag>();
			techTag.type = techType;

			var prefabIdentifier = newPrefab.GetComponent<PrefabIdentifier>();
			prefabIdentifier.ClassId = classID;

			ModUtils.PrintObject(newPrefab);

			PrefabDatabase.prefabFiles.Add(classID, classID);
			PrefabDatabase.AddToCache(classID, newPrefab);
			BuilderUtils.AddPrefab(techType, classID, newPrefab);
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

	/*[HarmonyPatch(typeof(PrefabDatabase))]
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
	}*/
}
