using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BlueprintTracker
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
		}
	}
}
