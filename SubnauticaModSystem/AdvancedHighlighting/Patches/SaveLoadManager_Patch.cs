using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedHighlighting.Patches
{
  [HarmonyPatch(typeof(SaveLoadManager), "NotifySaveInProgress", new Type[] { typeof(bool) })]
  public static class SaveLoadManager_NotifySaveInProgress_Patch
  {
    public static void Postfix(SaveLoadManager __instance, bool isInProgress)
    {
      if (!Settings.GetInstance().ShowPings)
        return;
      if (isInProgress)
      {
        foreach (var pingKey in Highlighting_OnUpdate_Patch.pings.Keys)
        {
          var ping = PingManager.Get(Highlighting_OnUpdate_Patch.pings[pingKey]);
          if (ping != null)
            UnityEngine.Object.Destroy(ping);
        }
        Highlighting_OnUpdate_Patch.pings.Clear();
      }
      else
      {
        Highlighting_OnUpdate_Patch.stories.Clear();
      }
    }
  }
}