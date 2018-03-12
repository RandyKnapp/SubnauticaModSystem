using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	public class uGUI_ColoredIconButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		private static readonly Color DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		private Color UpColor = new Color32(66, 134, 244, 255);
		private static readonly Color HoverColor = new Color(0.9f, 0.9f, 1);
		private static readonly Color DownColor = new Color(0.9f, 0.9f, 1, 0.8f);

		public bool isEnabled = true;
		public bool pointerOver;
		public bool pointerDown;
		public Color imageColor;

		public uGUI_Icon image;
		public Text text;
		public Action onClick = delegate { };

		private Color imageDisabledColor;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (isEnabled)
			{
				onClick();
			}
		}

		public void Initialize(Atlas.Sprite sprite, Color color)
		{
			imageColor = color;

			color.a = 0.5f;
			imageDisabledColor = color;

			image.sprite = sprite;
			image.color = imageColor;
		}

		public void Update()
		{
			var color = !isEnabled ? DisabledColor : (pointerDown ? DownColor : (pointerOver ? HoverColor : UpColor));

			if (image != null && text != null)
			{
				image.color = isEnabled ? imageColor : imageDisabledColor;
				text.color = color;
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
		public static uGUI_ColoredIconButton Create(Transform parent, Color color, Text textPrefab, string label, float width = 100, float iconWidth = 20)
		{
			var checkboxButton = new GameObject("Checkbox", typeof(RectTransform));
			var rt = checkboxButton.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, width, 20);
			rt.anchoredPosition = new Vector2(0, 0);

			var icon = new GameObject("Icon", typeof(RectTransform)).AddComponent<uGUI_Icon>();
			RectTransformExtensions.SetParams(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(icon.rectTransform, iconWidth, iconWidth);
			icon.rectTransform.anchoredPosition = new Vector2(-width / 2 + 10, 0);

			var spacing = 5;
			var text = LockerPrefabShared.CreateText(rt, textPrefab, color, 0, 10, label);
			RectTransformExtensions.SetSize(text.rectTransform, width - 20 - spacing, 20);
			text.rectTransform.anchoredPosition = new Vector2(10 + spacing, 0);
			text.alignment = TextAnchor.MiddleLeft;

			checkboxButton.AddComponent<BoxCollider2D>();

			var button = checkboxButton.AddComponent<uGUI_ColoredIconButton>();
			button.image = icon;
			button.text = text;
			button.UpColor = color;

			return button;
		}
	}
}
