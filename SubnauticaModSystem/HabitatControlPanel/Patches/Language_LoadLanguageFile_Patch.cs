using Common.Mod;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HabitatControlPanel.Patches
{
	[HarmonyPatch(typeof(Language))]
	[HarmonyPatch("LoadLanguageFile")]
	class Language_LoadLanguageFile_Patch
	{
		private static void Postfix()
		{
			BuilderUtils.OnLanguageStringsInitialized();
		}
	}
}
