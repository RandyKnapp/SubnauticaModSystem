using Common.Mod;
using Common.Utility;
using Harmony;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTracker
{
	static class Mod
	{
		public const string SaveDataFilename = "BlueprintTrackerSave.json";
		public const int MaxPins = 13;
		public const int MinPins = 1;

		public static Config config;
		public static bool Left = false;
		public static bool Top = true;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.blueprinttracker.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
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

		public static int GetMaxPins()
		{
			return config.MaxPinnedBlueprints;
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

			ModUtils.ValidateConfigValue("MaxPinnedBlueprints", MinPins, MaxPins, config, defaultConfig);

			switch (config.Position)
			{
				case "TopLeft":		Left = true;	Top = true;		break;
				case "TopRight":	Left = false;	Top = true;		break;
				case "BottomLeft":	Left = true;	Top = false;	break;
				case "BottomRight":	Left = false;	Top = false;	break;

				default:
					Logger.Log("Config value for '{0}' ({1}) as not valid. Must be one of: TopLeft, TopRight, BottomLeft, BottomRight", "Position", config.Position);
					config.Position = defaultConfig.Position;
					break;
			}

			ModUtils.ValidateConfigValue("TrackerScale", 0.01f, 5.0f, config, defaultConfig);
			ModUtils.ValidateConfigValue("FontSize", 10, 60, config, defaultConfig);
			ModUtils.ValidateConfigValue("BackgroundAlpha", 0.0f, 1.0f, config, defaultConfig);
		}

		public static SaveData LoadSaveData()
		{
			return ModUtils.LoadSaveData<SaveData>(SaveDataFilename);
		}

		public static void Save(SaveData newSaveData)
		{
			ModUtils.Save<SaveData>(newSaveData, SaveDataFilename);
		}
	}
}