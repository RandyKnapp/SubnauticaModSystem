using Common.Mod;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CustomizedStorage.Patches
{
	[HarmonyPatch(typeof(StorageContainer))]
	[HarmonyPatch("Awake")]
	class StorageContainer_Awake_Patch
	{
		private static List<string> names = new List<string>();

		private static bool Prefix(StorageContainer __instance)
		{
			if (IsSmallLocker(__instance))
			{
				SetSize(__instance, Mod.config.SmallLocker);
			}
			else if (IsLargeLocker(__instance))
			{
				SetSize(__instance, Mod.config.Locker);
			}

			return true;
		}

		private static void SetSize(StorageContainer __instance, Size size)
		{
			__instance.width = size.width;
			__instance.height = size.height;
		}

		private static bool IsSmallLocker(StorageContainer __instance)
		{
			return __instance.gameObject.name.StartsWith("SmallLocker");
		}

		private static bool IsLargeLocker(StorageContainer __instance)
		{
			return __instance.gameObject.name.StartsWith("Locker");
		}
	}

	[HarmonyPatch(typeof(Inventory))]
	[HarmonyPatch("Awake")]
	class Inventory_Awake_Patch
	{
		private static readonly FieldInfo Inventory_container = typeof(Inventory).GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance);

		private static void Postfix(Inventory __instance)
		{
			var container = (ItemsContainer)Inventory_container.GetValue(__instance);
			container.Resize(Mod.config.Inventory.width, Mod.config.Inventory.height);
		}
	}

	[HarmonyPatch(typeof(SeamothStorageContainer))]
	[HarmonyPatch("Init")]
	class SeamothStorageContainer_Init_Patch
	{
		private static bool Prefix(SeamothStorageContainer __instance)
		{
			__instance.width = Mod.config.SeamothStorage.width;
			__instance.height = Mod.config.SeamothStorage.height;
			return true;
		}
	}

	[HarmonyPatch(typeof(Exosuit))]
	[HarmonyPatch("UpdateStorageSize")]
	class Exosuit_UpdateStorageSize_Patch
	{
		private static void Postfix(Exosuit __instance)
		{
			int height = Mod.config.Exosuit.baseHeight + (Mod.config.Exosuit.heightPerStorageModule * __instance.modules.GetCount(TechType.VehicleStorageModule));
			__instance.storageContainer.Resize(Mod.config.Exosuit.width, height);
		}
	}

	[HarmonyPatch(typeof(BaseBioReactor))]
	[HarmonyPatch("get_container")]
	class BaseBioReactor_get_container_Patch
	{
		private static readonly FieldInfo BaseBioReactor_container = typeof(BaseBioReactor).GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance);

		private static void Postfix(BaseBioReactor __instance)
		{
			ItemsContainer container = (ItemsContainer)BaseBioReactor_container.GetValue(__instance);
			container.Resize(Mod.config.BioReactor.width, Mod.config.BioReactor.height);
		}
	}

	[HarmonyPatch(typeof(FiltrationMachine))]
	[HarmonyPatch("Start")]
	class FiltrationMachine_Start_Patch
	{
		private static void Postfix(FiltrationMachine __instance)
		{
			__instance.maxSalt = Mod.config.FiltrationMachine.maxSalt;
			__instance.maxWater = Mod.config.FiltrationMachine.maxWater;
			__instance.storageContainer.Resize(Mod.config.FiltrationMachine.width, Mod.config.FiltrationMachine.height);
		}
	}

	[HarmonyPatch(typeof(uGUI_ItemsContainer))]
	[HarmonyPatch("OnResize")]
	class uGUI_ItemsContainer_OnResize_Patch
	{
		private static void Postfix(uGUI_ItemsContainer __instance, int width, int height)
		{
			var x = __instance.rectTransform.anchoredPosition.x;
			if (height == 9)
			{
				__instance.rectTransform.anchoredPosition = new Vector2(x, -39);
			}
			else if (height == 10)
			{
				__instance.rectTransform.anchoredPosition = new Vector2(x, -75);
			}
			else
			{
				__instance.rectTransform.anchoredPosition = new Vector2(x, -4);
			}

			var y = __instance.rectTransform.anchoredPosition.y;
			var sign = Mathf.Sign(x);
			if (width == 8)
			{
				__instance.rectTransform.anchoredPosition = new Vector2(sign * (284 + 8), y);
			}
			else
			{
				__instance.rectTransform.anchoredPosition = new Vector2(sign * 284, y);

			}
		}
	}
}
 
 
 