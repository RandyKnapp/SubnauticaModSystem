using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BetterPowerInfo
{
	public static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.betterpowerinfo.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			GameController.Load();

			Logger.Log("Initialized");
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
		}
	}
}