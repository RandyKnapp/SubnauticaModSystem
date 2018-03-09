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
	class ColorPickerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private bool pointerOver;
		private int index;

		public RectTransform rectTransform;
		public bool toggled;
		public Image image;
		public Image highlight;

		public Action<int> onClick = delegate { };

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			onClick(index);
		}

		public void Initialize(int index, Color color, bool toggled)
		{
			this.index = index;
			this.toggled = toggled;

			if (highlight.sprite == null)
			{
				highlight.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"));
			}

			if (image.sprite == null)
			{
				image.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"));
				image.color = color;
			}
		}

		public void Update()
		{
			highlight.gameObject.SetActive(toggled || pointerOver);
			highlight.transform.localScale = new Vector3(toggled ? 1 : 0.8f, toggled ? 1 : 0.8f, 1);
			highlight.color = new Color(1, 1, 1, toggled ? 1 : 0.7f);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			pointerOver = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			pointerOver = false;
		}


		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static ColorPickerButton Create(Transform parent, float width, float iconWidth)
		{
			var button = new GameObject("ColorButton", typeof(RectTransform)).AddComponent<ColorPickerButton>();
			var rt = button.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, width, width);

			var highlight = LockerPrefabShared.CreateIcon(rt, Color.white, 0);
			RectTransformExtensions.SetParams(highlight.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(highlight.rectTransform, width, width);
			button.highlight = highlight;

			var image = LockerPrefabShared.CreateIcon(rt, Color.white, 0);
			RectTransformExtensions.SetParams(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(image.rectTransform, iconWidth, iconWidth);
			button.image = image;

			return button;
		}
	}
}
