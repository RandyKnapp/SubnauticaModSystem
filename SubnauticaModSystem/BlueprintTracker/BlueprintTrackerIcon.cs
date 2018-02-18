using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTracker
{
	class BlueprintTrackerIcon : MonoBehaviour
	{
		public const float Width = BlueprintTrackerEntry.Height;
		public const float IconWidth = 50;
		public const string IngredientColorGood = "#94DE00FF";
		public const string IngredientColorBad = "#DF4026FF";

		public const string IngredientColorGoodColorblind = "#00FFEDFF";
		public const string IngredientColorBadColorblind = "#F8B5FFFF";

		private Color goodColor;
		private Color badColor;

		public IIngredient ingredient;

		private uGUI_ItemIcon icon;
		private Text text;
		private LayoutElement layout;
		private int currentAmount = -1;
		private Image background;

		public static BlueprintTrackerIcon Create(Transform parent, IIngredient ingredient, Atlas.Sprite sprite, bool first, bool last)
		{
			var go = new GameObject("TrackerIcon", typeof(RectTransform));
			go.transform.SetParent(parent, false);
			go.layer = parent.gameObject.layer;
			var icon = go.AddComponent<BlueprintTrackerIcon>();
			icon.Init(ingredient, sprite, first, last);

			return icon;
		}

		private void Init(IIngredient ingredient, Atlas.Sprite sprite, bool first, bool last)
		{
			this.ingredient = ingredient;
			bool mainIcon = ingredient == null;

			var quickSlots = GameObject.FindObjectOfType<uGUI_QuickSlots>();
			var bgSprite = first ? quickSlots.spriteLeft : last ? quickSlots.spriteRight : quickSlots.spriteCenter;

			(transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bgSprite.rect.width);
			(transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bgSprite.rect.height);

			layout = gameObject.AddComponent<LayoutElement>();
			layout.minWidth = bgSprite.rect.width;
			layout.minHeight = bgSprite.rect.height;

			background = gameObject.AddComponent<Image>();
			background.color = new Color(1, 1, 1, Mod.config.BackgroundAlpha);
			background.raycastTarget = false;
			background.material = quickSlots.materialBackground;
			background.sprite = bgSprite;

			icon = new GameObject("Icon").AddComponent<uGUI_ItemIcon>();
			icon.transform.SetParent(transform, false);
			icon.SetForegroundSprite(sprite);
			if (mainIcon)
			{
				icon.SetSize(Width, Width);
			}
			else
			{
				icon.SetSize(IconWidth, IconWidth);
			}
			icon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			icon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			icon.rectTransform.anchoredPosition = new Vector2(0, 0);
			icon.raycastTarget = false;

			if (!mainIcon)
			{
				text = Mod.InstantiateNewText("Text", transform);
				RectTransformExtensions.SetSize(text.rectTransform, Width, Width);
				text.rectTransform.anchorMin = new Vector2(0.5f, 0);
				text.rectTransform.anchorMax = new Vector2(0.5f, 0);
				text.rectTransform.pivot = new Vector2(0.5f, 0);
				text.rectTransform.anchoredPosition = new Vector2(0, 0);
				text.alignment = TextAnchor.LowerCenter;
				text.fontSize = Mod.config.FontSize;
				text.raycastTarget = false;

				ColorUtility.TryParseHtmlString(Mod.config.ColorblindMode ? IngredientColorGoodColorblind : IngredientColorGood, out goodColor);
				ColorUtility.TryParseHtmlString(Mod.config.ColorblindMode ? IngredientColorBadColorblind : IngredientColorBad, out badColor);
			}

			UpdateText();
		}

		private void OnEnable()
		{
			StartCoroutine(UpdateRoutine());
		}

		private IEnumerator UpdateRoutine()
		{
			for (;;)
			{
				yield return new WaitForSeconds(0.2f + UnityEngine.Random.Range(0, 0.05f));
				UpdateText();
			}
		}

		private void UpdateText()
		{
			if (ingredient == null)
			{
				return;
			}
			int newCurrent = GetCurrentAmount();
			if (newCurrent != currentAmount)
			{
				currentAmount = newCurrent;
				int required = ingredient.amount;
				bool hasEnough = currentAmount >= required;

				text.color = hasEnough ? goodColor : badColor;
				text.text = string.Format("{0}/{1}", Math.Min(currentAmount, required), required);
			}
		}

		private void DebugLog(string message)
		{
			if (ingredient != null && ingredient.techType == TechType.CopperWire)
			{
				Logger.Log(message);
			}
		}

		private int GetCurrentAmount()
		{
			if (ingredient == null)
			{
				return 0;
			}
			return Inventory.main.GetPickupCount(ingredient.techType);
		}
	}
}
