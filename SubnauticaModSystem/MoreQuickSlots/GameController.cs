using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreQuickSlots
{
	public class GameController : MonoBehaviour
	{
		private static readonly string GAME_OBJECT_NAME = "MoreQuickSlots.Controller";

		public static void Load()
		{
			Unload();
			new GameObject(GAME_OBJECT_NAME).AddComponent<global::MoreQuickSlots.GameController>();
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

			if (Inventory.main == null)
			{
				return;
			}

			for (int i = Player.quickSlotButtonsCount; i < Mod.config.SlotCount; ++i)
			{
				KeyCode key = Mod.GetKeyCodeForSlot(i);
				if (Input.GetKeyDown(key))
				{
					SelectQuickSlot(i);
				}
			}

			/*if (Input.GetKeyDown(KeyCode.I)) MoveAllLabels(new Vector2(0, 1));
			if (Input.GetKeyDown(KeyCode.K)) MoveAllLabels(new Vector2(0, -1));
			if (Input.GetKeyDown(KeyCode.J)) MoveAllLabels(new Vector2(-1, 0));
			if (Input.GetKeyDown(KeyCode.L)) MoveAllLabels(new Vector2(1, 0));
			if (Input.GetKeyDown(KeyCode.U)) SetTextSize(-1);
			if (Input.GetKeyDown(KeyCode.O)) SetTextSize(1);*/
		}

		/*private void MoveAllLabels(Vector2 offset)
		{
			GameObject[] labels = GetAllLabels();
			foreach(var label in labels)
			{
				RectTransform rt = label.transform as RectTransform;
				rt.anchoredPosition = rt.anchoredPosition + offset;
			}
			Logger.Log("Label Position = " + (labels[0].transform as RectTransform).anchoredPosition);
		}*/

		/*private void SetTextSize(int offset)
		{
			GameObject[] labels = GetAllLabels();
			foreach (var label in labels)
			{
				Text t = label.GetComponent<Text>();
				t.fontSize = t.fontSize + offset;
			}
			Logger.Log("Label Font Size = " + labels[0].GetComponent<Text>().fontSize);
		}*/

		private GameObject[] GetAllLabels()
		{
			GameObject[] labels = new GameObject[Mod.config.SlotCount];
			for (int i = 0; i < Mod.config.SlotCount; ++i)
			{
				GameObject label = GameObject.Find("QuickSlotText" + i.ToString());
				if (label != null)
				{
					labels[i] = label;
				}
			}
			return labels;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "Main")
			{
				gameObject.SetActive(true);
			}
		}

		private void SelectQuickSlot(int slotID)
		{
			Inventory.main.quickSlots.Select(slotID);
		}
	}
}
