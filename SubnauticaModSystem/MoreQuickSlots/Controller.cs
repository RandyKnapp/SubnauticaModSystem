using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreQuickSlots
{
	public class Controller : MonoBehaviour
	{
		private static readonly string GAME_OBJECT_NAME = "MoreQuickSlots.Controller";

		public static void Load()
		{
			Unload();
			new GameObject(GAME_OBJECT_NAME).AddComponent<global::MoreQuickSlots.Controller>();
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
				KeyCode key = GetKeyCodeForSlot(i);
				if (Input.GetKeyDown(key))
				{
					SelectQuickSlot(i);
				}
			}
		}

		private KeyCode GetKeyCodeForSlot(int slotID)
		{
			if (slotID == 9) return KeyCode.Alpha0;
			if (slotID == 10) return KeyCode.Minus;
			if (slotID == 11) return KeyCode.Equals;
			else return KeyCode.Alpha1 + slotID;
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
