﻿using Common.Mod;
using HarmonyLib;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace HudConfig
{
	static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory)
		{
			Mod.modDirectory = modDirectory;
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.HudConfig.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		public static string GetModPath()
		{
			return Path.Combine(Environment.CurrentDirectory, modDirectory);
		}

		public static string GetAssetPath(string filename)
		{
			return Path.Combine(Path.Combine(GetModPath(), "Assets"), filename);
		}

		private static string GetModInfoPath()
		{
			return Path.Combine(GetModPath(), "mod.json");
		}

		private static void LoadConfig()
		{
			var configPath = GetAssetPath("config.json");
			config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
			if (config == null)
			{
				config = new Config();
			}
		}
	}
}