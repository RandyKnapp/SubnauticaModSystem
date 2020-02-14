using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BetterScannerBlips
{
	class CustomBlip : MonoBehaviour
	{
		private static Color circleColor;
		private static Color textColor;

		private Image image;
		private Text text;
		private TechType techType;
		private string resourceName;

		public static void InitializeColors()
		{
			if (Mod.config.CustomColors)
			{
				if (!ColorUtility.TryParseHtmlString(Mod.config.CircleColor, out circleColor))
				{
					circleColor = new Color(1, 1, 1, 1);
				}
				if (!ColorUtility.TryParseHtmlString(Mod.config.TextColor, out textColor))
				{
					textColor = new Color(1, 1, 1, 1);
				}
			}
		}

		private void Awake()
		{
			image = gameObject.GetComponent<Image>();
			text = gameObject.GetComponentInChildren<Text>();
		}

		public void Refresh(ResourceTrackerDatabase.ResourceInfo target)
		{
			if (target != null)
			{
				var vectorToPlayer = Player.main.transform.position - target.position;
				var distance = vectorToPlayer.magnitude;

				if (resourceName == string.Empty || techType != target.techType)
				{
					techType = target.techType;
					resourceName = Language.main.Get(techType);
				}

				RefreshColor(distance);
				RefreshText(distance);
				RefreshScale(distance);
			}
		}

		private void RefreshText(float distance)
		{
			if (Mod.config.NoText)
			{
				text.gameObject.SetActive(false);
			}
			else
			{
				if (Mod.config.ShowDistance)
				{
					string meters = (distance < 5 ? Math.Round(distance, 1) : Mathf.RoundToInt(distance)).ToString();
					text.text = resourceName + " " + meters + "m";
				}
				text.gameObject.SetActive(distance < Mod.config.TextRange);
			}
		}

		private void RefreshScale(float distance)
		{
			if (distance < Mod.config.MinRange)
			{
				SetScale(Mod.config.MinRangeScale);
			}
			else if (distance >= Mod.config.MinRange && distance < Mod.config.CloseRange)
			{
				var t = Mathf.InverseLerp(Mod.config.MinRange, Mod.config.CloseRange, distance);
				SetScale(Mathf.Lerp(Mod.config.MinRangeScale, Mod.config.CloseRangeScale, t));
			}
			else if (distance >= Mod.config.CloseRange && distance < Mod.config.MaxRange)
			{
				var t = Mathf.InverseLerp(Mod.config.CloseRange, Mod.config.MaxRange, distance);
				SetScale(Mathf.Lerp(Mod.config.CloseRangeScale, Mod.config.MaxRangeScale, t));
			}
			else if (distance >= Mod.config.MaxRange)
			{
				SetScale(Mod.config.MaxRangeScale);
			}
		}

		private void SetScale(float scale)
		{
			transform.localScale = new Vector3(scale, scale, 1);
		}

		private void RefreshColor(float distance)
		{
			if (Mod.config.CustomColors)
			{
				image.color = circleColor;
				text.color = textColor;
			}

			if (distance < Mod.config.AlphaOutRange)
			{
				SetAlpha(Mod.config.MaxAlpha);
			}
			else
			{
				var t = Mathf.InverseLerp(Mod.config.AlphaOutRange, Mod.config.MaxRange, distance);
				SetAlpha(Mathf.Lerp(Mod.config.MaxAlpha, Mod.config.MinAlpha, t));
			}
		}

		private void SetAlpha(float alpha)
		{
			var iColor = image.color;
			iColor.a = alpha;
			image.color = iColor;

			var tColor = text.color;
			tColor.a = alpha;
			text.color = tColor;
		}
	}
}
