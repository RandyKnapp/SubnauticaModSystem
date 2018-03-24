using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TorpedoImprovements.Patches
{
	static class Seamoth_Patches
	{
		private static readonly string[] SlotIDs = new string[]
		{
			"SeamothModule1",
			"SeamothModule2",
			"SeamothModule3",
			"SeamothModule4"
		};

		private static bool AllowTorpedoRemoval(Pickupable pickupable, bool verbose)
		{
			return true;
		}

		private static void OpenTorpedoStorageEx(this SeaMoth seamoth, int slotID, Transform useTransform)
		{
			bool hasTorpedoModules = seamoth.modules.GetCount(TechType.SeamothTorpedoModule) > 0;
			if (hasTorpedoModules)
			{
				ItemsContainer storageInSlot = seamoth.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
				if (storageInSlot != null)
				{
					Inventory.main.ClearUsedStorage();
					storageInSlot.isAllowedToRemove = new IsAllowedToRemove(AllowTorpedoRemoval);
					storageInSlot.Resize(Mod.config.TorpedoStorageWidth, Mod.config.TorpedoStorageHeight);
					Inventory.main.SetUsedStorage(storageInSlot, false);
					Player.main.GetPDA().Open(PDATab.Inventory, useTransform, null, -1f);
				}
			}
		}

		private static void OnHoverTorpedoStorageEx(this SeaMoth seamoth, int slotID, Transform useTransform)
		{
			var torpedoStorage = seamoth.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
			if (torpedoStorage != null)
			{
				var interactText = $"{Language.main.Get("SeamothTorpedoStorage")} {slotID + 1}";

				List<string> countText = new List<string>();
				foreach (var torpedoType in seamoth.torpedoTypes)
				{
					var typeName = Language.main.Get(torpedoType.techType);
					var count = torpedoStorage.GetCount(torpedoType.techType);
					if (count > 0)
					{
						countText.Add($"{typeName} x{count}");
					}
				}

				HandReticle.main.SetInteractTextRaw(interactText, countText.Count > 0 ? string.Join(", ", countText.ToArray()) : "empty");
				HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}


		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("Start")]
		class SeaMoth_Start_Patch
		{
			private static bool Prefix(SeaMoth __instance)
			{
				var seamoth = __instance;

				if (seamoth.GetComponent<PrimaryTorpedo>() == null)
				{
					var primary = seamoth.gameObject.AddComponent<PrimaryTorpedo>();
					primary.Types = seamoth.torpedoTypes.Select((t) => t.techType).ToList();
				}

				var left = __instance.gameObject.transform.Find("TorpedoSiloLeft").gameObject;
				var right = __instance.gameObject.transform.Find("TorpedoSiloRight").gameObject;

				var upperLeft = GameObject.Instantiate(left);
				upperLeft.name = "TorpedoSiloUpperLeft";
				upperLeft.transform.SetParent(__instance.transform, false);
				upperLeft.transform.localPosition += new Vector3(0, 0.2f, 0);

				var upperRight = GameObject.Instantiate(right);
				upperRight.name = "TorpedoSiloUpperRight";
				upperRight.transform.SetParent(__instance.transform, false);
				upperRight.transform.localPosition += new Vector3(0, 0.2f, 0);

				//Color[] colors = { Color.red, Color.yellow, Color.magenta, Color.cyan };
				var torpedoSilos = new GameObject[] { left, right, upperLeft, upperRight };
				for (var i = 0; i < torpedoSilos.Length; ++i)
				{
					var index = i;
					var silo = torpedoSilos[i];
					var handTarget = silo.GetComponent<GenericHandTarget>();
					handTarget.onHandHover.RemoveAllListeners();
					handTarget.onHandHover.AddListener((e) => {
						seamoth.OnHoverTorpedoStorageEx(index, e.transform);
					});

					handTarget.onHandClick.RemoveAllListeners();
					handTarget.onHandClick.AddListener((e) => {
						seamoth.OpenTorpedoStorageEx(index, e.transform);
					});

					var collider = silo.GetComponent<CapsuleCollider>();
					collider.height = collider.height / 2;
					collider.radius = collider.radius / 2;
					silo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

					/*var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
					capsule.transform.SetParent(silo.transform, false);
					capsule.GetComponent<MeshRenderer>().material.color = colors[i];*/
				}

				return true;
			}
		}


		/*if (base.GetPilotingMode())
		{
			string buttonFormat = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
			HandReticle.main.SetUseTextRaw(buttonFormat, string.Empty);
		}*/
		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("Update")]
		class SeaMoth_Update_Patch
		{
			private static void Postfix(SeaMoth __instance)
			{
				if (__instance.GetPilotingMode())
				{
					string key = GameInput.GetBindingName(GameInput.Button.Deconstruct, GameInput.BindingSet.Primary);
					string button2 = string.Format("Change Torpedo (<color=#ADF8FFFF>{0}</color>)", key);

					string buttonFormat = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
					HandReticle.main.SetUseTextRaw(buttonFormat, button2);
				}
			}
		}

		[HarmonyPatch(typeof(CyclopsVehicleStorageTerminalManager))]
		[HarmonyPatch("StorageButtonClick")]
		class CyclopsVehicleStorageTerminalManager_StorageButtonClick_Patch
		{
			private static readonly FieldInfo CyclopsVehicleStorageTerminalManager_currentVehicle = typeof(CyclopsVehicleStorageTerminalManager).GetField("currentVehicle", BindingFlags.NonPublic | BindingFlags.Instance);
			private static readonly FieldInfo CyclopsVehicleStorageTerminalManager_dockedVehicleType = typeof(CyclopsVehicleStorageTerminalManager).GetField("dockedVehicleType", BindingFlags.NonPublic | BindingFlags.Instance);

			private static bool Prefix(CyclopsVehicleStorageTerminalManager __instance, CyclopsVehicleStorageTerminalManager.VehicleStorageType type, int slotID)
			{
				var currentVehicle = (Vehicle)CyclopsVehicleStorageTerminalManager_currentVehicle.GetValue(__instance);
				if (currentVehicle == null)
				{
					return false;
				}

				if (type == CyclopsVehicleStorageTerminalManager.VehicleStorageType.Torpedo)
				{
					SeaMoth seamoth = currentVehicle.GetComponent<SeaMoth>();
					if (seamoth)
					{
						seamoth.OpenTorpedoStorageEx(slotID, __instance.transform);
						return false;
					}
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OpenTorpedoStorage")]
		class Seamoth_OpenTorpedoStorage_Patch
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnHoverTorpedoStorage")]
		class Seamoth_OnHoverTorpedoStorage_Patch
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnOpenTorpedoStorage")]
		class Seamoth_OnOpenTorpedoStorage_Patch
		{
			private static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnUpgradeModuleChange")]
		class Seamoth_OnUpgradeModuleChange_Patch
		{
			private static bool Prefix(SeaMoth __instance, int slotID, TechType techType, bool added)
			{
				if (techType == TechType.SeamothTorpedoModule)
				{
					var torpedoStorage = __instance.GetStorageInSlot(slotID, techType);
					torpedoStorage.SetAllowedTechTypes(__instance.torpedoTypes.Select((t) => t.techType).ToArray());
					torpedoStorage.Resize(Mod.config.TorpedoStorageWidth, Mod.config.TorpedoStorageHeight);
					if (slotID == 2 || slotID == 3)
					{
						GameObject gameObject = __instance.transform.Find(slotID == 2 ? "TorpedoSiloUpperLeft" : "TorpedoSiloUpperRight")?.gameObject;
						gameObject?.SetActive(added);
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("IsAllowedToRemove")]
		class Seamoth_IsAllowedToRemove_Patch
		{
			private static bool Prefix(SeaMoth __instance, ref bool __result, Pickupable pickupable, bool verbose)
			{
				TechType techType = pickupable.GetTechType();
				if (techType == TechType.SeamothTorpedoModule)
				{
					SeamothStorageContainer torpedoStorage = pickupable.GetComponent<SeamothStorageContainer>();
					if (torpedoStorage != null)
					{
						bool empty = torpedoStorage.container.count == 0;
						if (verbose && !empty)
						{
							ErrorMessage.AddDebug("Torpedo silo is not empty");
						}
						__result = empty;
						return false;
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(SeaMoth))]
		[HarmonyPatch("OnUpgradeModuleUse")]
		class Seamoth_OnUpgradeModuleUse_Patch
		{
			private static readonly FieldInfo Vehicle_quickSlotTimeUsed = typeof(Vehicle).GetField("quickSlotTimeUsed", BindingFlags.NonPublic | BindingFlags.Instance);
			private static readonly FieldInfo Vehicle_quickSlotCooldown = typeof(Vehicle).GetField("quickSlotCooldown", BindingFlags.NonPublic | BindingFlags.Instance);

			private static bool Prefix(SeaMoth __instance, TechType techType, int slotID)
			{
				if (techType == TechType.SeamothTorpedoModule)
				{
					Transform[] muzzles = {
						__instance.transform.Find("TorpedoSiloLeft"),
						__instance.transform.Find("TorpedoSiloRight"),
						__instance.transform.Find("TorpedoSiloUpperLeft"),
						__instance.transform.Find("TorpedoSiloUpperRight"),
					};
					Transform muzzle = muzzles[slotID];

					ItemsContainer storageInSlot = __instance.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
					if (storageInSlot.count == 0)
					{
						ErrorMessage.AddError(Language.main.Get("VehicleTorpedoNoAmmo"));
					}

					TorpedoType torpedoType = null;
					var primaryType = __instance.GetComponent<PrimaryTorpedo>().PrimaryTorpedoType;
					List<TorpedoType> torpedoTypes = __instance.torpedoTypes.ToList();
					torpedoTypes.Sort((a, b) => {
						return a.techType == b.techType ? 0 : (a.techType == primaryType ? -1 : 1);
					});
					foreach (var t in torpedoTypes)
					{
						if (storageInSlot.Contains(t.techType))
						{
							torpedoType = t;
							break;
						}
					}

					var firedTorpedo = Vehicle.TorpedoShot(storageInSlot, torpedoType, muzzle);
					if (firedTorpedo && storageInSlot.count == 0)
					{
						Utils.PlayFMODAsset(__instance.torpedoDisarmed, __instance.transform, 1f);
					}

					if (firedTorpedo)
					{
						if (torpedoType.techType == TechType.CyclopsDecoy)
						{

						}
						var quickSlotTimeUsed = (float[])Vehicle_quickSlotTimeUsed.GetValue(__instance);
						var quickSlotCooldown = (float[])Vehicle_quickSlotCooldown.GetValue(__instance);
						quickSlotTimeUsed[slotID] = Time.time;
						quickSlotCooldown[slotID] = Mod.config.TorpedoShotCooldown;
					}

					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(uGUI_SeamothHUD))]
		[HarmonyPatch("Update")]
		class uGUI_SeamothHUD_Update_Patch
		{
			private static bool Prefix(uGUI_SeamothHUD __instance)
			{
				var t = __instance.root.transform.parent.Find("TorpedoHud");
				if (t == null)
				{
					Logger.Log("Adding TorpeduHudController");
					var torpedoRoot = new GameObject("TorpedoHud", typeof(RectTransform)).AddComponent<TorpedoHudController>();
					RectTransformExtensions.SetParams(torpedoRoot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), __instance.root.transform.parent);
				}
				return true;
			}
		}
	}
}
