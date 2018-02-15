using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreQuickSlots
{
	public class GameController : MonoBehaviour
	{
		private void Awake()
		{
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

		private void SelectQuickSlot(int slotID)
		{
			Inventory.main.quickSlots.Select(slotID);
		}
	}
}
