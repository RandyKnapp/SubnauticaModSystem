using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MoreQuickSlots
{
	public static class Mod
	{
		public const int MaxSlots = 12;
		public const int MinSlots = 1;

		public static Config config;

		public static void Patch()
		{
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.morequickslots.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Controller.Load();

			Logger.Log("Initialized");
		}

		private static string GetModInfoPath()
		{
			return Environment.CurrentDirectory + @"\QMods\mod.json";
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
			string configJson = modInfoObject["config"].ToString();
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

			if (config.SlotCount < MinSlots || config.SlotCount > MaxSlots)
			{
				config.SlotCount = defaultConfig.SlotCount;
			}
		}
	}
}