using Common.Mod;
using Common.Utility;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Oculus.Newtonsoft.Json;

namespace CustomBeacons
{
	[Serializable]
	public class ColorInfo
	{
		public List<SerializableColor> Colors = new List<SerializableColor>();
	}

	static class Mod
	{
		private const int StartingPingIndex = 100;

		public static Config config;
		public static ColorInfo colorInfo;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.CustomBeacons.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			foreach (var color in colorInfo.Colors)
			{
				CustomPings.AddPingColor(color.ToColor());
			}

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
			return Environment.CurrentDirectory + "/" + modDirectory;
		}

		public static string GetAssetPath(string filename)
		{
			return GetModPath() + "/Assets/" + filename;
		}

		private static string GetModInfoPath()
		{
			return GetModPath() + "/mod.json";
		}

		private static void LoadConfig()
		{
			config = ModUtils.LoadConfig<Config>(GetModInfoPath());
			ValidateConfig();

			LoadCustomColors();
		}

		private static void LoadCustomColors()
		{
			var colorInfoPath = GetAssetPath("colors.json");
			if (File.Exists(colorInfoPath))
			{
				try
				{
					colorInfo = JsonConvert.DeserializeObject<ColorInfo>(File.ReadAllText(colorInfoPath));
				}
				catch (Exception)
				{
					colorInfo = new ColorInfo();
					Logger.Error("Could not load colors.json! Check to make sure its JSON is valid.");
				}
			}
			else
			{
				colorInfo = new ColorInfo();
				File.WriteAllText(colorInfoPath, JsonConvert.SerializeObject(colorInfo, Formatting.Indented));
			}
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