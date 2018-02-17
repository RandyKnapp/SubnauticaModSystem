using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BlueprintTracker
{
	public static class Mod
	{
		public const int MaxPins = 6;
		public const int MinPins = 1;

		public static Config config;

		private static string modDirectory;

		public static List<TechType> CurrentPins = new List<TechType>();

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.blueprinttracker.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		private static string GetModInfoPath()
		{
			return Environment.CurrentDirectory + "\\" + modDirectory + "\\mod.json";
		}

		private static void LoadConfig()
		{
			string modInfoPath = GetModInfoPath();

			if (!File.Exists(modInfoPath))
			{
				config = new Config();
				return;
			}

			var modInfoObject = JSON.Parse(File.ReadAllText(modInfoPath));
			string configJson = modInfoObject["Config"].ToString();
			config = JsonUtility.FromJson<Config>(configJson);
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

			if (config.MaxPinnedBlueprints < MinPins || config.MaxPinnedBlueprints > MaxPins)
			{
				Logger.Log("Config value for {0} ({1}) was not valid. Must be between {2} and {3}",
					"MaxPinnedBlueprints",
					config.MaxPinnedBlueprints,
					MinPins,
					MaxPins
				);
				config.MaxPinnedBlueprints = defaultConfig.MaxPinnedBlueprints;
			}
		}
	}
}