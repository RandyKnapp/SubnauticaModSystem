using Common.Mod;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HabitatControlPanel.Patches
{
	[HarmonyPatch(typeof(KnownTech))]
	[HarmonyPatch("Initialize")]
	class KnownTech_Initialize_Patch
	{
		private static bool initialized = false;

		private static void Postfix()
		{
			if (initialized)
			{
				return;
			}
			initialized = true;

			BuilderUtils.OnKnownTechInitialized();
		}
	}
}
