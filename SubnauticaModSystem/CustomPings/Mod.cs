using Common.Mod;
using Common.Utility;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Oculus.Newtonsoft.Json;

namespace CustomBeacons
{
	static class Mod
	{
		private const int StartingPingIndex = 100;

		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.CustomBeacons.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			CustomPings.AddPingColor(new Color32(112, 192, 65, 255));
			CustomPings.AddPingColor(new Color32(64, 192, 147, 255));
			CustomPings.AddPingColor(new Color32(179, 106, 226, 255));
			CustomPings.AddPingColor(new Color32(226, 105, 209, 255));
			CustomPings.AddPingColor(Color.red);
			CustomPings.AddPingColor(new Color32(255, 165, 0, 255));
			CustomPings.AddPingColor(Color.yellow);
			CustomPings.AddPingColor(Color.green);
			CustomPings.AddPingColor(Color.cyan);
			CustomPings.AddPingColor(Color.blue);
			CustomPings.AddPingColor(new Color32(131, 0, 255, 255));
			CustomPings.AddPingColor(Color.magenta);
			CustomPings.AddPingColor(Color.white);
			CustomPings.AddPingColor(Color.gray);
			CustomPings.AddPingColor(new Color32(60, 60, 60, 255));

			AddCustomPings();

			CustomPings.Initialize();

			Logger.Log("Patched");
		}

		private static void AddCustomPings()
		{
			var assetDir = GetAssetPath("Pings");

			int pingIndex = StartingPingIndex;
			foreach (var file in Directory.GetFiles(assetDir))
			{
				var name = Path.GetFileNameWithoutExtension(file);
				name = name.SubstringFromOccuranceOf("_", 0);
				CustomPings.AddPingType(pingIndex, name, new Atlas.Sprite(ImageUtils.LoadSprite(file, new Vector2(0.5f, 0.5f))));

				pingIndex++; 
			}
		}

		public static string GetModPath()
		{
			return Environment.CurrentDirectory + "\\" + modDirectory;
		}

		public static string GetAssetPath(string filename)
		{
			return GetModPath() + @"\Assets\" + filename;
		}

		private static string GetModInfoPath()
		{
			return GetModPath() + "\\mod.json";
		}

		private static void LoadConfig()
		{
			config = ModUtils.LoadConfig<Config>(GetModInfoPath());
			ValidateConfig();
		}

		private static void ValidateConfig()
		{
			Config defaultConfig = new Config();
			if (config == null)
			{
				config = defaultConfig;
				return;
			}
		}
	}
}