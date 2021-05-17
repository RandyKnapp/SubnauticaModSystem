using System;
using System.Reflection;
using Common.Mod;
using HarmonyLib;
using QModManager.API;
using SMLHelper.V2.Handlers;

namespace DockedVehicleStorageAccess
{
	internal static class Mod
	{
#if !BELOWZERO
		public static Config config;
#else
		public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();
#endif

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Logger.Log("Starting patching");

			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			AddBuildables();

			new Harmony("com.DockedVehicleStorageAccessSML.mod").PatchAll(Assembly.GetExecutingAssembly());

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
#if !BELOWZERO
			config = ModUtils.LoadConfig<Config>(GetModPath() + "/config.json");
#endif
			config.UseAutosortMod = QModServices.Main.ModPresent("AutosortLockersSML");

			if (config.UseAutosortMod)
				Logger.Log("AutosortLockersSML detected. Cross-mod features enabled.");
			else
				Logger.Log("Running in standalone mode.");
		}
	}
}