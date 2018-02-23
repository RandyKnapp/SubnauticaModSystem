using Common.Mod;
using Common.Utility;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutosortLockers
{
	public enum CustomTechType
	{
		AutosortLocker = 11110,
		AutosortTarget
	}

	static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			DevConsole.disableConsole = false;

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
	}
}