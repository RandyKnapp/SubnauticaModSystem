using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AdvancedHighlighting.Patches
{
    public static class uGUI_Pings_Patch
    {
        private static readonly FieldInfo pingsField = typeof(uGUI_Pings).GetField("pings", BindingFlags.Instance | BindingFlags.NonPublic);
        public static Dictionary<string, uGUI_Ping> GetPings(uGUI_Pings instance) =>
             (Dictionary<string, uGUI_Ping>)pingsField.GetValue(instance);
    }

    [HarmonyPatch(typeof(uGUI_Pings), "OnAdd", new Type[] { typeof(PingInstance) })]
    public static class uGUI_Pings_OnAdd_Patch
    {
        public static void Postfix(uGUI_Pings __instance, PingInstance instance)
        {
            var id = instance._id;
            if (!id.StartsWith("ahping_")) return;
            var pings = uGUI_Pings_Patch.GetPings(__instance);
            if (pings != null && pings.TryGetValue(id, out uGUI_Ping ping))
            {
                //Console.WriteLine($"[AdvancedHighlighting] OnAdd Ping {id} {Utils.GetEyeSprite(out _)}...");
                if (Utils.GetColorForType(id.Split('_')[1], out Color newColor))
                    ping.SetColor(newColor);
                if (Utils.GetEyeSprite(out Sprite sprite))
                    ping.SetIcon(sprite);
            }
        }
    }

    [HarmonyPatch(typeof(uGUI_Pings), "OnColor", new Type[] { typeof(string), typeof(Color) })]
    public static class uGUI_Pings_OnColor_Patch
    {
        public static void Postfix(uGUI_Pings __instance, string id, Color color)
        {
            if (id.StartsWith("ahping_"))
            {
                //Console.WriteLine($"[AdvancedHighlighting] OnColor Ping {id}, {pings?.Count}...");
                var pings = uGUI_Pings_Patch.GetPings(__instance);
                if (pings != null && pings.TryGetValue(id, out uGUI_Ping ping))
                    if (Utils.GetColorForType(id.Split('_')[1], out Color newColor))
                        ping.SetColor(newColor);
            }
        }
    }
}
