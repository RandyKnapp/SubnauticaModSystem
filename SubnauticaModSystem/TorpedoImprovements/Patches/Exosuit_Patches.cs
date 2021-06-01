using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TorpedoImprovements.Patches
{
	[HarmonyPatch(typeof(Exosuit))]
	[HarmonyPatch("Start")]
	class Exosuit_Start_Patch
	{
		private static bool Prefix(Exosuit __instance)
		{
			return true;
		}
	}
}
