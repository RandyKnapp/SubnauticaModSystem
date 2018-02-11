using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MoreQuickSlots
{
    public static class Mod
    {
        public static Config config;

        public static void Patch()
        {
            LoadConfig();

            HarmonyInstance harmony = HarmonyInstance.Create("com.morequickslots.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Controller.Load();

            Logger.Log("Initialized");
        }

        private static void LoadConfig()
        {
            string modDirectory = Environment.CurrentDirectory + @"\QMods";
            string settingsPath = modDirectory + @"\mod.json";

            if (!File.Exists(settingsPath))
            {
                Logger.Log("Could not find mod.json");
                config = new Config();
                return;
            }

            string modInfoJson = File.ReadAllText(settingsPath);
            Logger.Log("modInfoJson = " + modInfoJson);

            ModInfo modInfo = JsonUtility.FromJson<ModInfo>(modInfoJson);

            var N = JSON.Parse(modInfoJson);
            var configObject = N["config"];
            string configJson = configObject.ToString();
            Logger.Log("configJson = " + configJson);

            config = JsonUtility.FromJson<Config>(configJson);
            ValidateConfig();
        }

        private static void ValidateConfig()
        {
            Config defaultConfig = new Config();
            if (config == null)
            {
                Logger.Log("Config was missing from mod.json");
                config = defaultConfig;
                return;
            }

            if (config.SlotCount < 1 || config.SlotCount > 12)
            {
                config.SlotCount = defaultConfig.SlotCount;
            }
        }
    }
}