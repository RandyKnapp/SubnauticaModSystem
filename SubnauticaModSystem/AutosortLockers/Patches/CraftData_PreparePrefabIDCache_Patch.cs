using Common.Mod;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutosortLockers.Patches
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

			BuilderUtils.OnTechMappingInitialized();

			initialized = true;
		}
	}
}
