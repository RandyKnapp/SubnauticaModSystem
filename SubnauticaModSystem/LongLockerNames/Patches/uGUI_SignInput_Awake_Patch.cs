using Harmony;
using LongLockerNames.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LongLockerNames.Patches
{
	[HarmonyPatch(typeof(uGUI_SignInput))]
	[HarmonyPatch("Awake")]
	public static class uGUI_SignInput_Awake_Patch
	{
		private const float TextFieldHeight = 600;

		private static void Postfix(uGUI_SignInput __instance)
		{
			AddColors(__instance);
			if (IsOnSmallLocker(__instance))
			{
				PatchSmallLocker(__instance);
			}
			else
			{
				PatchSign(__instance);
			}
		}

		private static bool IsOnSmallLocker(uGUI_SignInput __instance)
		{
			return __instance.inputField.characterLimit == 10;
		}

		private static void PatchSmallLocker(uGUI_SignInput __instance)
		{
			__instance.inputField.lineType = InputField.LineType.MultiLineNewline;
			__instance.inputField.characterLimit = Mod.config.SmallLockerTextLimit;

			var rt = __instance.inputField.transform as RectTransform;
			RectTransformExtensions.SetSize(rt, rt.rect.width, TextFieldHeight);

			GameObject.Destroy(__instance.inputField.textComponent.GetComponent<ContentSizeFitter>());
			rt = __instance.inputField.textComponent.transform as RectTransform;
			RectTransformExtensions.SetSize(rt, rt.rect.width, TextFieldHeight);

			__instance.inputField.textComponent.alignment = TextAnchor.MiddleCenter;

			AddColorBackButton(__instance);

			Mod.PrintObject(__instance.gameObject);
		}

		private static void AddColorBackButton(uGUI_SignInput __instance)
		{
			var currentButton = __instance.transform.GetChild(1).GetComponent<Button>();
			if (currentButton != null)
			{
				var crt = (currentButton.transform as RectTransform);
				var w = crt.rect.width;
				var h = crt.rect.height;

				var newButton = GameObject.Instantiate(currentButton);
				newButton.name = "ColorSelectorBack";
				newButton.transform.SetParent(__instance.transform, false);

				var rt = newButton.transform as RectTransform;
				RectTransformExtensions.SetSize(rt, w, h);
				rt.anchoredPosition += new Vector2(0, 96);

				var go = newButton.gameObject;
				GameObject.DestroyImmediate(newButton);
				newButton = go.AddComponent<Button>();
				newButton.transition = currentButton.transition;
				newButton.targetGraphic = go.GetComponentInChildren<Image>();
				newButton.colors = currentButton.colors;

				newButton.onClick.RemoveAllListeners();
				var instance = __instance;
				newButton.onClick.AddListener(() => {
					int old = instance.colorIndex;
					int c = instance.colorIndex - 1;
					int newColor = ((c < 0) ? instance.colors.Length + c : c);
					__instance.colorIndex = newColor;
				});

				currentButton.GetComponentInChildren<Image>().sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("color_down.png"));
				newButton.GetComponentInChildren<Image>().sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("color_up.png"));

				__instance.editOnly = __instance.editOnly.Add(newButton.gameObject).ToArray();
				__instance.colorizedElements = __instance.colorizedElements.Add(newButton.GetComponentInChildren<Image>()).ToArray();
			}
		}

		private static void SetColorPrevious(uGUI_SignInput __instance)
		{
			int num = __instance.colorIndex - 1;
			__instance.colorIndex = ((num < 0) ? __instance.colors.Length - 1 : num);
		}

		private static void PatchSign(uGUI_SignInput __instance)
		{
			__instance.inputField.lineType = InputField.LineType.MultiLineNewline;
			__instance.inputField.characterLimit = Mod.config.SignTextLimit;
		}

		private static void AddColors(uGUI_SignInput __instance)
		{
			__instance.colors = __instance.colors.Concat(new Color[] {
				new Color(255 / 255f, 182 / 255f, 193 / 255f),
				new Color(255 / 255f, 127 / 255f, 80  / 255f),
				new Color(255 / 255f, 218 / 255f, 185 / 255f),
				new Color(230 / 255f, 230 / 255f, 250 / 255f),
				new Color(255 / 255f, 0   / 255f, 255 / 255f),
				new Color(123 / 255f, 104 / 255f, 238 / 255f),
				new Color(173 / 255f, 255 / 255f, 47  / 255f),
				new Color(154 / 255f, 205 / 255f, 50  / 255f),
				new Color(128 / 255f, 128 / 255f, 0   / 255f),
				new Color(32  / 255f, 178 / 255f, 170 / 255f),
				new Color(135 / 255f, 206 / 255f, 235 / 255f),
				new Color(30  / 255f, 144 / 255f, 255 / 255f),
				new Color(255 / 255f, 228 / 255f, 196 / 255f),
				new Color(210 / 255f, 105 / 255f, 30  / 255f),
				new Color(192 / 255f, 192 / 255f, 192 / 255f),
				new Color(112 / 255f, 128 / 255f, 144 / 255f)
			}).ToArray();

			if (Mod.config.AdditionalColors != null)
			{
				__instance.colors = __instance.colors.Concat(Mod.config.AdditionalColors).ToArray();
			}
		}
	}
}
