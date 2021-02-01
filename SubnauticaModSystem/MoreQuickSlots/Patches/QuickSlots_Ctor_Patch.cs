using HarmonyLib;
using System;
using UnityEngine;

namespace MoreQuickSlots.Patches
{
	[HarmonyPatch(typeof(QuickSlots), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(GameObject), typeof(Transform), typeof(Transform), typeof(Inventory), typeof(Transform), typeof(int) })]
	class QuickSlots_Ctor_Patch
	{
		[HarmonyPrefix]
		static void Prefix(ref int slotCount, ref string[] ___slotNames)
		{
			slotCount = Mod.config.SlotCount;

			string[] newSlotNames = new string[slotCount];
			for (int i = 0; i < slotCount; ++i)
			{
				newSlotNames[i] = "QuickSlot" + i;
			}
			___slotNames = newSlotNames;
		}
	}
}

