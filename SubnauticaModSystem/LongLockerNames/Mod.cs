using Common.Utility;
using Harmony;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LongLockerNames
{
	static class Mod
	{
		public static Config config;
		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.LongLockerNames.mod");
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

			ValidateConfigValue("SmallLockerTextLimit", 1, 500, defaultConfig);
			ValidateConfigValue("SignTextLimit", 1, 500, defaultConfig);
		}

		private static void ValidateConfigValue<T>(string field, T min, T max, Config defaultConfig) where T : IComparable
		{
			var fieldInfo = typeof(Config).GetField(field, BindingFlags.Public | BindingFlags.Instance);
			T value = (T)fieldInfo.GetValue(config);
			if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
			{
				Logger.Log("Config value for '{0}' ({1}) was not valid. Must be between {2} and {3}",
					field,
					value,
					min,
					max
				);
				fieldInfo.SetValue(config, fieldInfo.GetValue(defaultConfig));
			}
		}

		public static void PrintObject(GameObject obj, string indent = "")
		{
			Console.WriteLine(indent + "[[" + obj.name + "]]:");
			Console.WriteLine(indent + "{");
			Console.WriteLine(indent + "  Components:");
			Console.WriteLine(indent + "  {");
			var lastC = obj.GetComponents<Component>().Last();
			foreach (var c in obj.GetComponents<Component>())
			{
				Console.WriteLine(indent + "    " + c.ToString().Replace(obj.name, "").Trim());
			}
			Console.WriteLine(indent + "  }");
			Console.WriteLine(indent + "  Children:");
			Console.WriteLine(indent + "  {");
			foreach (Transform child in obj.transform)
			{
				PrintObject(child.gameObject, indent + "    ");
			}
			Console.WriteLine(indent + "  }");
			Console.WriteLine(indent + "}");
		}
	}
}