using Harmony;
using System;
using UnityEngine;

namespace MoreQuickSlots.Patches
{
	[HarmonyPatch(typeof(QuickSlots), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(GameObject), typeof(Transform), typeof(Transform), typeof(Inventory), typeof(Transform), typeof(int) })]
	class QuickSlots_Ctor_Patch
	{
		static bool Prefix(ref int slotCount)
		{
			//Logger.Log("QuickSlots Ctor override");
			slotCount = Mod.config.SlotCount;
			return true;
		}
	}
}

