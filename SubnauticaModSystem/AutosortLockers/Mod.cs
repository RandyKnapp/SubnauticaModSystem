using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Common.Mod;
using HarmonyLib;
#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
using SMLHelper.V2.Handlers;
#endif
using UnityEngine;
using UWE;

namespace AutosortLockers
{
	internal static class Mod
	{
		public const string SaveDataFilename = "AutosortLockerSMLSaveData.json";

		private const int MAX_LOCKER_WIDTH = 8;
		private const int MAX_LOCKER_HEIGHT = 10;
		public static Config config;
		public static SaveData saveData;
		public static List<Color> colors = new List<Color>();

		private static string modDirectory;
		private static ModSaver saveObject;

		public static event Action<SaveData> OnDataLoaded;

		public static void Patch(string modDirectory = null)
		{
			Logger.Log("Starting patching");

			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			AddBuildables();

			Harmony harmony = new Harmony("com.AutosortLockersSML.mod");
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

		private static void LoadConfig()
		{
			config = ModUtils.LoadConfig<Config>(GetModPath() + "/config.json");
			ValidateConfig();

			List<SerializableColor> serializedColors = JsonConvert.DeserializeObject< List<SerializableColor> >(File.ReadAllText(GetAssetPath("colors.json")));
			foreach (var sColor in serializedColors)
			{
				colors.Add(sColor.ToColor());
			}
		}

		private static void ValidateConfig()
		{
			Config defaultConfig = new Config();

			ModUtils.ValidateConfigValue("SortInterval", 0.1f, 10.0f, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("AutosorterWidth", 1, MAX_LOCKER_WIDTH, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("AutosorterHeight", 1, MAX_LOCKER_HEIGHT, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("ReceptacleWidth", 1, MAX_LOCKER_WIDTH, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("ReceptacleHeight", 1, MAX_LOCKER_HEIGHT, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("StandingReceptacleWidth", 1, MAX_LOCKER_WIDTH, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("StandingReceptacleHeight", 1, MAX_LOCKER_HEIGHT, ref config, ref defaultConfig);
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
			ModUtils.LoadSaveData<SaveData>(SaveDataFilename, (data) =>
			{
				saveData = data;
				Logger.Log("Save Data Loaded");
				OnDataLoaded?.Invoke(saveData);
			});
		}
	}
}