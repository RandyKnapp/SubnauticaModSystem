using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if BELOWZERO
using TMPro;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace BetterScannerBlips
{
	public class CustomBlip : MonoBehaviour
	{
		private static Color circleColor;
		private static Color textColor;

		private Image image;
#if SN1
		private Text text;
#elif BELOWZERO
		private TextMeshProUGUI text;
#endif
		private TechType techType;
		private string resourceName;
		private string lastID = ""; // Last unique identifier we were attached to. As long as the current ID matches this, we shouldn't need to retrieve the string.
		// Randy's original code didn't take this into account; and in fairness, he may not have needed to take it into account at all.
		// This code was originally based on Subnautica 1, and this behaviour may not have happened there.
		// However, in BZ, a given blip might be moved around - for example, a blip may initially be highlighting a Seaglide fragment, but later on - be it frames, seconds, whatever -
		// it might instead be highlighting a Seatruck fragment. Wouldn't be a problem under normal circumstances, but since we want proper text for these blips...
		// Not only that, but since every blip from a given map room would've been showing the exact same text anyway, blips moving around didn't matter, until we wanted blips with different text.

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
#if SN1
			text = gameObject.GetComponentInChildren<Text>();
#elif BELOWZERO
			text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
#endif
		}

#if SN1
		public void Refresh(ResourceTracker.ResourceInfo target)
#elif BELOWZERO
		public void Refresh(ResourceTrackerDatabase.ResourceInfo target)
#endif
		{
			if (target != null)
			{
				var vectorToPlayer = Player.main.transform.position - target.position;
				var distance = vectorToPlayer.magnitude;

				if(this.techType == TechType.None)
					this.techType = target.techType;
				if (this.techType == TechType.Fragment || string.IsNullOrEmpty(resourceName))
				{
					/*blipId = gameObject.EnsureComponent<BlipIdentifier>();
					blipId.uniqueId = target.uniqueId;*/
					if (this.techType == TechType.Fragment)
					{
						/*thisTechType = blipId.actualTechType;
						if (thisTechType == TechType.None || thisTechType == TechType.Fragment)
						{
							QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
								$"CustomBlip.Refresh(): Could not get TechType from BlipIdentifier, attempting to use ResourceTrackerPatches.GetTechTypeForId");
							thisTechType = ResourceTrackerPatches.GetTechTypeForId(target.uniqueId);
							QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
								$"CustomBlip.Refresh(): Got TechType of {techType.AsString()}");
						}*/
						if (lastID != target.uniqueId)
						{
							/*QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
								$"CustomBlip.Refresh(): Last unique ID {lastID} does not match current unique ID {target.uniqueId}; Attempting to establish resourceName for ResourceInfo, which includes TechType {thisTechType}");
							*/
							if (!ResourceTrackerPatches.TryGetResourceName(target.uniqueId, out resourceName))
							{
								resourceName = Language.main.Get(this.techType);
								lastID = target.uniqueId;
								/*QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
									$"For unique ID {target.uniqueId}, got TechType {thisTechType.AsString()} and resourceName '{resourceName}'");*/
							}
						}
					}

					/*if (thisTechType != TechType.None && thisTechType != TechType.Fragment)
					{

						techType = thisTechType;
						resourceName = Language.main.Get(thisTechType);
						lastID = target.uniqueId;
						QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug,
							$"For unique ID {target.uniqueId}, got TechType {thisTechType.AsString()} and resourceName '{resourceName}'");
					}*/
					else
						resourceName = Language.main.Get(target.techType);
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
