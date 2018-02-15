using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	public abstract class PowerDisplayBase : MonoBehaviour
	{
		protected const float TextUpdateInterval = 0.5f;
		protected Text text;

		private void Awake()
		{
			text = GetComponent<Text>();
			text.supportRichText = true;
		}

		private IEnumerator Start()
		{
			for (; ; )
			{
				yield return new WaitForSeconds(TextUpdateInterval);
				if (ShouldSkipUpdate())
				{
					continue;
				}

				UpdatePower();
			}
		}

		private void Update()
		{
			var pda = Player.main.GetPDA();
			if (pda != null && pda.isInUse && text.enabled)
			{
				text.enabled = false;
			}
			else if ((pda == null || !pda.isInUse) && !text.enabled)
			{
				text.enabled = true;
			}
		}

		protected abstract void UpdatePower();

		protected bool ShouldSkipUpdate()
		{
			return MainMenuOrStillLoading() || IsWrongGameState() || IsInCameraMode() || Player.main == null;
		}

		protected bool MainMenuOrStillLoading()
		{
			return !uGUI_SceneLoading.IsLoadingScreenFinished || uGUI.main == null || uGUI.main.loading.IsLoading;
		}

		protected bool IsWrongGameState()
		{
			return uGUI.isIntro || LaunchRocket.isLaunching || !GameModeUtils.RequiresPower();
		}

		protected bool IsInCameraMode()
		{
			uGUI_CameraDrone cameraDrone = uGUI_CameraDrone.main;
			return cameraDrone != null && cameraDrone.GetCamera() != null;
		}
	}
}
