using Common.Utility;
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
		private static string[] keys = new string[MaxSlots];

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			keys[5] = config.Slot6Key;
			keys[6] = config.Slot7Key;
			keys[7] = config.Slot8Key;
			keys[8] = config.Slot9Key;
			keys[9] = config.Slot10Key;
			keys[10] = config.Slot11Key;
			keys[11] = config.Slot12Key;

			HarmonyInstance harmony = HarmonyInstance.Create("com.morequickslots.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
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

			if (config.SlotCount < MinSlots || config.SlotCount > MaxSlots)
			{
				config.SlotCount = defaultConfig.SlotCount;
			}
		}

		public static string GetInputForSlot(int slotID)
		{
			if (slotID < Player.quickSlotButtonsCount)
			{
				return GameInput.GetBindingName(GameInput.Button.Slot1 + slotID, GameInput.BindingSet.Primary);
			}
			if (slotID < 0 || slotID >= MaxSlots)
			{
				return "???";
			}

			return keys[slotID];
		}

		public static bool GetKeyDownForSlot(int slotID)
		{
			return slotID >= Player.quickSlotButtonsCount && slotID < Mod.MaxSlots && Input.GetKeyDown(Mod.GetInputForSlot(slotID));
		}
	}
}