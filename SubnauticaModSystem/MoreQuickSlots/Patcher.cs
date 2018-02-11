using Harmony;
using System;
using System.Reflection;

namespace MoreQuickSlots
{
    public static class Patcher
    {
        public static void Patch()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.morequickslots.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger.Log("Patched");

            Controller.Load();
        }
    }
}