using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	public class GameController : MonoBehaviour
	{
		private PowerProductionDisplay productionDisplay;
		//private PowerConsumerDisplay consumerDisplay;

		private void Awake()
		{
			DevConsole.RegisterConsoleCommand(this, "deplete", false, false);
			Logger.Log("GameController Added");
		}

		private void OnDestroy()
		{
			Logger.Log("GameController Destroyed");
		}

		private void Update()
		{
			if (!uGUI_SceneLoading.IsLoadingScreenFinished || uGUI.main == null || uGUI.main.loading.IsLoading)
			{
				return;
			}

			if (productionDisplay == null)
			{
				Logger.Log("Creating Text Objects...");
				Transform hud = GameObject.FindObjectOfType<uGUI_PowerIndicator>().transform;
				productionDisplay = CreatePowerDisplay("PowerProduction", hud, -515).AddComponent<PowerProductionDisplay>();
				//consumerDisplay = CreatePowerDisplay("PowerConsumers", hud, 515).AddComponent<PowerConsumerDisplay>();
			}

			if (productionDisplay != null /*&& consumerDisplay != null*/)
			{
				bool keyDown = Input.GetKeyDown(Mod.config.ViewToggleKey);
				if (keyDown)
				{
					productionDisplay.SetMode((DisplayMode)(((int)productionDisplay.Mode + 1) % 3));
					//consumerDisplay.SetMode(productionDisplay.Mode);
				}
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
			ErrorMessage.AddDebug("Depleting batteries and powercells");
		}

		private static GameObject CreatePowerDisplay(string name, Transform parent, int x)
		{
			var go = new GameObject(name, typeof(RectTransform));
			var rt = go.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), parent);
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 700);
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 800);
			rt.anchoredPosition = new Vector3(x, -425);

			return go;
		}

		public static GameObject CreateNewText(string name, Transform parent, TextAnchor anchor)
		{
			Text prefab = GameObject.FindObjectOfType<HandReticle>().interactPrimaryText;
			if (prefab == null)
			{
				Logger.Log("Could not find text prefab! (HandReticle.interactPrimaryText)");
				return null;
			}

			Text text = GameObject.Instantiate(prefab);
			text.gameObject.layer = parent.gameObject.layer;
			text.gameObject.name = name;
			text.transform.SetParent(parent, false);
			text.transform.localScale = new Vector3(1, 1, 1);
			text.gameObject.SetActive(true);
			text.enabled = true;
			text.text = "";
			text.fontSize = 20;
			RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 700);
			text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30);
			text.rectTransform.anchoredPosition = new Vector3(0, 0);
			text.alignment = anchor;
			text.raycastTarget = false;

			return text.gameObject;
		}
	}
}
