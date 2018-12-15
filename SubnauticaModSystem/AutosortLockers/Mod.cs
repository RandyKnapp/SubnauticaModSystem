using Common.Mod;
using Common.Utility;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Oculus.Newtonsoft.Json;

namespace AutosortLockers
{
	public enum CustomTechType
	{
		AutosortLocker = 11110,
		AutosortTarget,
		AutosortTargetStanding
	}

	static class Mod
	{
		public const string SaveDataFilename = "AutosortLockerSaveData.json";
		public static Config config;
		public static SaveData saveData;
		public static List<Color> colors = new List<Color>();

		private static string modDirectory;
		private static ModSaver saveObject;

		public static event Action<SaveData> OnDataLoaded;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			AddBuildables();

			HarmonyInstance harmony = HarmonyInstance.Create("com.AutosortLockers.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		public static void AddBuildables()
		{
			AutosortLocker.AddBuildable();
			AutosortTarget.AddBuildable();
		}

		public static string GetModPath()
		{
			return Environment.CurrentDirectory + "/" + modDirectory;
		}

		public static string GetAssetPath(string filename)
		{
			return GetModPath() + "/Assets/" + filename;
		}

		private static string GetModInfoPath()
		{
			return GetModPath() + "/mod.json";
		}

		private static void LoadConfig()
		{
			config = ModUtils.LoadConfig<Config>(GetModInfoPath());
			ValidateConfig();

			var serializedColors = JsonConvert.DeserializeObject<List<SerializableColor>>(File.ReadAllText(GetAssetPath("colors.json")));
			foreach (var sColor in serializedColors)
			{
				colors.Add(sColor.ToColor());
			}
		}

		private static void ValidateConfig()
		{
			Config defaultConfig = new Config();
			if (config == null)
			{
				config = defaultConfig;
				return;
			}

			ModUtils.ValidateConfigValue("SortInterval", 0.1f, 10.0f, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("AutosorterWidth", 1, 8, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("AutosorterHeight", 1, 10, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("ReceptacleWidth", 1, 8, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("ReceptacleHeight", 1, 10, ref config, ref defaultConfig);
		}

		public static TechType GetTechType(CustomTechType customTechType)
		{
			return (TechType)customTechType;
		}

		public static SaveData GetSaveData()
		{
			return saveData ?? new SaveData();
		}

		public static SaveDataEntry GetSaveData(string id)
		{
			var saveData = GetSaveData();
			foreach (var entry in saveData.Entries)
			{
				if (entry.Id == id)
				{
					return entry;
				}
			}
			return new SaveDataEntry() { Id = id };
		}

		public static void Save()
		{
			if (!IsSaving())
			{
				saveObject = new GameObject().AddComponent<ModSaver>();

				SaveData newSaveData = new SaveData();
				var targets = GameObject.FindObjectsOfType<AutosortTarget>();
				foreach (var target in targets)
				{
					target.Save(newSaveData);
				}
				saveData = newSaveData;
				ModUtils.Save<SaveData>(saveData, SaveDataFilename, OnSaveComplete);
			}
		}

		public static void OnSaveComplete()
		{
			saveObject.StartCoroutine(SaveCoroutine());
		}

		private static IEnumerator SaveCoroutine()
		{
			while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
			{
				yield return null;
			}
			GameObject.DestroyImmediate(saveObject.gameObject);
			saveObject = null;
		}

		public static bool IsSaving()
		{
			return saveObject != null;
		}

		public static void LoadSaveData()
		{
			Logger.Log("Loading Save Data...");
			ModUtils.LoadSaveData<SaveData>(SaveDataFilename, (data) => {
				saveData = data;
				Logger.Log("Save Data Loaded");
				OnDataLoaded?.Invoke(saveData);
			});
		}
	}
}