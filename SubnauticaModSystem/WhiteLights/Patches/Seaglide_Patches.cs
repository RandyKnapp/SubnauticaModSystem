using Harmony;
using UnityEngine;

namespace WhiteLights.Patches
{
	[HarmonyPatch(typeof(Seaglide))]
	[HarmonyPatch("Start")]
	class Seaglide_Start_Patch
	{
		private static bool Prefix(Seaglide __instance)
		{
			var lights = __instance.toggleLights.lightsParent.GetComponentsInChildren<Light>();
			foreach (var light in lights)
			{
				light.color = Mod.config.Seaglide.ToColor();
			}
			return true;
		}
	}
}
 