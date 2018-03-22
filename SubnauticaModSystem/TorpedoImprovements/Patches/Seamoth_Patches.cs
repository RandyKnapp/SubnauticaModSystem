using Harmony;
using UnityEngine;

namespace TorpedoImprovements.Patches
{
	[HarmonyPatch(typeof(SeaMoth))]
	[HarmonyPatch("Start")]
	class Seamoth_Start_Patch
	{
		private static bool Prefix(SeaMoth __instance)
		{
			return true;
		}
	}
}
