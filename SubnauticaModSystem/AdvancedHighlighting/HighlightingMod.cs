using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using System;
#if SN
#elif BZ
#endif

namespace AdvancedHighlighting
{
  [QModCore]

  public static class HighlightingMod
  {
    [QModPatch]
    public static void InitMod()
    {
      Console.WriteLine("[AdvancedHighlighting] Start Patching...");
      Harmony harmony = new Harmony("net.ogmods.highlighting");
      harmony.PatchAll();
    }
  }
}