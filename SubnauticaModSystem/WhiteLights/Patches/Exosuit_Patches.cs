using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WhiteLights.Patches
{
	[HarmonyPatch(typeof(Exosuit))]
	[HarmonyPatch("Start")]
	class Exosuit_Start_Patch
	{
		private static bool Prefix(Exosuit __instance)
		{
			var lights = __instance.transform.Find("lights_parent").GetComponentsInChildren<Light>();
			foreach (var light in lights)
			{
				if (light.gameObject.name.Contains("left"))
				{
					light.color = Mod.config.PrawnsuitLeft.ToColor();
				}
				else
				{
					light.color = Mod.config.PrawnsuitRight.ToColor();
				}
			}
			return true;
		}
	}
}
