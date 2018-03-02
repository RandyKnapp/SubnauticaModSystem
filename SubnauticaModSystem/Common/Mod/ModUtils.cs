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
		private static FieldInfo CraftData_techMapping = typeof(CraftData).GetField("techMapping", BindingFlags.NonPublic | BindingFlags.Static);

		private static List<TechType> pickupableTypes;

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

		public static void ValidateConfigValue<T, ConfigT>(string field, T min, T max, ref ConfigT config, ref ConfigT defaultConfig) where T : IComparable
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
			if (obj == null)
			{
				Console.WriteLine(indent + "null");
				return;
			}
			Console.WriteLine(indent + "[[" + obj.name + "]]:");
			Console.WriteLine(indent + "{");
			Console.WriteLine(indent + "  Components:");
			Console.WriteLine(indent + "  {");
			var lastC = obj.GetComponents<Component>().Last();
			foreach (var c in obj.GetComponents<Component>())
			{
				Console.WriteLine(indent + "    (" + c.GetType().ToString() + ")");
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

		public static void PrintObjectFields(object obj, string indent = "")
		{
			if (obj == null)
			{
				Console.WriteLine(indent + "  null");
				return;
			}

			Type type = obj.GetType();
			FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (FieldInfo field in fields)
			{
				Console.WriteLine(indent + "  " + field.Name + " : " + field.GetValue(obj));
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
			foreach (PropertyInfo property in properties)
			{
				Console.WriteLine(indent + "  " + property.Name + " : " + property.GetValue(obj, new object[] { }));
			}
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

		public static GameObject GetChildByName(GameObject parent, string name, bool recursive = false)
		{
			GameObject found = null;
			foreach (Transform child in parent.transform)
			{
				if (child.gameObject.name == name)
				{
					found = child.gameObject;
					break;
				}

				if (found == null && recursive)
				{
					found = GetChildByName(child.gameObject, name, recursive);
					if (found != null)
					{
						break;
					}
				}
			}
			return found;
		}

		public static List<TechType> GetPickupableTechTypes()
		{
			if (pickupableTypes != null)
			{
				return pickupableTypes;
			}

			Console.WriteLine("[ModUtils] Initialize Pickupable Types");
			pickupableTypes = new List<TechType>();

			var techMapping = (Dictionary<TechType, string>)CraftData_techMapping.GetValue(null);
			foreach (var entry in techMapping)
			{
				var techType = entry.Key;
				var prefab = CraftData.GetPrefabForTechType(techType);
				if (prefab != null)
				{
					if (prefab.GetComponent<Pickupable>() != null)
					{
						Console.WriteLine("" + techType);
						pickupableTypes.Add(techType);
					}
				}
			}
			
			return pickupableTypes;
		}

		public static T CopyComponent<T>(T original, GameObject destination) where T : Component
		{
			Type type = original.GetType();
			Component copy = destination.AddComponent(type);
			FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (FieldInfo field in fields)
			{
				field.SetValue(copy, field.GetValue(original));
			}
			return copy as T;
		}
	}
}
