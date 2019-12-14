using Common.Mod;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CustomizedStorage
{
	static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.CustomizedStorage.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		public static string GetModPath()
		{
			return Environment.CurrentDirectory + "/" + modDirectory;
		}

		public static string GetAssetPath(string filename)
		{
			return GetModPath() + "/Assets/" + filename;
		}

		private static string GetConfigPath()
		{
			return GetModPath() + "/config.json";
		}

		private static void LoadConfig()
		{
			string filePath = GetConfigPath();
			if (!File.Exists(filePath))
			{
				config = new Config();
				File.WriteAllText(filePath, JsonConvert.SerializeObject(config, Formatting.Indented));
				return;
			}

			string configJson = File.ReadAllText(filePath);
			config = JsonConvert.DeserializeObject<Config>(configJson);
			if (config == null)
			{
				config = new Config();
			}

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
			var min = new Size(1, 1);
			var max = new Size(8, 10);

			ModUtils.ValidateConfigValue("Inventory", min, max, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("SmallLocker", min, max, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("Locker", min, max, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("EscapePodLocker", min, max, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("CyclopsLocker", min, max, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("WaterproofLocker", min, max, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("CarryAll", min, max, ref config, ref defaultConfig);
			ModUtils.ValidateConfigValue("SeamothStorage", min, max, ref config, ref defaultConfig);

			ModUtils.ValidateConfigValue("width", min.width, max.width, ref config.Exosuit, ref defaultConfig.Exosuit);
			ModUtils.ValidateConfigValue("baseHeight", min.height, max.height, ref config.Exosuit, ref defaultConfig.Exosuit);

			int exoMaxHeight = config.Exosuit.baseHeight + (config.Exosuit.heightPerStorageModule * 4);
			exoMaxHeight = Mathf.Min(max.height, exoMaxHeight);
			if (exoMaxHeight > max.height)
			{
				Console.WriteLine("Config values for 'Exosuit' were not valid. Max height is {0} but the exosuit might exceed that. ({1})",
					max.height,
					"MaxHeight = baseHeight + (heightPerStorageModule * 4)"
				);
				config.Exosuit = defaultConfig.Exosuit;
			}

			ModUtils.ValidateConfigValue("width", min.width, max.width, ref config.FiltrationMachine, ref defaultConfig.FiltrationMachine);
			ModUtils.ValidateConfigValue("height", min.height, max.height, ref config.FiltrationMachine, ref defaultConfig.FiltrationMachine);
			var totalSpace = config.FiltrationMachine.width * config.FiltrationMachine.height;
			var totalContents = config.FiltrationMachine.maxSalt + config.FiltrationMachine.maxWater;
			if (totalContents > totalSpace)
			{
				Console.WriteLine("Config values for 'FiltrationMachine' were not valid. Total capacity is {0} but the maxWater and maxSalt combined are more than that. (maxSalt={1}, maxWater={2})",
					totalSpace,
					config.FiltrationMachine.maxSalt,
					config.FiltrationMachine.maxWater
				);
				config.FiltrationMachine.maxSalt = Mathf.FloorToInt(totalSpace / 2);
				config.FiltrationMachine.maxWater = Mathf.CeilToInt(totalSpace / 2);
			}
		}
	}
}