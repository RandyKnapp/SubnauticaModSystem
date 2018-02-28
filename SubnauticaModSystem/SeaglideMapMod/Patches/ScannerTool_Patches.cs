using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SeaglideMapMod.Patches
{
	[HarmonyPatch(typeof(ScannerTool))]
	[HarmonyPatch("Update")]
	class ScannerTool_Update_Patch
	{
		private static readonly MethodInfo PlayerTool_get_isDrawn = typeof(PlayerTool).GetMethod("get_isDrawn", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo ScannerTool_idleTimer = typeof(ScannerTool).GetField("idleTimer", BindingFlags.NonPublic | BindingFlags.Instance);

		private static bool Prefix(ScannerTool __instance)
		{
			if (!Mod.config.FixeScannerToolTextBug)
			{
				return true;
			}

			bool isDrawn = (bool)PlayerTool_get_isDrawn.Invoke(__instance, new object[] { });
			if (isDrawn)
			{
				float idleTimer = (float)ScannerTool_idleTimer.GetValue(__instance);
				if (idleTimer > 0f)
				{
					float newValue = Mathf.Max(0f, idleTimer - Time.deltaTime);
					ScannerTool_idleTimer.SetValue(__instance, newValue);
				}
			}
			return false;
		}
	}
}
