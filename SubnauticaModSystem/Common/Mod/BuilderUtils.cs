using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UWE;

namespace Common.Mod
{
    public static class BuilderUtils
    {
		private static FieldInfo CraftData_groups = typeof(CraftData).GetField("groups", BindingFlags.NonPublic | BindingFlags.Static);
		private static FieldInfo CraftData_buildables = typeof(CraftData).GetField("buildables", BindingFlags.NonPublic | BindingFlags.Static);
		private static FieldInfo CraftData_techData = typeof(CraftData).GetField("techData", BindingFlags.NonPublic | BindingFlags.Static);
		private static FieldInfo CraftData_techMapping = typeof(CraftData).GetField("techMapping", BindingFlags.NonPublic | BindingFlags.Static);
		private static FieldInfo CachedEnumString_valueToString = typeof(CachedEnumString<TechType>).GetField("valueToString", BindingFlags.NonPublic | BindingFlags.Instance);
		private static FieldInfo Language_strings = typeof(Language).GetField("strings", BindingFlags.NonPublic | BindingFlags.Instance);

		private static Dictionary<TechType, string> TechType_stringsNormal			= (Dictionary<TechType, string>)typeof(TechTypeExtensions).GetField("stringsNormal", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		private static Dictionary<TechType, string> TechType_stringsLowercase		= (Dictionary<TechType, string>)typeof(TechTypeExtensions).GetField("stringsLowercase", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		private static Dictionary<string, TechType> TechType_techTypesNormal		= (Dictionary<string, TechType>)typeof(TechTypeExtensions).GetField("techTypesNormal", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		private static Dictionary<string, TechType> TechType_techTypesIgnoreCase	= (Dictionary<string, TechType>)typeof(TechTypeExtensions).GetField("techTypesIgnoreCase", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		private static Dictionary<TechType, string> TechType_techTypeKeys			= (Dictionary<TechType, string>)typeof(TechTypeExtensions).GetField("techTypeKeys", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		private static Dictionary<string, TechType> TechType_keyTechTypes			= (Dictionary<string, TechType>)typeof(TechTypeExtensions).GetField("keyTechTypes", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

		private static readonly Dictionary<TechType, CustomTechInfo> techData = new Dictionary<TechType, CustomTechInfo>();
		private static readonly Dictionary<TechType, GameObject> prefabsByTechType = new Dictionary<TechType, GameObject>();
		private static readonly Dictionary<string, GameObject> prefabsByClassID = new Dictionary<string, GameObject>();
		private static bool techMappingInitialized;
		private static bool knownTechInitialized;
		private static bool prefabDatabaseInitialized;

		public static void AddBuildable(CustomTechInfo info)
		{
			techData[info.techType] = info;

			var groups = (Dictionary<TechGroup, Dictionary<TechCategory, List<TechType>>>)CraftData_groups.GetValue(null);
			groups[info.techGroup][info.techCategory].Add(info.techType);

			var buildables = (List<TechType>)CraftData_buildables.GetValue(null);
			buildables.Add(info.techType);

			string techTypeKey = info.techTypeKey;
			string intValueString = ((int)(info.techType)).ToString();
			TechType_stringsNormal[info.techType] = techTypeKey;
			TechType_stringsLowercase[info.techType] = techTypeKey.ToLower();
			TechType_techTypesNormal[techTypeKey] = info.techType;
			TechType_techTypesIgnoreCase[techTypeKey] = info.techType;
			TechType_techTypeKeys.Add(info.techType, intValueString);
			TechType_keyTechTypes.Add(intValueString, info.techType);

			var valueToString = (Dictionary<TechType, string>)CachedEnumString_valueToString.GetValue(TooltipFactory.techTypeTooltipStrings);
			string tooltipKey = "Tooltip_" + info.techTypeKey;
			valueToString[info.techType] = tooltipKey;

			Console.WriteLine("[BuilderUtils] Added builder entry for " + info.techTypeKey + " (" + info.techType + ")");
		}

		public static void OnTechMappingInitialized()
		{
			if (techMappingInitialized)
			{
				return;
			}
			techMappingInitialized = true;

			Console.WriteLine("[BuilderUtils] Initializing tech mapping");
			var techMapping = (Dictionary<TechType, string>)CraftData_techMapping.GetValue(null);
			foreach (var entry in techData)
			{
				var info = entry.Value;
				techMapping.Add(info.techType, info.assetPath);
			}
		}

		public static void OnKnownTechInitialized()
		{
			if (knownTechInitialized)
			{
				return;
			}
			knownTechInitialized = true;

			Console.WriteLine("[BuilderUtils] Adding initially known blueprints to KnownTech");
			foreach (var entry in techData)
			{
				var info = entry.Value;
				if (info.knownAtStart)
				{
					KnownTech.Add(info.techType, false);
				}
			}
		}

		public static void OnLanguageStringsInitialized()
		{
			Console.WriteLine("[BuilderUtils] Adding tooltip strings to language file");
			Dictionary <string, string> strings = (Dictionary<string, string>)Language_strings.GetValue(Language.main);
			foreach (var entry in techData)
			{
				var info = entry.Value;
				string tooltipKey = "Tooltip_" + info.techTypeKey;
				string intValueString = ((int)(info.techType)).ToString();
				strings[info.techTypeKey] = info.displayString;
				strings[intValueString] = info.displayString;
				strings[tooltipKey] = info.tooltip;
			}
		}

		public static void OnPrefabDatabaseInitialized()
		{
			if (prefabDatabaseInitialized)
			{
				return;
			}
			prefabDatabaseInitialized = true;

			prefabsByTechType.Clear();
			prefabsByClassID.Clear();

			Console.WriteLine("[BuilderUtils] Initializing prefabs");
			foreach (var entry in techData)
			{
				var info = entry.Value;
				var prefab = GetPrefab(info.techType);
				if (prefab != null)
				{
					continue;
				}

				if (prefab == null && info.getPrefab != null)
				{
					prefab = info.getPrefab.Invoke();
				}

				if (prefab == null)
				{
					Console.WriteLine("[BuilderUtils] ERROR creating prefab for " + info.techTypeKey);
					continue;
				}

				prefab.SetActive(false);
				prefab.transform.position = new Vector3(5000, 5000, 5000);

				var constructable = prefab.GetComponent<Constructable>();
				if (constructable != null)
				{
					constructable.techType = info.techType;
				}

				var techTag = prefab.GetComponent<TechTag>();
				if (techTag != null)
				{
					techTag.type = info.techType;
				}

				var prefabIdentifier = prefab.GetComponent<PrefabIdentifier>();
				if (prefabIdentifier != null)
				{
					prefabIdentifier.ClassId = info.assetPath;
				}

				var onDestroy = prefab.AddComponent<CallbackOnDestroy>();
				onDestroy.onDestroy = OnPrefabDestroyed;

				AddPrefab(info.techType, info.assetPath, prefab);
				PrefabDatabase.prefabFiles[info.assetPath] = info.assetPath;
				PrefabDatabase.AddToCache(info.assetPath, prefab);
			}
		}

		private static void OnPrefabDestroyed(GameObject prefab)
		{
			Console.WriteLine("OnPrefabDestroyed:" + prefab);
			var entry1 = prefabsByTechType.FirstOrDefault((x) => x.Value == prefab);
			prefabsByTechType.Remove(entry1.Key);

			var entry2 = prefabsByClassID.FirstOrDefault((x) => x.Value == prefab);
			prefabsByClassID.Remove(entry2.Key);

			prefabDatabaseInitialized = false;
		}

		public static void AddPrefab(TechType techType, string classID, GameObject prefab)
		{
			prefabsByTechType[techType] = prefab;
			prefabsByClassID[classID] = prefab;
		}

		public static void RemovePrefab(TechType techType)
		{

		}

		public static GameObject GetPrefab(TechType techType)
		{
			prefabsByTechType.TryGetValue(techType, out GameObject result);
			if (result == null && techData.ContainsKey(techType))
			{
				OnPrefabDatabaseInitialized();
				prefabsByTechType.TryGetValue(techType, out result);
			}
			if (result != null)
			{
				result.SetActive(true);
			}
			return result;
		}

		public static GameObject GetPrefab(string classID)
		{
			prefabsByClassID.TryGetValue(classID, out GameObject result);
			if (result == null && techData.Count((KeyValuePair<TechType, CustomTechInfo> x) => x.Value.assetPath == classID) > 0)
			{
				OnPrefabDatabaseInitialized();
				prefabsByClassID.TryGetValue(classID, out result);
			}
			if (result != null)
			{
				result.SetActive(true);
			}
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

		public static CustomTechInfo GetCustomTechDataByKey(string key)
		{
			foreach (var entry in techData)
			{
				if (entry.Key.ToString() == key)
				{
					return entry.Value;
				}
			}

			return null;
		}

		public static ITechData GetTechData(TechType techType)
		{
			return GetCustomTechData(techType);
		}
	}
}
