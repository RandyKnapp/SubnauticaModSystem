using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AutosortLockers
{
	public class PickerPageButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public bool pointerOver;
		public AutosortTypePicker target;
		public int pageOffset;
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if (enabled)
			{
				target.ChangePage(pageOffset);
			}
		}

		public void Update()
		{
			var hover = enabled && pointerOver;
			transform.localScale = new Vector3(hover ? 1.3f : 1, hover ? 1.3f : 1, 1);
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
