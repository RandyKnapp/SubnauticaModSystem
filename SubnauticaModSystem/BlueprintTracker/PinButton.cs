using Common.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlueprintTracker
{
	class PinButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
	{
		private static readonly Vector2 Size = new Vector2(50, 50);
		private static readonly Vector2 Position = new Vector2(-45, -45);
		private static readonly Vector3 DownScale = new Vector3(0.9f, 0.9f, 1);
		private static readonly Vector3 HoverScale = new Vector3(1.2f, 1.2f, 1);
		private static readonly Color HoverColor = new Color(0.956f, 0.796f, 0.258f);
		private static readonly Color CrossColor = new Color(1, 0, 0, 0.5f);

		private static Sprite addPinSprite;
		private static Sprite removePinSprite;
		private static Sprite crossSprite;

		public Action onClick = delegate { };

		private RectTransform rectTransform;
		private LayoutElement layout;
		private Image image;
		private bool hover;
		private bool down;
		private Mode mode;

		public enum Mode { Add, Remove, Cross }

		private void Awake()
		{
			if (addPinSprite == null)
			{
				addPinSprite = ImageUtils.LoadSprite(Mod.GetAssetPath("pin_not_pinned.png"));
				removePinSprite = ImageUtils.LoadSprite(Mod.GetAssetPath("pin_pinned.png"));
				crossSprite = ImageUtils.LoadSprite(Mod.GetAssetPath("remove_pin.png"));
			}

			rectTransform = transform as RectTransform;
			rectTransform.anchorMin = new Vector2(1, 1);
			rectTransform.anchorMax = new Vector2(1, 1);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = Position;
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Size.x);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Size.y);

			layout = gameObject.AddComponent<LayoutElement>();
			layout.ignoreLayout = true;

			image = gameObject.AddComponent<Image>();
			image.sprite = addPinSprite;
		}

		public void OnDisable()
		{
			hover = false;
			down = false;
			Update();
		}

		public void SetMode(PinButton.Mode mode)
		{
			this.mode = mode;
			switch (mode)
			{
				case Mode.Add:
					image.sprite = addPinSprite;
					break;
				case Mode.Remove:
					image.sprite = removePinSprite;
					break;
				case Mode.Cross:
					image.sprite = crossSprite;
					break;
			}
			Update();
		}

		private void Update()
		{
			image.transform.localScale = down ? DownScale : hover ? HoverScale : new Vector3(1, 1, 1);

			switch (mode)
			{
				case Mode.Add:
					image.color = hover ? HoverColor : Color.white;
					break;
				case Mode.Remove:
					image.color = HoverColor;
					break;
				case Mode.Cross:
					image.color = CrossColor;
					break;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (mode != Mode.Cross)
			{
				onClick.Invoke();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hover = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			hover = false;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (mode == Mode.Cross)
			{
				onClick.Invoke();
			}
			down = true;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			down = false;
		}
	}
}
