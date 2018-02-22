using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Common.Mod
{
    public static class BuilderUtils
    {
		private static FieldInfo CraftData_groups = typeof(CraftData).GetField("groups", BindingFlags.NonPublic | BindingFlags.Static);
		private static FieldInfo CraftData_buildables = typeof(CraftData).GetField("buildables", BindingFlags.NonPublic | BindingFlags.Static);
		private static FieldInfo CraftData_techData = typeof(CraftData).GetField("techData", BindingFlags.NonPublic | BindingFlags.Static);
		private static FieldInfo CraftData_techMapping = typeof(CraftData).GetField("techMapping", BindingFlags.NonPublic | BindingFlags.Static);

		private static readonly Dictionary<TechType, CustomTechInfo> techData = new Dictionary<TechType, CustomTechInfo>();
		private static readonly Dictionary<TechType, GameObject> prefabsByTechType = new Dictionary<TechType, GameObject>();
		private static readonly Dictionary<string, GameObject> prefabsByClassID = new Dictionary<string, GameObject>();
		private static bool techMappingInitialized;
		private static bool knownTechInitialized;

		public static void AddBuildable(CustomTechInfo info)
		{
			techData[info.techType] = info;

			var groups = (Dictionary<TechGroup, Dictionary<TechCategory, List<TechType>>>)CraftData_groups.GetValue(null);
			groups[info.techGroup][info.techCategory].Add(info.techType);
			Console.WriteLine("[BuilderUtils] All entries in " + info.techGroup + ">" + info.techCategory + ":");
			foreach (var techType in groups[info.techGroup][info.techCategory])
			{
				Console.WriteLine("-" + techType);
			}

			var buildables = (List<TechType>)CraftData_buildables.GetValue(null);
			buildables.Add(info.techType);
			Console.WriteLine("[BuilderUtils] All entries in buildabiles:");
			foreach (var techType in buildables)
			{
				Console.WriteLine("-" + techType);
			}

			Console.WriteLine("[BuilderUtils] Added builder entry for " + info.techType);
		}

		public static void OnTechMappingInitialized()
		{
			if (techMappingInitialized)
			{
				return;
			}

			var techMapping = (Dictionary<TechType, string>)CraftData_techMapping.GetValue(null);
			foreach (var entry in techData)
			{
				var info = entry.Value;
				techMapping.Add(info.techType, info.assetPath);
			}
			
			Console.WriteLine("[BuilderUtils] All entries in techMapping:");
			foreach (var entry in techMapping)
			{
				Console.WriteLine("-" + entry.Key + ":" + entry.Value);
			}

			techMappingInitialized = true;
		}

		public static void OnKnownTechInitialized()
		{
			if (knownTechInitialized)
			{
				return;
			}

			foreach (var entry in techData)
			{
				var info = entry.Value;
				if (info.knownAtStart)
				{
					KnownTech.Add(info.techType);
				}
			}

			knownTechInitialized = true;
		}

		public static void AddPrefab(TechType techType, string classID, GameObject prefab)
		{
			prefabsByTechType[techType] = prefab;
			prefabsByClassID[classID] = prefab;
		}

		public static GameObject GetPrefab(TechType techType)
		{
			prefabsByTechType.TryGetValue(techType, out GameObject result);
			return result;
		}

		public static GameObject GetPrefab(string classID)
		{
			prefabsByClassID.TryGetValue(classID, out GameObject result);
			return result;
		}

		public static bool HasCustomTechData(TechType techType)
		{
			return techData.ContainsKey(techType);
		}

		public static CustomTechInfo GetCustomTechData(TechType techType)
		{
			techData.TryGetValue(techType, out CustomTechInfo info);
			return info;
		}

		public static ITechData GetTechData(TechType techType)
		{
			return GetCustomTechData(techType);
		}
	}
}
