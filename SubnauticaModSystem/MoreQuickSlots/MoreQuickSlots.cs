using HarmonyLib;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using UnityEngine;

namespace MoreQuickSlots
{
    [BepInPlugin(PluginId, DisplayName, Version)]
	public class MoreQuickSlots : BaseUnityPlugin
    {
		public const string PluginId = "randyknapp.mods.morequickslots";
        public const string DisplayName = "MoreQuickSlots";
        public const string Version = "2.0.0";

		public const int MaxSlots = 12;
		public const int MinSlots = 1;

        public static ConfigEntry<int> SlotCount;
        public static ConfigEntry<bool> ShowInputText;
        public static readonly ConfigEntry<string>[] KeyBinds = new ConfigEntry<string>[MaxSlots];

        private Harmony _harmony;

		[UsedImplicitly]
        public void Awake()
        {
			SlotCount = Config.Bind("Configuration", "SlotCount", 12, "Number of quick slots to show. Max = 12");
            ShowInputText = Config.Bind("Configuration", "ShowInputText", true, "If true, show a label for the hotkey for each quick slot.");
			KeyBinds[5]	= Config.Bind("Key Bindings", "Slot6Key", "6", "Keybind for Quick Slot 6");
			KeyBinds[6]	= Config.Bind("Key Bindings", "Slot7Key", "7", "Keybind for Quick Slot 7");
			KeyBinds[7]	= Config.Bind("Key Bindings", "Slot8Key", "8", "Keybind for Quick Slot 8");
			KeyBinds[8]	= Config.Bind("Key Bindings", "Slot9Key", "9", "Keybind for Quick Slot 9");
			KeyBinds[9]	= Config.Bind("Key Bindings", "Slot10Key", "0", "Keybind for Quick Slot 10");
			KeyBinds[10] = Config.Bind("Key Bindings", "Slot11Key", "-", "Keybind for Quick Slot 11");
			KeyBinds[11] = Config.Bind("Key Bindings", "Slot12Key", "=", "Keybind for Quick Slot 12");

            if (SlotCount.Value < MinSlots)
                SlotCount.Value = MinSlots;
            if (SlotCount.Value > MaxSlots)
                SlotCount.Value = MaxSlots;

			_harmony = new Harmony(PluginId);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

		[UsedImplicitly]
        public void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

		public static string GetInputForSlot(int slotID)
		{
			if (slotID < Player.quickSlotButtonsCount)
			{
				var inputName = GameInput.GetBindingName(GameInput.Button.Slot1 + slotID, GameInput.BindingSet.Primary);
				var input = LanguageCache.GetButtonFormat("{0}", GameInput.Button.Slot1 + slotID);
				return string.IsNullOrEmpty(inputName) ? "" : input;
			}
            return slotID < 0 || slotID >= MaxSlots ? "???" : KeyBinds[slotID].Value;
        }

        public static bool GetKeyDownForSlot(int slotID)
		{
			return slotID >= Player.quickSlotButtonsCount && slotID < MaxSlots && Input.GetKeyDown(GetInputForSlot(slotID));
		}
	}
}