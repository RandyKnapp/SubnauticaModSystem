using Common.Utility;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	public static class Mod
	{
		private static readonly Color ChargeColorEmpty = Color.red;
		private static readonly Color ChargeColorHalf = Color.yellow;
		private static readonly Color ChargeColorFull = new Color(0, 1, 0);

		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data/Managed";
			LoadConfig();

			HarmonyInstance harmony = HarmonyInstance.Create("com.betterpowerinfo.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		private static string GetModInfoPath()
		{
			return Environment.CurrentDirectory + "/" + modDirectory + "/mod.json";
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
		}

		public static string FormatName(string name)
		{
			string s = name.Replace("(Clone)", "");
			s = System.Text.RegularExpressions.Regex.Replace(s, "[A-Z]", " $0").Trim();
			return s;
		}

		public static Color GetChargeColor(float percentCharged)
		{
			return (percentCharged >= 0.5f) ? Color.Lerp(ChargeColorHalf, ChargeColorFull, 2f * percentCharged - 1f) : Color.Lerp(ChargeColorEmpty, ChargeColorHalf, 2f * percentCharged);
		}

		public static string GetChargeColorString(float percentCharged)
		{
			return "#" + ColorUtility.ToHtmlStringRGBA(GetChargeColor(percentCharged));
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
	}
}