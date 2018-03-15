using Common.Mod;
using Common.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomBeacons
{
	public class PingColorButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		private Color UpColor = new Color32(66, 134, 244, 255);
		private static readonly Color HoverColor = new Color(0.9f, 0.9f, 1);
		private static readonly Color DownColor = new Color(0.9f, 0.9f, 1, 0.8f);

		public bool pointerOver;
		public bool pointerDown;

		public Image image;
		public Image editImage;
		public Action onClick = delegate { };

		public void OnPointerClick(PointerEventData eventData)
		{
			onClick();
		}

		public void Initialize(Color color)
		{
			if (editImage.sprite == null)
			{
				editImage.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Edit.png"), new Vector2(0.5f, 0.5f));
			}
			if (image.sprite == null)
			{
				image.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"), new Vector2(0.5f, 0.5f));
			}

			image.color = color;
		}

		public void Update()
		{
			var color = (pointerDown ? DownColor : (pointerOver ? HoverColor : UpColor));

			if (editImage != null)
			{
				editImage.color = color;
				editImage.gameObject.SetActive(pointerOver || pointerDown);
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
		public static PingColorButton Create(Transform parent, Color color, float iconWidth = 60, float editIconWidth = 30)
		{
			var button = new GameObject("ColorButton", typeof(RectTransform));
			var rt = button.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, iconWidth, iconWidth);
			rt.anchoredPosition = new Vector2(0, 0);

			var icon = LockerPrefabShared.CreateIcon(rt, color, 0);
			icon.gameObject.name = "ColorIcon";
			RectTransformExtensions.SetSize(icon.rectTransform, iconWidth, iconWidth);
			icon.rectTransform.anchoredPosition = new Vector2(0, 0);

			var editIcon = LockerPrefabShared.CreateIcon(rt, color, 0);
			editIcon.gameObject.name = "EditIcon";
			RectTransformExtensions.SetSize(editIcon.rectTransform, editIconWidth, editIconWidth);
			editIcon.rectTransform.anchoredPosition = new Vector2(0, 0);
			editIcon.gameObject.SetActive(false);

			button.AddComponent<BoxCollider2D>();

			var pingColorButton = button.AddComponent<PingColorButton>();
			pingColorButton.image = icon;
			pingColorButton.editImage = editIcon;

			var layout = pingColorButton.gameObject.AddComponent<LayoutElement>();
			layout.ignoreLayout = true;

			return pingColorButton;
		}
	}
}
