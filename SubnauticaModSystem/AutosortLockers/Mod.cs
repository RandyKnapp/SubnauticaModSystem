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
		AutosortTarget
	}

	static class Mod
	{
		public const string SaveDataFilename = "AutosortLockerSaveData.json";
		public static Config config;
		public static SaveData saveData;

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
		}

		private static void ValidateConfig()
		{
			Config defaultConfig = new Config();
			if (config == null)
			{
				config = defaultConfig;
				return;
			}
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

		public static void Save()
		{
			if (!IsSaving())
			{
				SaveData newSaveData = new SaveData();
				var targets = GameObject.FindObjectsOfType<AutosortTarget>();
				foreach (var target in targets)
				{
					target.SaveFilters(newSaveData);
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
				string saveDataJson = JsonConvert.SerializeObject(newSaveData);
				File.WriteAllText(saveFile, saveDataJson);
			}
		}
	}
}