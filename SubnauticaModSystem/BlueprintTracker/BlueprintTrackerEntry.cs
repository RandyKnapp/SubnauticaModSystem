using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTracker
{
	class BlueprintTrackerEntry : MonoBehaviour
	{
		public const float Height = 70;
		public const float IconSpacing = -6;

		private LayoutElement layout;
		private HorizontalLayoutGroup contents;

		public TechType techType;

		public static BlueprintTrackerEntry Create(Transform parent, TechType techType)
		{
			var go = new GameObject("TrackerEntry" + techType, typeof(RectTransform));
			go.transform.SetParent(parent, false);
			go.layer = parent.gameObject.layer;
			var entry = go.AddComponent<BlueprintTrackerEntry>();
			entry.SetTechType(techType);

			return entry;
		}

		private void Awake()
		{
			var quickSlots = GameObject.FindObjectOfType<uGUI_QuickSlots>();

			layout = gameObject.AddComponent<LayoutElement>();
			layout.minHeight = Height;

			var iconContainer = new GameObject("Icons", typeof(RectTransform));
			var rt = iconContainer.transform as RectTransform;
			rt.anchorMin = new Vector2(0, 0);
			rt.anchorMax = new Vector2(1, 1);
			rt.anchoredPosition = new Vector2(0, 0);
			rt.sizeDelta = new Vector2(0, 0);

			contents = iconContainer.AddComponent<HorizontalLayoutGroup>();
			contents.transform.SetParent(transform, false);
			contents.childAlignment = TextAnchor.UpperLeft;
			contents.spacing = IconSpacing;
			contents.childForceExpandHeight = false;
			contents.childForceExpandWidth = false;
			contents.childControlHeight = true;
			contents.childControlWidth = false;
			contents.padding.left = 0;
		}

		public void SetTechType(TechType techType)
		{
			if (this.techType == techType)
			{
				return;
			}

			this.techType = techType;

			ITechData techData = CraftData.Get(techType, true);
			if (techData == null)
			{
				Logger.Error("Could not find tech data for techtype: " + techType);
				return;
			}

			var quickSlots = GameObject.FindObjectOfType<uGUI_QuickSlots>();

			bool locked = false;

			BlueprintTrackerIcon.Create(contents.transform, null, SpriteManager.Get(techType), true, false);

			string tooltipText;
			List<TooltipIcon> iconData = new List<TooltipIcon>();
			TooltipFactory.BuildTech(techType, locked, out tooltipText, iconData);

			for (int i = 0; i < techData.ingredientCount; ++i)
			{
				IIngredient ingredient = techData.GetIngredient(i);
				Atlas.Sprite sprite = iconData[i].sprite;

				BlueprintTrackerIcon.Create(contents.transform, ingredient, sprite, false, i == techData.ingredientCount - 1);
			}
		}
	}
}
