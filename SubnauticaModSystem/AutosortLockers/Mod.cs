using Common.Mod;
using Harmony;
using System;
using System.Reflection;

namespace AutosortLockers
{
	static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.AutosortLockers.mod");
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
	}
}