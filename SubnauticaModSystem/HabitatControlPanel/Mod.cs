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

namespace HabitatControlPanel
{
	public enum CustomTechType
	{
		HabitatControlPanel = 11120
	}

	static class Mod
	{
		public const string SaveDataFilename = "HabitatControlPanelSaveData.json";
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			AddBuildables();

			HarmonyInstance harmony = HarmonyInstance.Create("com.HabitatControlPanel.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			ProtobufSerializerPatcher.Patch(harmony);

			Logger.Log("Patched");
		}

		public static void AddBuildables()
		{
			HabitatControlPanel.AddBuildable();
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