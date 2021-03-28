using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Common.Mod
{
	internal static class ModUtils
	{
		//private static FieldInfo CraftData_techMapping = typeof(CraftData).GetField("techMapping", BindingFlags.NonPublic | BindingFlags.Static);

		//private static List<TechType> pickupableTypes;
		private static MonoBehaviour coroutineObject;

		public static ConfigT LoadConfig<ConfigT>(string configFilePath) where ConfigT : class, new()
		{
			if (!File.Exists(configFilePath))
			{
				return WriteDefaultConfig<ConfigT>(configFilePath);
			}

			try
			{
				string serialilzedConfig = File.ReadAllText(configFilePath);

				if (string.IsNullOrEmpty(serialilzedConfig))
				{
					return new ConfigT();
				}

				ConfigT config = JsonConvert.DeserializeObject<ConfigT>(serialilzedConfig);

				if (config == null)
				{
					config = new ConfigT();
				}

				return config;
			}
			catch
			{
				return WriteDefaultConfig<ConfigT>(configFilePath);
			}
		}

		private static ConfigT WriteDefaultConfig<ConfigT>(string configFilePath)
			 where ConfigT : class, new()
		{
			var defaultConfig = new ConfigT();
			string serialilzedConfig = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
			File.WriteAllText(configFilePath, serialilzedConfig);
			return defaultConfig;
		}

		public static void ValidateConfigValue<T, ConfigT>(string field, T min, T max, ref ConfigT config, ref ConfigT defaultConfig) where T : IComparable
		{
			PropertyInfo fieldInfo = typeof(ConfigT).GetProperty(field, BindingFlags.Public | BindingFlags.Instance);
			var value = (T)fieldInfo.GetValue(config, null);
			if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
			{
				string errorString = $"Config value for '{field}' ({value}) was not valid. Must be between {min} and {max}";
				//Console.WriteLine(string);
				QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Error, errorString, null, true);
				var newValue = value;
				if (value.CompareTo(min) < 0)
					newValue = min;
				else if (value.CompareTo(max) > 0)
					newValue = max;
				//fieldInfo.SetValue(config, fieldInfo.GetValue(defaultConfig, null), null);
				fieldInfo.SetValue(config, newValue, null);
			}
		}

		public static void LoadSaveData<SaveDataT>(string fileName, Action<SaveDataT> onSuccess) where SaveDataT : new()
		{
			StartCoroutine(LoadInternal<SaveDataT>(fileName, onSuccess));
		}

		private static IEnumerator LoadInternal<SaveDataT>(string fileName, Action<SaveDataT> onSuccess)
		{
			UserStorage userStorage = PlatformUtils.main.GetUserStorage();
			var files = new List<string> { fileName };
			UserStorageUtils.LoadOperation loadOperation = userStorage.LoadFilesAsync(SaveLoadManager.main.GetCurrentSlot(), files);
			yield return loadOperation;
			if (loadOperation.GetSuccessful())
			{
				string stringData = Encoding.ASCII.GetString(loadOperation.files[fileName]);
				SaveDataT saveData = JsonConvert.DeserializeObject<SaveDataT>(stringData);
				onSuccess(saveData);
			}
			else
			{
				Console.WriteLine("Load Failed: " + loadOperation.errorMessage);
			}
		}

		public static void Save<SaveDataT>(SaveDataT newSaveData, string fileName, Action onSaveComplete = null)
		{
			if (newSaveData != null)
			{
				string saveDataJson = JsonConvert.SerializeObject(newSaveData);
				StartCoroutine(SaveInternal(saveDataJson, fileName, onSaveComplete));
			}
		}

		private static IEnumerator SaveInternal(string saveData, string fileName, Action onSaveComplete = null)
		{
			UserStorage userStorage = PlatformUtils.main.GetUserStorage();
			var saveFileMap = new Dictionary<string, byte[]>
			{
				{ fileName, Encoding.ASCII.GetBytes(saveData) }
			};
			SaveLoadManager.main.GetCurrentSlot();
			UserStorageUtils.SaveOperation saveOp = userStorage.SaveFilesAsync(SaveLoadManager.main.GetCurrentSlot(), saveFileMap);
			yield return saveOp;
			if (saveOp.GetSuccessful())
			{
				onSaveComplete?.Invoke();
			}
		}

		public static void PrintObject(GameObject obj, string indent = "", bool includeMaterials = false)
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
			Component lastC = obj.GetComponents<Component>().Last();
			foreach (Component c in obj.GetComponents<Component>())
			{
				Console.WriteLine(indent + "    (" + c.GetType().ToString() + ")");
				if (includeMaterials)
				{
					if (c.GetType().IsAssignableFrom(typeof(SkinnedMeshRenderer)) || c.GetType().IsAssignableFrom(typeof(MeshRenderer)))
					{
						var renderer = c as Renderer;
						Console.WriteLine(indent + "    {");
						foreach (Material material in renderer.materials)
						{
							Console.WriteLine(indent + $"      {material}");
						}
						Console.WriteLine(indent + "    }");
						Console.WriteLine(indent + "    {");
						foreach (Material material in renderer.sharedMaterials)
						{
							Console.WriteLine(indent + $"      {material}");
						}
						Console.WriteLine(indent + "    }");
					}
				}
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

		private static List<string> s_gameObjectFields;

		public static void PrintObjectFields(object obj, string indent = "")
		{
			if (s_gameObjectFields == null)
			{
				s_gameObjectFields = new List<string>();
				FieldInfo[] goFields = typeof(GameObject).GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (FieldInfo field in goFields)
				{
					s_gameObjectFields.Add(field.Name);
				}
				PropertyInfo[] goProps = typeof(GameObject).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
				foreach (PropertyInfo prop in goProps)
				{
					s_gameObjectFields.Add(prop.Name);
				}
			}

			if (obj == null)
			{
				Console.WriteLine(indent + "  null");
				return;
			}

			Type type = obj.GetType();
			FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (FieldInfo field in fields)
			{
				if (s_gameObjectFields.Contains(field.Name))
				{
					continue;
				}
				Console.WriteLine(indent + "  " + field.Name + " : " + field.GetValue(obj));
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
			foreach (PropertyInfo property in properties)
			{
				if (s_gameObjectFields.Contains(property.Name))
				{
					continue;
				}
				Console.WriteLine(indent + "  " + property.Name + " : " + property.GetValue(obj, new object[] { }));
			}
		}

		public static Text GetTextPrefab()
		{
			Text prefab = null;
#if SUBNAUTICA
			prefab = GameObject.FindObjectOfType<HandReticle>().interactPrimaryText;
#elif BELOWZERO
			//prefab = GameObject.FindObjectOfType<HandReticle>().
#endif
			if (prefab == null)
			{
				return null;
			}

			return prefab;
		}

		public static Text InstantiateNewText(string name, Transform parent)
		{
			Text text = GameObject.Instantiate(GetTextPrefab());
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

		/*
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
		*/

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

		private static Coroutine StartCoroutine(IEnumerator coroutine)
		{
			if (coroutineObject == null)
			{
				var go = new GameObject();
				coroutineObject = go.AddComponent<ModSaver>();
			}

			return coroutineObject.StartCoroutine(coroutine);
		}
	}
}
