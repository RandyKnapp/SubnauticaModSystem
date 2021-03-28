using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterScannerBlips
{
	public class CustomBlip : MonoBehaviour
	{
		private static Color circleColor;
		private static Color textColor;

		private Image image;
#if SUBNAUTICA
		private Text text;
#elif BELOWZERO
		private TextMeshProUGUI text;
#endif
		private TechType techType;
		private string resourceName;

		private BlipIdentifier blipId;

		public void ResetCustomBlip()
		{
			this.techType = TechType.None;
			this.resourceName = "";
		}

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
#if SUBNAUTICA
			text = gameObject.GetComponentInChildren<Text>();
#elif BELOWZERO
			text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
#endif
		}

		public void Refresh(ResourceTrackerDatabase.ResourceInfo target)
		{
			if (target != null)
			{
				var vectorToPlayer = Player.main.transform.position - target.position;
				var distance = vectorToPlayer.magnitude;

				if (techType == TechType.None)
				{
					TechType thisTechType = target.techType;
					QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
						$"CustomBlip.Refresh(): Attempting to establish resourceName for ResourceInfo with unique ID {target.uniqueId}, which includes TechType {thisTechType}");
					if (thisTechType == TechType.Fragment)
					{
						blipId = gameObject.GetComponent<BlipIdentifier>();
						if (blipId == null)
						{
							blipId = gameObject.AddComponent<BlipIdentifier>();
							blipId.uniqueId = target.uniqueId;
						}
						if (blipId != null)
						{
							thisTechType = blipId.actualTechType;
							if (thisTechType == TechType.None)
							{
								QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
									$"CustomBlip.Refresh(): Could not get TechType from BlipIdentifier, attempting to use ResourceTrackerPatches.GetTechTypeForId");
								thisTechType = ResourceTrackerPatches.GetTechTypeForId(target.uniqueId);
								QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
									$"CustomBlip.Refresh(): Got TechType of {techType.AsString()}");
							}
						}
					}

					if (thisTechType != TechType.None && thisTechType != TechType.Fragment)
					{
						techType = thisTechType;
						resourceName = Language.main.Get(techType);
						QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
							$"For unique ID {target.uniqueId}, got TechType {thisTechType.AsString()} and resourceName '{resourceName}'");
					}
					else
						resourceName = "(*)" + Language.main.Get(target.techType);
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
