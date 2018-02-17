using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuitToDesktop.Patches
{
	[HarmonyPatch(typeof(IngameMenu))]
	[HarmonyPatch("Open")]
	public static class IngameMenu_Open_Patch
	{
		static Button quitButton;

		private static void Postfix(IngameMenu __instance)
		{
			if (GameModeUtils.IsPermadeath())
			{
				return;
			}

			if (__instance != null && quitButton == null)
			{
				var prefab = __instance.quitToMainMenuButton.transform.parent.GetChild(0).gameObject.GetComponent<Button>();
				quitButton = GameObject.Instantiate(prefab, __instance.quitToMainMenuButton.transform.parent);
				quitButton.name = "ButtonQuitToDesktop";
				quitButton.onClick.RemoveAllListeners();
				quitButton.onClick.AddListener(() => { __instance.QuitGame(true); });

				var texts = quitButton.GetComponents<Text>().Concat(quitButton.GetComponentsInChildren<Text>());
				foreach (Text text in texts)
				{
					text.text = "Quit to Desktop";
				}

				texts = __instance.quitToMainMenuButton.GetComponents<Text>().Concat(__instance.quitToMainMenuButton.GetComponentsInChildren<Text>());
				foreach (Text text in texts)
				{
					text.text = "Quit to Main Menu";
				}
			}
		}
	}
}
