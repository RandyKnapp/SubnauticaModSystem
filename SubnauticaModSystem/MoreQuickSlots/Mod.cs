using HarmonyLib;
using QModManager.API.ModLoading;
using System.Reflection;
using UnityEngine;

namespace MoreQuickSlots
{
	[QModCore]
	public static class Mod
	{
		public const int MaxSlots = 12;
		public const int MinSlots = 1;

		public static Config config = new Config();

		private static readonly string[] keys = new string[MaxSlots];
		
		[QModPatch]
		public static void Patch()
		{
			keys[5] = config.Slot6Key;
			keys[6] = config.Slot7Key;
			keys[7] = config.Slot8Key;
			keys[8] = config.Slot9Key;
			keys[9] = config.Slot10Key;
			keys[10] = config.Slot11Key;
			keys[11] = config.Slot12Key;

			Harmony harmony = new Harmony("com.morequickslots.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		[QModPrePatch]
		public static void LoadConfig()
		{
			config.Load(true);
			ValidateConfig();
		}

		private static void ValidateConfig()
		{
			if (config.SlotCount < MinSlots || config.SlotCount > MaxSlots)
			{
				config.SlotCount = 12;
			}
		}

		public static string GetInputForSlot(int slotID)
		{
			if (slotID < Player.quickSlotButtonsCount)
			{
				string inputName = GameInput.GetBindingName(GameInput.Button.Slot1 + slotID, GameInput.BindingSet.Primary);
				string input = LanguageCache.GetButtonFormat("{0}", GameInput.Button.Slot1 + slotID);
				return string.IsNullOrEmpty(inputName) ? "" : input;
			}
            return slotID < 0 || slotID >= MaxSlots ? "???" : keys[slotID];
        }

        public static bool GetKeyDownForSlot(int slotID)
		{
			return slotID >= Player.quickSlotButtonsCount && slotID < Mod.MaxSlots && Input.GetKeyDown(Mod.GetInputForSlot(slotID));
		}
	}
}