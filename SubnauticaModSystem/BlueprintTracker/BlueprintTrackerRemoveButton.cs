using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTracker
{
	class BlueprintTrackerRemoveButton : MonoBehaviour
	{
		public const float Width = BlueprintTrackerEntry.Height;

		private TechType techType;
		private LayoutElement layout;
		private Image background;
		private PinButton button;

		public static BlueprintTrackerRemoveButton Create(Transform parent, TechType techType, bool first, bool last)
		{
			var go = new GameObject("StopTrackingButton", typeof(RectTransform));
			go.transform.SetParent(parent, false);
			go.layer = parent.gameObject.layer;
			var icon = go.AddComponent<BlueprintTrackerRemoveButton>();
			icon.Init(techType, first, last);

			return icon;
		}

		private void Init(TechType techType, bool first, bool last)
		{
			this.techType = techType;

			var quickSlots = GameObject.FindObjectOfType<uGUI_QuickSlots>();
			var bgSprite = first ? quickSlots.spriteLeft : last ? quickSlots.spriteRight : quickSlots.spriteCenter;

			(transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bgSprite.rect.width);
			(transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bgSprite.rect.height);

			layout = gameObject.AddComponent<LayoutElement>();
			layout.minWidth = bgSprite.rect.width;
			layout.minHeight = bgSprite.rect.height;

			background = gameObject.AddComponent<Image>();
			background.color = new Color(1, 1, 1, 0.5f);
			background.raycastTarget = false;
			background.material = quickSlots.materialBackground;
			background.sprite = bgSprite;

			button = new GameObject(name, typeof(RectTransform)).AddComponent<PinButton>();
			var brt = button.transform as RectTransform;
			RectTransformExtensions.SetParams(brt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
			brt.anchoredPosition = new Vector2(0, 0);
			RectTransformExtensions.SetSize(brt, 30, 30);

			button.SetMode(PinButton.Mode.Cross);
			button.onClick += OnButtonClick;
		}

		private void OnButtonClick()
		{
			BlueprintTracker.StopTracking(techType);
		}
	}
}
