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

	abstract class PowerDisplayBase : MonoBehaviour
	{
		protected const float Spacing = 4;
		protected const float TextUpdateInterval = 0.1f;

		public DisplayMode Mode { get; private set; }

		protected Dictionary<TechType, PowerDisplayEntry> entries = new Dictionary<TechType, PowerDisplayEntry>();

		protected VerticalLayoutGroup layout;
		protected Text text;
		protected bool pdaActive = true;

		private void Awake()
		{
			Mode = DisplayMode.Minimal;

			layout = gameObject.AddComponent<VerticalLayoutGroup>();
			layout.spacing = Spacing;
			layout.childAlignment = TextAnchor.UpperRight;
			layout.childControlWidth = false;
			layout.childControlHeight = false;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = false;

			text = Mod.InstantiateNewText("MainText", transform);
			text.alignment = TextAnchor.MiddleRight;
			text.supportRichText = true;

			var contentFitter = text.gameObject.AddComponent<ContentSizeFitter>();
			contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			text.text = "MainText";
		}

		private IEnumerator Start()
		{
			for (;;)
			{
				yield return new WaitForSeconds(TextUpdateInterval);
				if (ShouldSkipUpdate())
				{
					continue;
				}

				yield return UpdatePower();
			}
		}

		private void Update()
		{
			UpdatePdaVisibility();
			RefreshVisibility();
		}

		protected virtual bool ShouldShow()
		{
			return !pdaActive;
		}

		private void UpdatePdaVisibility()
		{
			var pda = Player.main.GetPDA();
			if (pda != null && pda.isInUse && !pdaActive)
			{
				pdaActive = true;
			}
			else if ((pda == null || !pda.isInUse) && pdaActive)
			{
				pdaActive = false;
			}
		}

		private void RefreshVisibility()
		{
			text.gameObject.SetActive(ShouldShow());
			foreach (var entry in entries)
			{
				entry.Value.gameObject.SetActive(ShouldShow() && ShouldShowEntry(entry.Value));
			}
		}

		private bool ShouldShowEntry(PowerDisplayEntry entry)
		{
			return Mode == DisplayMode.Verbose && entry.ShouldShow();
		}

		protected virtual IEnumerator UpdatePower()
		{
			foreach (var entry in entries)
			{
				entry.Value.Refresh();
				yield return null;
			}
		}

		public virtual void SetMode(DisplayMode mode)
		{
			if (Mode != mode)
			{
				Mode = mode;
				Update();
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
