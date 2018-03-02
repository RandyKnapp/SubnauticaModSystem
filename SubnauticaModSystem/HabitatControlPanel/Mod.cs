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

namespace HabitatControlPanel
{
	public enum CustomTechType
	{
		HabitatControlPanel = 11120
	}

	public enum CustomFaceType
	{
		HabitatControlPanel = 80
	}

	static class Mod
	{
		public const string SaveDataFilename = "HabitatControlPanelSaveData.json";
		public static Config config;
		public static SaveData saveData;

		private static string modDirectory;
		private static GameObject modObject;
		private static bool needsSave;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			AddBuildables();

			HarmonyInstance harmony = HarmonyInstance.Create("com.HabitatControlPanel.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		public static void AddBuildables()
		{
			HabitatControlPanel.AddBuildable();
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

		public static void SetNeedsSaving()
		{
			needsSave = true;
		}

		public static bool NeedsSaving()
		{
			return needsSave;
		}

		public static void Save()
		{
			if (!IsSaving())
			{
				modObject = new GameObject("HabitatControlPanelSaveObject");
				var x = modObject.AddComponent<ModSaver>();
				x.StartCoroutine(SaveCoroutine());
			}
		}

		public static bool IsSaving()
		{
			return modObject != null;
		}

		private static IEnumerator SaveCoroutine()
		{
			yield return null;

			SaveData newSaveData = new SaveData();
			var targets = GameObject.FindObjectsOfType<HabitatControlPanel>();
			foreach (var target in targets)
			{
				target.Save(newSaveData);
			}
			WriteSaveData(newSaveData);
			saveData = newSaveData;

			yield return null;
			GameObject.Destroy(modObject);
			modObject = null;
			needsSave = false;
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