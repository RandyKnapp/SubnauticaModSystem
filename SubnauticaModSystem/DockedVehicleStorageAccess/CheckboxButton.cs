﻿using Common.Mod;
using Common.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if BZ
using TMPro;
#endif

namespace DockedVehicleStorageAccess
{
	public class CheckboxButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		private static readonly Color DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		private Color UpColor = new Color32(66, 134, 244, 255);
		private static readonly Color HoverColor = new Color(0.9f, 0.9f, 1);
		private static readonly Color DownColor = new Color(0.9f, 0.9f, 1, 0.8f);

		public bool isEnabled = true;
		public bool toggled = true;
		public bool pointerOver;
		public bool pointerDown;

		public RectTransform rectTransform;
		public Image image;
#if SN1
		public Text text;
#elif BZ
		public TextMeshProUGUI text;
#endif
		public Action<bool> onToggled = delegate { };

		private Sprite checkedSprite = null;
		private Sprite uncheckedSprite = null;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (isEnabled)
			{
				toggled = !toggled;
				onToggled(toggled);
			}
		}

		public void Initialize()
		{
			if (checkedSprite == null)
			{
				checkedSprite = ImageUtils.LoadSprite(Mod.GetAssetPath("CheckboxChecked.png"));
			}
			if (uncheckedSprite == null)
			{
				uncheckedSprite = ImageUtils.LoadSprite(Mod.GetAssetPath("CheckboxUnchecked.png"));
			}
		}

		public void Update()
		{
			var color = !isEnabled ? DisabledColor : (pointerDown ? DownColor : (pointerOver ? HoverColor : UpColor));

			if (image != null)
			{
				image.color = color;
			}
			if (text != null)
			{
				text.color = color;
			}

			if (checkedSprite != null && uncheckedSprite != null)
			{
				image.sprite = !isEnabled ? uncheckedSprite : (toggled ? checkedSprite : uncheckedSprite);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			pointerOver = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			pointerOver = false;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			pointerDown = true;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			pointerDown = false;
		}


		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if SN1
		public static CheckboxButton CreateCheckbox(Transform parent, Color color, Text textPrefab, string label, float width = 100)
		{
#elif BZ
		public static CheckboxButton CreateCheckbox(Transform parent, Color color, TextMeshProUGUI textPrefab, string label, float width = 100)
		{
#endif
		var checkboxButton = new GameObject("Checkbox", typeof(RectTransform));
			var rt = checkboxButton.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, width, 20);
			rt.anchoredPosition = new Vector2(0, 0);

			var iconWidth = 20;
			var checkbox = LockerPrefabShared.CreateIcon(rt, color, 0);
			RectTransformExtensions.SetSize(checkbox.rectTransform, iconWidth, iconWidth);
			checkbox.rectTransform.anchoredPosition = new Vector2(-width / 2 + iconWidth / 2, 0);

			var spacing = 5;
			var text = LockerPrefabShared.CreateText(rt, textPrefab, color, 0, 10, label);
			RectTransformExtensions.SetSize(text.rectTransform, width - iconWidth - spacing, iconWidth);
			text.rectTransform.anchoredPosition = new Vector2(iconWidth / 2 + spacing, 0);
#if SN1
			text.alignment = TextAnchor.MiddleLeft;
#elif BZ
			text.alignment = TextAlignmentOptions.MidlineLeft;
#endif
			checkboxButton.AddComponent<BoxCollider2D>();

			var button = checkboxButton.AddComponent<CheckboxButton>();
			button.image = checkbox;
			button.text = text;
			button.UpColor = color;

			return button;
		}

		public static CheckboxButton CreateCheckboxNoText(Transform parent, Color color, float iconWidth)
		{
			var checkboxButton = new GameObject("Checkbox", typeof(RectTransform));
			var rt = checkboxButton.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, iconWidth, iconWidth);
			rt.anchoredPosition = new Vector2(0, 0);

			var checkbox = LockerPrefabShared.CreateIcon(rt, color, 0);
			RectTransformExtensions.SetSize(checkbox.rectTransform, iconWidth, iconWidth);
			checkbox.rectTransform.anchoredPosition = new Vector2(0, 0);

			checkboxButton.AddComponent<BoxCollider2D>();

			var button = checkboxButton.AddComponent<CheckboxButton>();
			button.image = checkbox;
			button.UpColor = color;

			return button;
		}
	}
}
