using QLogger = QModManager.Utility.Logger;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;


namespace BetterScannerBlips.Patches
{
	[HarmonyPatch(typeof(ResourceTracker))]
	public class ResourceTrackerPatches
	{
		internal static Dictionary<string, GameObject> IdToGameObjectDict = new Dictionary<string, GameObject>();
		internal static Dictionary<string, TechType> IdToTechTypeDict = new Dictionary<string, TechType>();
		internal static Dictionary<string, string> IdToResourceNameDict = new Dictionary<string, string>();
		internal static readonly FieldInfo uniqueIdInfo = typeof(ResourceTracker).GetField("uniqueId", BindingFlags.Instance | BindingFlags.NonPublic);
		internal static readonly FieldInfo TechTypeInfo = typeof(ResourceTracker).GetField("techType", BindingFlags.Instance | BindingFlags.NonPublic);

/*
		[HarmonyPatch(nameof(ResourceTracker.Start))]
		[HarmonyPrefix]
		public static bool PreStart(ref ResourceTracker __instance)
		{
			if (__instance.overrideTechType == TechType.Fragment)
			{
				__instance.overrideTechType = TechType.None;
			}

			return true;
		}
*/

		[HarmonyPatch("Register")]
		[HarmonyPrefix]
		public static void PreRegister(ref ResourceTracker __instance)
		{
			GameObject go = __instance.gameObject;
			TechType gameTechType = (go != null ? CraftData.GetTechType(go) : TechType.None);
			//TechType tt = ((__instance.overrideTechType == TechType.None) ? gameTechType : __instance.overrideTechType);
			if (gameTechType == TechType.Fragment || __instance.overrideTechType == TechType.Fragment) // We only need to concern ourselves with fragments
			{
				string uniqueId = __instance.prefabIdentifier.Id;
				QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"ResourceTrackerPatches.PreRegister() running on ResourceTracker with unique ID {uniqueId}, overrideTechType {__instance.overrideTechType} and techType {gameTechType}");

				if (!string.IsNullOrEmpty(uniqueId))
				{
					string resourceName = Language.main.Get(gameTechType);
					QLogger.Log(QLogger.Level.Debug, $"Registering resource names for fragment:\nUnique ID: {uniqueId}\nTechType: {gameTechType.AsString()}\nResource name: {resourceName}");
					IdToGameObjectDict[uniqueId] = __instance.gameObject;
					IdToTechTypeDict[uniqueId] = gameTechType;
					IdToResourceNameDict[uniqueId] = resourceName;
				}

				//__instance.overrideTechType = TechType.None;
			}
		}

		/*
		[HarmonyPatch("Register")]
		[HarmonyPostfix]
		public static void PostRegister(ref ResourceTracker __instance)
		{
			TechType tt = (TechType)TechTypeInfo.GetValue(__instance);
			if (tt == TechType.Fragment || __instance.overrideTechType == TechType.Fragment)
			{
				string uniqueId = __instance.prefabIdentifier.Id;
				GameObject go = __instance.gameObject;
				TechType resourceType = (go == null ? TechType.None : CraftData.GetTechType(go));
				//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"ResourceTrackerPatches.PostRegister() running on ResourceTracker with unique ID {uniqueId}, overrideTechType {__instance.overrideTechType} and techType {resourceType}");

				//TechType actualTechType = CraftData.GetTechType(__instance.gameObject);
				if (!string.IsNullOrEmpty(uniqueId))
				{
					if (__instance.gameObject != null)
					{
						IdToGameObjectDict[uniqueId] = __instance.gameObject;
					}
					IdToTechTypeDict[uniqueId] = resourceType;
					IdToResourceNameDict[uniqueId] = Language.main.Get(resourceType);
				}
			}

			//if((TechType)TechTypeInfo.GetValue(__instance) == TechType.Fragment)
			//CoroutineHost.StartCoroutine(RegisterResourceTrackerCoroutine(__instance));
		}
		*/

		[HarmonyPatch("Unregister")]
		[HarmonyPostfix]
		public static void PostUnregister(ref ResourceTracker __instance)
		{
			string uniqueId = (string)uniqueIdInfo.GetValue(__instance);
			if (!string.IsNullOrEmpty(uniqueId))
			{
				IdToGameObjectDict.Remove(uniqueId);
				IdToTechTypeDict.Remove(uniqueId);
				IdToResourceNameDict.Remove(uniqueId);
			}
		}

		public static TechType GetTechTypeForId(string uniqueId)
		{
			if (!string.IsNullOrEmpty(uniqueId))
			{
				if (IdToTechTypeDict.TryGetValue(uniqueId, out TechType tt))
				{
					if(tt != TechType.Fragment)
						return tt;
				}
				else if (IdToGameObjectDict.TryGetValue(uniqueId, out GameObject go))
				{
					if(go != null)
						return CraftData.GetTechType(go);
				}
			}

			return TechType.None;
		}

		public static bool TryGetResourceName(string uniqueId, out string resourceName)
		{
			bool result = IdToResourceNameDict.TryGetValue(uniqueId, out resourceName);

			QLogger.Log(QLogger.Level.Debug, $"Attempting to retrieve resource name for unique ID '{uniqueId}': Result is {result}" + (result ? $", retrieved string '{resourceName}'" : ""));

			return result;
		}
	}
}
