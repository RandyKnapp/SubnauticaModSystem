using Harmony;
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
			if (IsOnSmallLocker(__instance))
			{
				PatchSmallLocker(__instance);
			}
			else
			{
				PatchSign(__instance);
			}
			AddColors(__instance);
		}

		private static bool IsOnSmallLocker(uGUI_SignInput __instance)
		{
			return __instance.inputField.characterLimit == 10;
		}

		private static void PatchSmallLocker(uGUI_SignInput __instance)
		{
			__instance.inputField.lineType = InputField.LineType.MultiLineNewline;
			__instance.inputField.characterLimit = 60;

			var rt = __instance.inputField.transform as RectTransform;
			RectTransformExtensions.SetSize(rt, rt.rect.width, TextFieldHeight);

			GameObject.Destroy(__instance.inputField.textComponent.GetComponent<ContentSizeFitter>());
			rt = __instance.inputField.textComponent.transform as RectTransform;
			RectTransformExtensions.SetSize(rt, rt.rect.width, TextFieldHeight);
			Logger.Log("TextComponent Rect: " + rt.rect);

			__instance.inputField.textComponent.alignment = TextAnchor.MiddleCenter;
		}

		private static void PatchSign(uGUI_SignInput __instance)
		{
			__instance.inputField.characterLimit = 100;
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
		}
	}
}
