using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace AdvancedHighlighting.Patches
{
  public static class PingInstance_Patch
  {
    private static readonly FieldInfo fakePositionUsedField = typeof(PingInstance).GetField("fakePositionUsed", BindingFlags.Instance | BindingFlags.NonPublic);
    public static bool GetFakePositionUsed(PingInstance instance) =>
       (bool)fakePositionUsedField.GetValue(instance);
  }
  [HarmonyPatch(typeof(PingInstance), "GetPosition")]
  public static class PingInstance_GetPosition_Patch
  {
    public static bool Prefix(PingInstance __instance, ref Vector3 __result)
    {
      if (__instance == null || (__instance.origin == null && !PingInstance_Patch.GetFakePositionUsed(__instance)))
      {
        if (__instance != null && Settings.GetInstance().DestroyNullPings)
        {
          UnityEngine.Object.Destroy(__instance);
          Console.WriteLine($"[AdvancedHighlighting] Destroying {__instance._id}...");
        }
        __result = Vector3.zero;
        return false;
      }
      return true;
    }
  }
}