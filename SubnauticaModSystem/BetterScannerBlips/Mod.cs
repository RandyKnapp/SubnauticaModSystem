﻿using Common.Mod;
using Common.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System.Threading;
using System.Globalization;

namespace BetterScannerBlips
{
	static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			var harmony = new Harmony("com.BetterScannerBlips.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			CustomBlip.InitializeColors();

			Logger.Log("Patched");
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
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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