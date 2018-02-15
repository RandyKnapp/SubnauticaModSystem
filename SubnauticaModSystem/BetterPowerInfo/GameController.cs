using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	public class GameController : MonoBehaviour
	{
		private static readonly string GAME_OBJECT_NAME = "BetterPowerInfo.Controller";

		private PowerIndicatorDisplay display;

		public static void Load()
		{
			Unload();
			new GameObject(GAME_OBJECT_NAME).AddComponent<GameController>();
		}

		private static void Unload()
		{
			GameObject gameObject = GameObject.Find(GAME_OBJECT_NAME);
			if (gameObject)
			{
				DestroyImmediate(gameObject);
			}
		}

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;
			DevConsole.RegisterConsoleCommand(this, "deplete", false, false);
		}

		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void Update()
		{
			if (!uGUI_SceneLoading.IsLoadingScreenFinished || uGUI.main == null || uGUI.main.loading.IsLoading)
			{
				return;
			}

			if (display == null)
			{
				Logger.Log("Creating Text Object...");
				Transform hud = GameObject.FindObjectOfType<uGUI_PowerIndicator>().transform;
				display = CreateNewText(hud, "XXXXXXXXXX").AddComponent<PowerIndicatorDisplay>();
			}
		}
		
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "Main")
			{
				gameObject.SetActive(true);
			}
		}

		private void OnConsoleCommand_deplete()
		{
			if (Player.main != null && Inventory.main != null)
			{
				foreach (InventoryItem inventoryItem in Inventory.main.container)
				{
					if (inventoryItem.item.GetTechType() == TechType.Battery ||
						inventoryItem.item.GetTechType() == TechType.PowerCell)
					{
						var battery = inventoryItem.item.GetComponent<Battery>();
						if (battery)
						{
							battery.charge = 0;
						}
					}
				}
			}
			ErrorMessage.AddDebug("Depleting batteries");
		}

		private static GameObject CreateNewText(Transform parent, string newText)
		{
			Text prefab = GameObject.FindObjectOfType<HandReticle>().interactPrimaryText;
			if (prefab == null)
			{
				Logger.Log("Could not find text prefab! (HandReticle.interactPrimaryText)");
				return null;
			}

			Text text = GameObject.Instantiate(prefab);
			text.gameObject.layer = parent.gameObject.layer;
			text.gameObject.name = "PowerIndicatorDisplayText";
			text.transform.SetParent(parent, false);
			text.transform.localScale = new Vector3(1, 1, 1);
			text.gameObject.SetActive(true);
			text.enabled = true;
			text.text = newText;
			text.fontSize = 20;
			RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 800);
			text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 800);
			text.rectTransform.anchoredPosition = new Vector3(-500, 100);
			text.alignment = TextAnchor.UpperLeft;
			text.raycastTarget = false;

			return text.gameObject;
		}
	}
}
