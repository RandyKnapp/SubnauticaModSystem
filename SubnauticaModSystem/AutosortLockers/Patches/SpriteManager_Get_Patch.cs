using AutosortLockers;
using Common.Mod;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutosortLockers.Patches
{
	[HarmonyPatch(typeof(SpriteManager))]
	[HarmonyPatch("Get")]
	[HarmonyPatch(new Type[] { typeof(TechType) } )]
	class SpriteManager_Get_Patch
	{
		private static bool Prefix(ref Atlas.Sprite __result, TechType techType)
		{
			CustomTechInfo techInfo = BuilderUtils.GetCustomTechData(techType);
			if (techInfo != null)
			{
				__result = techInfo.sprite;
				return false;
			}

			return true;
		}
	}
}
