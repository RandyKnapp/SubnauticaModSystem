using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace MoreQuickSlots.Patches
{
	[HarmonyPatch(typeof(QuickSlots), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(GameObject), typeof(Transform), typeof(Transform), typeof(Inventory), typeof(Transform), typeof(int) })]
	class QuickSlots_Ctor_Patch
	{
		static bool Prefix(ref int slotCount)
		{
			slotCount = Mod.config.SlotCount;

			string[] newSlotNames = new string[slotCount];
			for (int i = 0; i < slotCount; ++i)
			{
				newSlotNames[i] = "QuickSlot" + i;
			}
			typeof(QuickSlots).GetField("slotNames", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, newSlotNames);

			return true;
		}
	}
}

