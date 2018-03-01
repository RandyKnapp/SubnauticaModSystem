using Common.Mod;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HabitatControlPanel.Patches
{
	[HarmonyPatch(typeof(CraftData))]
	[HarmonyPatch("PreparePrefabIDCache")]
	class CraftData_PreparePrefabIDCache_Patch
	{
		private static bool initialized = false;

		private static void Postfix()
		{
			if (initialized)
			{
				return;
			}
			initialized = true;

			//BuilderUtils.OnTechMappingInitialized();
		}
	}
}
