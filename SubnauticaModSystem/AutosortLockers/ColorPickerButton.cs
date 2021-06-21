using Common.Mod;
using Common.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutosortLockers
{
	class ColorPickerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private bool pointerOver;
		private int id;
		private Sprite hoverSprite;
		private Sprite selectedSprite;

		public RectTransform rectTransform;
		public bool toggled;
		public uGUI_Icon image;
		public Image highlight;

		public Action<int> onClick = delegate { };

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			onClick(id);
		}

		public void Initialize(int id, Color color, bool toggled, Sprite imageSprite)
		{
			this.id = id;
			this.toggled = toggled;

			if (hoverSprite == null)
			{
				hoverSprite = ImageUtils.LoadSprite(Mod.GetAssetPath("SelectorHover.png"), new Vector2(0.5f, 0.5f));
			}
			if (selectedSprite == null)
			{
				selectedSprite = ImageUtils.LoadSprite(Mod.GetAssetPath("SelectorSelected.png"), new Vector2(0.5f, 0.5f));
			}

			highlight.sprite = hoverSprite;

			image.sprite = imageSprite;
			image.color = color;
		}

		public void Update()
		{
			highlight.gameObject.SetActive(toggled || pointerOver);
			highlight.color = new Color(1, 1, 1, 1);
			highlight.sprite = toggled ? selectedSprite : hoverSprite;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			pointerOver = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			pointerOver = false;
		}

		/*_____________________________________________________________________________________________________*/

		public static ColorPickerButton Create(Transform parent, float width, float iconWidth)
		{
			var button = new GameObject("ColorPickerButton", typeof(RectTransform)).AddComponent<ColorPickerButton>();
			var rt = button.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, width, width);

			var highlight = LockerPrefabShared.CreateIcon(rt, Color.white, 0);
			RectTransformExtensions.SetParams(highlight.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(highlight.rectTransform, width, width);
			button.highlight = highlight;
			button.highlight.gameObject.SetActive(false);

			var image = new GameObject("Image", typeof(RectTransform)).AddComponent<uGUI_Icon>();
			RectTransformExtensions.SetParams(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(image.rectTransform, iconWidth, iconWidth);
			image.rectTransform.anchoredPosition = new Vector2(0, 0);
			button.image = image;

			return button;
		}
	}
}