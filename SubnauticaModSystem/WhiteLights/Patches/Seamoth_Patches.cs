using Harmony;
using UnityEngine;

namespace WhiteLights.Patches
{
	[HarmonyPatch(typeof(SeaMoth))]
	[HarmonyPatch("Awake")]
	class Seamoth_Awake_Patch
	{
		private static void Postfix(SeaMoth __instance)
		{
			var lights = __instance.toggleLights.lightsParent.GetComponentsInChildren<Light>();
			foreach (var light in lights)
			{
				if (light.gameObject.name.Contains("left"))
				{
					light.color = Mod.config.SeamothLeft.ToColor();
				}
				else
				{
					light.color = Mod.config.SeamothRight.ToColor();
				}
			}
		}
	}
}
