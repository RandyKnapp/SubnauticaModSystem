using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MoreQuickSlots
{
	public static class Mod
	{
		public const int MaxSlots = 12;
		public const int MinSlots = 1;

		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory)
		{
			Mod.modDirectory = modDirectory;
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.morequickslots.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			GameController.Load();

			Logger.Log("Initialized");
		}

		private static string GetModInfoPath()
		{
			return Environment.CurrentDirectory + "\\" + modDirectory + "\\mod.json";
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
			string configJson = modInfoObject["config"].ToString();
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

			if (config.SlotCount < MinSlots || config.SlotCount > MaxSlots)
			{
				config.SlotCount = defaultConfig.SlotCount;
			}
		}

		public static KeyCode GetKeyCodeForSlot(int slotID)
		{
			if (slotID == 9) return KeyCode.Alpha0;
			if (slotID == 10) return KeyCode.Minus;
			if (slotID == 11) return KeyCode.Equals;
			else return KeyCode.Alpha1 + slotID;
		}

		public static string GetHintTextForSlot(int slotID)
		{
			if (slotID == 9) return "0";
			if (slotID == 10) return "-";
			if (slotID == 11) return "=";
			else return (slotID + 1).ToString();
		}
	}
}