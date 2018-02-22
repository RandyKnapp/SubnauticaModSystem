using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Mod
{
    public static class ModUtils
    {
		public static ConfigT LoadConfig<ConfigT>(string modInfoPath) where ConfigT : new()
		{
			if (!File.Exists(modInfoPath))
			{
				return new ConfigT();
			}

			var modInfoObject = JSON.Parse(File.ReadAllText(modInfoPath));
			string configJson = modInfoObject["Config"].ToString();
			var config = JsonUtility.FromJson<ConfigT>(configJson);
			if (config == null)
			{
				config = new ConfigT();
			}
			return config;
		}

		public static void ValidateConfigValue<T, ConfigT>(string field, T min, T max, ConfigT config, ConfigT defaultConfig) where T : IComparable
		{
			var fieldInfo = typeof(ConfigT).GetField(field, BindingFlags.Public | BindingFlags.Instance);
			T value = (T)fieldInfo.GetValue(config);
			if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
			{
				Console.WriteLine("Config value for '{0}' ({1}) was not valid. Must be between {2} and {3}",
					field,
					value,
					min,
					max
				);
				fieldInfo.SetValue(config, fieldInfo.GetValue(defaultConfig));
			}
		}

		public static SaveDataT LoadSaveData<SaveDataT>(string fileName) where SaveDataT : new()
		{
			var saveDir = GetSaveDataDirectory();
			var saveFile = Path.Combine(saveDir, fileName);
			if (File.Exists(saveFile))
			{
				SaveDataT saveData = JsonUtility.FromJson<SaveDataT>(File.ReadAllText(saveFile));
				if (saveData != null)
				{
					return saveData;
				}
			}

			return new SaveDataT();
		}

		public static void Save<SaveDataT>(SaveDataT newSaveData, string fileName)
		{
			if (newSaveData != null)
			{
				var saveDir = GetSaveDataDirectory();
				var saveFile = Path.Combine(saveDir, fileName);
				string saveDataJson = JsonUtility.ToJson(newSaveData);
				File.WriteAllText(saveFile, saveDataJson);
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
	}
}
