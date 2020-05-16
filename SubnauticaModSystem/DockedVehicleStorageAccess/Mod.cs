using System;
using System.Reflection;
using Common.Mod;
using Harmony;
using QModManager.API;

namespace DockedVehicleStorageAccess
{
	internal static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Logger.Log("Starting patching");

			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			AddBuildables();

			var harmony = HarmonyInstance.Create("com.DockedVehicleStorageAccessSML.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		public static void AddBuildables()
		{
			VehicleStorageAccess.AddBuildable();
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
			config.UseAutosortMod = QModServices.Main.ModPresent("AutosortLockersSML");

			if (config.UseAutosortMod)
				Logger.Log("AutosortLockersSML detected. Cross-mod features enabled.");
			else
				Logger.Log("Running in standalone mode.");
		}
	}
}