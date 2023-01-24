using HarmonyLib;
using System;
using UnityEngine;

namespace MoreQuickSlots.Patches
{
	[HarmonyPatch(typeof(QuickSlots), MethodType.Constructor, new Type[] { typeof(GameObject), typeof(Transform), typeof(Transform), typeof(Inventory), typeof(Transform), typeof(int) })]
	public static class QuickSlots_Ctor_Patch
	{
		[HarmonyPrefix]
		public static void Prefix(ref int slotCount, ref string[] ___slotNames)
		{
			slotCount = MoreQuickSlots.SlotCount.Value;

			var newSlotNames = new string[slotCount];
			for (var i = 0; i < slotCount; ++i)
			{
				newSlotNames[i] = "QuickSlot" + i;
			}
			___slotNames = newSlotNames;
		}
	}
}

