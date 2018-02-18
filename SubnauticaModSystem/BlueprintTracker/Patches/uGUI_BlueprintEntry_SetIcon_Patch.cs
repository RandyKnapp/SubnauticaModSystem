using Harmony;

namespace BlueprintTracker.Patches
{
	[HarmonyPatch(typeof(uGUI_BlueprintEntry))]
	[HarmonyPatch("SetIcon")]
	public static class uGUI_BlueprintEntry_SetIcon_Patch
	{
		private static void Postfix(uGUI_BlueprintEntry __instance, TechType techType)
		{
			__instance.gameObject.AddComponent<BlueprintTrackerPdaEntry>().techType = techType;
		}
	}
}
