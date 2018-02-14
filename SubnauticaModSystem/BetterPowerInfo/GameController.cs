using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	public class GameController : MonoBehaviour
	{
		private static readonly string GAME_OBJECT_NAME = "BetterPowerInfo.Controller";

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
			
			/*if (Input.GetKeyDown(KeyCode.I))...*/
		}
		
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "Main")
			{
				gameObject.SetActive(true);
			}
		}
	}
}
