using Common.Utility;
using Harmony;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTracker
{
	static class Mod
	{
		public const string SaveDataFilename = "BlueprintTrackerSave.json";
		public const int MaxPins = 13;
		public const int MinPins = 1;

		public static Config config;
		public static bool Left = false;
		public static bool Top = true;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.blueprinttracker.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
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

		public static int GetMaxPins()
		{
			return config.MaxPinnedBlueprints;
		}

		private static void LoadConfig()
		{
			string modInfoPath = GetModInfoPath();

			if (!File.Exists(modInfoPath))
			{
				config = new Config();
				return;
			}

			var modInfoObject = JSON.Parse(File.ReadAllText(modInfoPath));
			string configJson = modInfoObject["Config"].ToString();
			config = JsonUtility.FromJson<Config>(configJson);
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

			ValidateConfigValue("MaxPinnedBlueprints", MinPins, MaxPins, defaultConfig);

			switch (config.Position)
			{
				case "TopLeft":		Left = true;	Top = true;		break;
				case "TopRight":	Left = false;	Top = true;		break;
				case "BottomLeft":	Left = true;	Top = false;	break;
				case "BottomRight":	Left = false;	Top = false;	break;

				default:
					Logger.Log("Config value for '{0}' ({1}) as not valid. Must be one of: TopLeft, TopRight, BottomLeft, BottomRight", "Position", config.Position);
					config.Position = defaultConfig.Position;
					break;
			}

			ValidateConfigValue("TrackerScale", 0.01f, 5.0f, defaultConfig);
			ValidateConfigValue("FontSize", 10, 60, defaultConfig);
			ValidateConfigValue("BackgroundAlpha", 0.0f, 1.0f, defaultConfig);
		}

		private static void ValidateConfigValue<T>(string field, T min, T max, Config defaultConfig) where T : IComparable
		{
			var fieldInfo = typeof(Config).GetField(field, BindingFlags.Public | BindingFlags.Instance);
			T value = (T)fieldInfo.GetValue(config);
			if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
			{
				Logger.Log("Config value for '{0}' ({1}) was not valid. Must be between {2} and {3}",
					field,
					value,
					min,
					max
				);
				fieldInfo.SetValue(config, fieldInfo.GetValue(defaultConfig));
			}
		}

		public static void PrintObject(GameObject obj, string indent = "")
		{
			Console.WriteLine(indent + "[[" + obj.name + "]]:");
			Console.WriteLine(indent + "{");
			Console.WriteLine(indent + "  Components:");
			Console.WriteLine(indent + "  {");
			var lastC = obj.GetComponents<Component>().Last();
			foreach (var c in obj.GetComponents<Component>())
			{
				Console.WriteLine(indent + "    " + c.ToString().Replace(obj.name, "").Trim());
			}
			Console.WriteLine(indent + "  }");
			Console.WriteLine(indent + "  Children:");
			Console.WriteLine(indent + "  {");
			foreach (Transform child in obj.transform)
			{
				PrintObject(child.gameObject, indent + "    ");
			}
			Console.WriteLine(indent + "  }");
			Console.WriteLine(indent + "}");
		}

		public static Text GetTextPrefab()
		{
			Text prefab = GameObject.FindObjectOfType<HandReticle>().interactPrimaryText;
			if (prefab == null)
			{
				Logger.Log("Could not find text prefab! (HandReticle.interactPrimaryText)");
				return null;
			}

			return prefab;
		}

		public static Text InstantiateNewText(string name, Transform parent)
		{
			var text = GameObject.Instantiate(GetTextPrefab());
			text.gameObject.layer = parent.gameObject.layer;
			text.gameObject.name = name;
			text.transform.SetParent(parent, false);
			text.transform.localScale = new Vector3(1, 1, 1);
			text.gameObject.SetActive(true);
			text.enabled = true;

			RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
			text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
			return text;
		}

		public static string GetSaveDataDirectory()
		{
			return Path.Combine(Path.Combine(Path.GetFullPath("SNAppData"), "SavedGames"), Utils.GetSavegameDir());
		}

		public static SaveData LoadSaveData()
		{
			var saveDir = GetSaveDataDirectory();
			var saveFile = Path.Combine(saveDir, SaveDataFilename);
			if (File.Exists(saveFile))
			{
				SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveFile));
				if (saveData != null)
				{
					Logger.Log("Save Data Loaded");
					return saveData;
				}
			}

			Logger.Log("Save Data not found, save data instance created");
			return new SaveData();
		}

		public static void Save(SaveData newSaveData)
		{
			if (newSaveData != null)
			{
				var saveDir = GetSaveDataDirectory();
				var saveFile = Path.Combine(saveDir, SaveDataFilename);
				string saveDataJson = JsonUtility.ToJson(newSaveData);
				File.WriteAllText(saveFile, saveDataJson);
			}
		}
	}
}