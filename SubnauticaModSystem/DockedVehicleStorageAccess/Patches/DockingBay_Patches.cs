using Common.Mod;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DockedVehicleStorageAccess.Patches
{
	[HarmonyPatch(typeof(VehicleDockingBay))]
	class VehicleDockingBay_Start_Patch
	{
		private static readonly MethodInfo CyclopsVehicleStorageTerminalManager_OnDockedChanged = typeof(CyclopsVehicleStorageTerminalManager).GetMethod("OnDockedChanged", BindingFlags.NonPublic | BindingFlags.Instance);

		private static GameObject prefab;
		private static bool requestingPrefab;

		[HarmonyPatch("Start")]
		private static void Postfix(VehicleDockingBay __instance)
		{
			// in SN1, VehicleDockingBay.subRoot will either be of type BaseRoot, if the docking bay in question is in a habitat, or of SubRoot, if it's in a Cyclops.
			// But subRoot always seems to be Null in BZ, so something else needs to be done

			// Of course, this assumes we can figure out an alternative to the Cyclops terminal used by default.
#if SN1
			//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"VehicleDockingBay.Start.Postfix: running on VehicleDockingBay {__instance.ToString()}");
			//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"VehicleDockingBay is of type {__instance.GetType().ToString()}");
			if (__instance.subRoot is BaseRoot)
			{
				//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"VehicleDockingBay is BaseRoot, creating console");
				__instance.StartCoroutine(CreateConsole(__instance));
			}
#elif BZ
			QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"Cyclops prefab not available in BZ, not attempting to add moonpool console");
			/*QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"VehicleDockingBay type is {__instance.dockType.ToString()}");
			if (__instance.dockType == Vehicle.DockType.Base)
			{
				QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"VehicleDockingBay type is correct, creating console");
			}*/
#endif
		}

		private static IEnumerator CreateConsole(VehicleDockingBay __instance)
		{
			if (prefab == null)
			{
				QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"CreateConsole: attempting to create prefab");
				yield return CreatePrefab();
			}

			GameObject console = GameObject.Instantiate(prefab);
			console.transform.SetParent(__instance.transform, false);
			console.transform.localPosition = new Vector3(4.96f, 1.4f, 3.23f);
			console.transform.localEulerAngles = new Vector3(0, 42.5f, 0);

			QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"Adding MoonpoolTerminalController component");
			console.AddComponent<MoonpoolTerminalController>();

			var terminalManager = console.GetComponent<CyclopsVehicleStorageTerminalManager>();
			terminalManager.dockingBay = __instance;

			// This resets and then calls the OnDockingChanged event handler
			console.SetActive(false);
			console.SetActive(true);
			CyclopsVehicleStorageTerminalManager_OnDockedChanged.Invoke(terminalManager, new object[] { });

			var mesh = console.transform.Find("Mesh").gameObject;
			var meshRenderer = mesh.GetComponent<MeshRenderer>();

			console.SetActive(false);
			var skyApplier = console.AddComponent<SkyApplier>();
			skyApplier.dynamic = true;
			skyApplier.renderers = new Renderer[] { meshRenderer };
			skyApplier.anchorSky = Skies.BaseInterior;
			console.SetActive(true);
		}

		private static IEnumerator CreatePrefab()
		{
			if (!requestingPrefab)
			{
				requestingPrefab = true;
				LightmappedPrefabs.main.RequestScenePrefab("cyclops", new LightmappedPrefabs.OnPrefabLoaded(OnPrefabLoaded));
			}
			
			while (prefab == null)
			{
				yield return null;
			}
		}

		private static void OnPrefabLoaded(GameObject cyclops)
		{
			prefab = GameObject.Instantiate(cyclops.transform.Find("CyclopsVehicleStorageTerminal").gameObject);
			prefab.AddComponent<CallbackOnDestroy>().onDestroy += OnPrefabDestroyed;
			prefab.transform.position = new Vector3(5000, 5000, 5000);

			requestingPrefab = false;
		}

		private static void OnPrefabDestroyed(GameObject x)
		{
			prefab = null;
		}
	}
}
