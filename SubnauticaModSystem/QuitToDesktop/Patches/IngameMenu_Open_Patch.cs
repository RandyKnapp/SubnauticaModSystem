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
			if (__instance != null && quitButton == null)
			{
				var prefab = __instance.quitToMainMenuButton.transform.parent.GetChild(0).gameObject.GetComponent<Button>();
				quitButton = GameObject.Instantiate(prefab, __instance.quitToMainMenuButton.transform.parent);
				quitButton.name = "ButtonQuitToDesktop";
				quitButton.GetComponentInChildren<Text>().text = "Quit to Desktop";
				quitButton.GetComponent<EventTrigger>().enabled = false;
				quitButton.GetComponent<EventTrigger>().triggers.Clear();
				quitButton.onClick.AddListener(() => { __instance.QuitGame(true); });
			}
		}
	}
}
