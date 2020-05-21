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
			var max = new Size(10000, 10000);

            config.Inventory = ValidateConfigValue("Inventory", min, max, config.Inventory, defaultConfig.Inventory);
			config.SmallLocker = ValidateConfigValue("SmallLocker", min, max, config.SmallLocker, defaultConfig.SmallLocker);
			config.Locker = ValidateConfigValue("Locker", min, max, config.Locker, defaultConfig.Locker);
			config.EscapePodLocker = ValidateConfigValue("EscapePodLocker", min, max, config.EscapePodLocker, defaultConfig.EscapePodLocker);
			config.CyclopsLocker = ValidateConfigValue("CyclopsLocker", min, max, config.CyclopsLocker, defaultConfig.CyclopsLocker);
			config.WaterproofLocker = ValidateConfigValue("WaterproofLocker", min, max, config.WaterproofLocker, defaultConfig.WaterproofLocker);
			config.CarryAll = ValidateConfigValue("CarryAll", min, max, config.CarryAll, defaultConfig.CarryAll);
			config.SeamothStorage = ValidateConfigValue("SeamothStorage", min, max, config.SeamothStorage, defaultConfig.SeamothStorage);

			config.Exosuit.width = ValidateConfigValue("Exosuit.width", min.width, max.width, config.Exosuit.width, defaultConfig.Exosuit.width);
			config.Exosuit.baseHeight = ValidateConfigValue("Exosuit.baseHeight", min.height, max.height, config.Exosuit.baseHeight, defaultConfig.Exosuit.baseHeight);

            config.FiltrationMachine.width = ValidateConfigValue("FiltrationMachine.width", min.width, max.width, config.FiltrationMachine.width, defaultConfig.FiltrationMachine.width);
            config.FiltrationMachine.height = ValidateConfigValue("FiltrationMachine.height", min.height, max.height, config.FiltrationMachine.height, defaultConfig.FiltrationMachine.height);
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
        
        public static T ValidateConfigValue<T>(string field, T min, T max, T value, T defaultValue) where T : IComparable
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                Console.WriteLine("Config value for '{0}' ({1}) was not valid. Must be between {2} and {3}",
                    field,
                    value,
                    min,
                    max
                );
                return defaultValue;
            }
            return value;
        }
    }
}