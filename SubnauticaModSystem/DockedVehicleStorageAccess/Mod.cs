using Common.Mod;
using Common.Utility;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Oculus.Newtonsoft.Json;

namespace DockedVehicleStorageAccess
{
	public enum CustomTechType
	{
		DockedVehicleStorageAccess = 11130
	}

	static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			AddBuildables();

			HarmonyInstance harmony = HarmonyInstance.Create("com.DockedVehicleStorageAccess.mod");
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

		private static string GetModInfoPath()
		{
			return GetModPath() + "/mod.json";
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