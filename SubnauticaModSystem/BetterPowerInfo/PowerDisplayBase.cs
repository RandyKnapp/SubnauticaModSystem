using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	public enum DisplayMode
	{
		Minimal,
		Verbose,
		Off
	}

	public abstract class PowerDisplayBase : MonoBehaviour
	{
		protected const float TextUpdateInterval = 0.5f;

		public DisplayMode Mode { get; private set; }

		public Text text;

		private void Awake()
		{
			Mode = DisplayMode.Minimal;
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

		public virtual void SetMode(DisplayMode mode)
		{
			if (Mode != mode)
			{
				Mode = mode;
				UpdatePower();
			}
		}

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

		protected PowerRelay GetCurrentPowerRelay()
		{
			Player player = Player.main;
			if (player.escapePod.value && player.currentEscapePod != null)
			{
				return player.currentEscapePod.GetComponent<PowerRelay>();
			}
			else if (player.currentSub != null && player.currentSub.isBase)
			{
				return player.currentSub.GetComponent<PowerRelay>();
			}
			// TODO: All the cyclops power consumers and producers
			//else if (player.currentSub != null && player.currentSub.powerRelay != null)
			//{
			//	return player.currentSub.powerRelay;
			//}
			return null;
		}
	}
}
