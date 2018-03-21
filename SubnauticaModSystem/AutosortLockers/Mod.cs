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

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
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
			return Environment.CurrentDirectory + "\\" + modDirectory;
		}

		public static string GetAssetPath(string filename)
		{
			return GetModPath() + @"\Assets\" + filename;
		}

		private static string GetModInfoPath()
		{
			return GetModPath() + "\\mod.json";
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
			if (saveData == null)
			{
				saveData = LoadSaveData();
				if (saveData == null)
				{
					saveData = new SaveData();
				}
			}
			return saveData;
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
			return new SaveDataEntry();
		}

		public static void Save()
		{
			if (!IsSaving())
			{
				SaveData newSaveData = new SaveData();
				var targets = GameObject.FindObjectsOfType<AutosortTarget>();
				foreach (var target in targets)
				{
					target.Save(newSaveData);
				}
				WriteSaveData(newSaveData);
				saveData = newSaveData;

				saveObject = new GameObject("AutosortLockersSaveObject").AddComponent<ModSaver>();
				saveObject.StartCoroutine(SaveCoroutine());
			}
		}

		public static bool IsSaving()
		{
			return saveObject != null;
		}

		private static IEnumerator SaveCoroutine()
		{
			while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
			{
				yield return null;
			}
			GameObject.Destroy(saveObject.gameObject);
			saveObject = null;
		}

		private static SaveData LoadSaveData()
		{
			var saveDir = ModUtils.GetSaveDataDirectory();
			var saveFile = Path.Combine(saveDir, SaveDataFilename);
			if (File.Exists(saveFile))
			{
				SaveData saveData = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(saveFile));
				if (saveData != null)
				{
					return saveData;
				}
			}

			return new SaveData();
		}

		private static void WriteSaveData(SaveData newSaveData)
		{
			if (newSaveData != null)
			{
				var saveDir = ModUtils.GetSaveDataDirectory();
				var saveFile = Path.Combine(saveDir, SaveDataFilename);
				string saveDataJson = JsonConvert.SerializeObject(newSaveData, Formatting.Indented);
				File.WriteAllText(saveFile, saveDataJson);
			}
		}
	}
}