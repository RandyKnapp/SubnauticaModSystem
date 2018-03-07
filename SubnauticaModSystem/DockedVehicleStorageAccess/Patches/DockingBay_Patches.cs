using Common.Mod;
using Harmony;
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
	[HarmonyPatch("Start")]
	class VehicleDockingBay_Start_Patch
	{
		private static readonly MethodInfo CyclopsVehicleStorageTerminalManager_OnDockedChanged = typeof(CyclopsVehicleStorageTerminalManager).GetMethod("OnDockedChanged", BindingFlags.NonPublic | BindingFlags.Instance);

		private static GameObject prefab;
		private static bool requestingPrefab;

		private static void Postfix(VehicleDockingBay __instance)
		{
			if (__instance.subRoot is BaseRoot)
			{
				__instance.StartCoroutine(CreateConsole(__instance));
			}
		}

		private static IEnumerator CreateConsole(VehicleDockingBay __instance)
		{
			if (prefab == null)
			{
				yield return CreatePrefab();
			}

			GameObject console = GameObject.Instantiate(prefab);
			console.transform.SetParent(__instance.transform, false);
			console.transform.localPosition = new Vector3(4.96f, 1.4f, 3.23f);
			console.transform.localEulerAngles = new Vector3(0, 42.5f, 0);

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
			GameObject.Destroy(cyclops);
		}

		private static void OnPrefabDestroyed(GameObject x)
		{
			prefab = null;
		}
	}
}
