using Common.Mod;
using Common.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if BZ
using TMPro;
#endif

namespace AutosortLockers
{
	public class ColoredIconButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		private static readonly Color DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		private Color UpColor = new Color32(66, 134, 244, 255);
		private static readonly Color HoverColor = new Color(0.9f, 0.9f, 1);
		private static readonly Color DownColor = new Color(0.9f, 0.9f, 1, 0.8f);

		public bool isEnabled = true;
		public bool pointerOver;
		public bool pointerDown;
		public Color imageColor;

		public Image image;
#if SN
		public Text text;
#elif BZ
		public TextMeshProUGUI text;
#endif
		public Action onClick = delegate { };

		private Color imageDisabledColor;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (isEnabled)
			{
				onClick();
			}
		}

		public void Initialize(string spriteName, Color color)
		{
			var sprite = ImageUtils.LoadSprite(Mod.GetAssetPath(spriteName), new Vector2(0.5f, 0.5f));
			Initialize(sprite, color);
		}

		public void Initialize(Sprite sprite, Color color)
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

			if (image != null)
			{
				image.color = isEnabled ? imageColor : imageDisabledColor;
			}
			if (text != null)
			{
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

		/*_____________________________________________________________________________________________________*/

		public static ColoredIconButton Create(Transform parent, Color color,
#if SN
			Text textPrefab = null,
#elif BZ
			TextMeshProUGUI textPrefab = null,
#endif
			string label = "", float width = 100, float iconWidth = 20)
		{
			var checkboxButton = new GameObject("Checkbox", typeof(RectTransform));
			var rt = checkboxButton.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, width, 20);
			rt.anchoredPosition = new Vector2(0, 0);

			var checkbox = LockerPrefabShared.CreateIcon(rt, color, 0);
			RectTransformExtensions.SetSize(checkbox.rectTransform, iconWidth, iconWidth);
			checkbox.rectTransform.anchoredPosition = new Vector2(textPrefab != null ? - width / 2 + 10 : 0, 0);

#if SN
			Text text = null;
#elif BZ
			TextMeshProUGUI text = null;
#endif

			if (textPrefab != null)
			{
				var spacing = 5;
				text = LockerPrefabShared.CreateText(rt, textPrefab, color, 0, 10, label, "Label");
				RectTransformExtensions.SetSize(text.rectTransform, width - 20 - spacing, 20);
				text.rectTransform.anchoredPosition = new Vector2(10 + spacing, 0);
#if SN
				text.alignment = TextAnchor.MiddleLeft;
#elif BZ
				text.alignment = TextAlignmentOptions.MidlineLeft;
#endif
			}

			checkboxButton.AddComponent<BoxCollider2D>();

			var button = checkboxButton.AddComponent<ColoredIconButton>();
			button.image = checkbox;
			button.text = text ?? null;
			button.UpColor = color;

			return button;
		}
	}
}