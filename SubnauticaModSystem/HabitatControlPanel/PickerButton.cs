using Common.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class PickerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private bool pointerOver;
		private int id;

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
			var sprite = new Atlas.Sprite(imageSprite);
			Initialize(id, color, toggled, sprite);
		}

		public void Initialize(int id, Color color, bool toggled, Atlas.Sprite imageSprite)
		{
			this.id = id;
			this.toggled = toggled;

			if (highlight.sprite == null)
			{
				highlight.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"), new Vector2(0.5f, 0.5f));
			}

			image.sprite = imageSprite;
			image.color = color;
		}

		public void Update()
		{
			highlight.gameObject.SetActive(toggled || pointerOver);
			highlight.transform.localScale = new Vector3(toggled ? 1 : 0.8f, toggled ? 1 : 0.8f, 1);
			highlight.color = new Color(1, 1, 1, toggled ? 0.5f : 0.7f);
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
		public static PickerButton Create(Transform parent, float width, float iconWidth)
		{
			var button = new GameObject("PickerButton", typeof(RectTransform)).AddComponent<PickerButton>();
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
