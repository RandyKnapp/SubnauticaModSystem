using Common.Mod;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseTeleporters.Patches
{
	[HarmonyPatch(typeof(PrecursorTeleporter))]
	[HarmonyPatch("Start")]
	class PrecursorTeleporter_Start_Patch
	{
		private static bool once;

		private static void Postfix(PrecursorTeleporter __instance)
		{
			if (once)
			{
				return;
			}
			once = true;

			Logger.Log("Teleporter:");
			ModUtils.PrintObject(__instance.gameObject);
		}
	}
}
