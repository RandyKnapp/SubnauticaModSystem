using Common.Mod;
using Common.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class ColorPickerPageButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		private Color UpColor = new Color32(66, 134, 244, 255);
		private static readonly Color HoverColor = new Color(0.9f, 0.9f, 1);
		private static readonly Color DownColor = new Color(0.9f, 0.9f, 1, 0.8f);

		public bool pointerOver;
		public bool pointerDown;
		public Color imageColor;

		public Image image;
		public Action onClick = delegate { };

		public void OnPointerClick(PointerEventData eventData)
		{
			onClick();
		}

		public void Initialize(string spriteName, Color color)
		{
			var sprite = ImageUtils.LoadSprite(Mod.GetAssetPath(spriteName), new Vector2(0.5f, 0.5f));
			Initialize(sprite, color);
		}

		public void Initialize(Sprite sprite, Color color)
		{
			imageColor = color;

			image.sprite = sprite;
			image.color = imageColor;
		}

		public void Update()
		{
			var color = (pointerDown ? DownColor : (pointerOver ? HoverColor : UpColor));
			if (image != null)
			{
				image.color = color;
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

		public static ColorPickerPageButton Create(Transform parent, Color color, float iconWidth = 20)
		{
			var pageButton = new GameObject("ColorPickerPageButton", typeof(RectTransform));
			var rt = pageButton.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);

			var icon = LockerPrefabShared.CreateIcon(rt, color, 0);
			// Size of the arrows
			RectTransformExtensions.SetSize(icon.rectTransform, iconWidth, iconWidth);

			pageButton.AddComponent<BoxCollider2D>();

			var button = pageButton.AddComponent<ColorPickerPageButton>();
			button.image = icon;

			return button;
		}
	}
}