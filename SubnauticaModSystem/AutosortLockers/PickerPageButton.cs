using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class PickerPageButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private static readonly Color DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
		private static readonly Color NormalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
		private static readonly Color HoverColor = Color.white;

		public bool canChangePage;
		public bool pointerOver;
		public AutosortTypePicker target;
		public int pageOffset;
		public Image image;

		public void Awake()
		{
			image = GetComponent<Image>();
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if (canChangePage)
			{
				target.ChangePage(pageOffset);
			}
		}

		public void Update()
		{
			image.color = canChangePage ? (pointerOver ? HoverColor : NormalColor) : DisabledColor;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			pointerOver = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			pointerOver = false;
		}
	}
}
