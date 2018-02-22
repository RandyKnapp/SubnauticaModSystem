using Common.Mod;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutosortLockers.Patches
{
	[HarmonyPatch(typeof(CraftData))]
	[HarmonyPatch("Get")]
	public class CraftData_Get_Patch
	{
		private static bool Prefix(ref ITechData __result, ref TechType techType, ref bool skipWarnings)
		{
			ITechData result = BuilderUtils.GetTechData(techType);
			if (result != null)
			{
				__result = result;
				return false;
			}

			return true;
		}
	}
}
