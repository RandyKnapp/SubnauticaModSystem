using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DockedVehicleStorageAccess
{
	public class CheckboxButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		public static readonly Color DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		public static readonly Color UpColor = new Color32(66, 134, 244, 255);
		public static readonly Color HoverColor = new Color(0.9f, 0.9f, 1);
		public static readonly Color DownColor = new Color(0.9f, 0.9f, 1, 0.8f);

		public bool toggled;
		public bool pointerOver;
		public bool pointerDown;
		public VehicleStorageAccess target;

		public Image image;
		public Text text;

		private Sprite checkedSprite = null;
		private Sprite uncheckedSprite = null;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (enabled)
			{
				toggled = !toggled;
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
			var color = !enabled ? DisabledColor : (pointerDown ? DownColor : (pointerOver ? HoverColor : UpColor));

			if (image != null && text != null)
			{
				image.color = color;
				text.color = color;
			}

			if (checkedSprite != null && uncheckedSprite != null)
			{
				image.sprite = !enabled ? uncheckedSprite : (toggled ? checkedSprite : uncheckedSprite);
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
	}
}
