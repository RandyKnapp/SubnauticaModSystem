using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AutosortLockers
{
	public class PickerCloseButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public bool pointerOver;
		public AutosortTarget target;
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if (target != null && enabled)
			{
				target.HideConfigureMenu();
			}
		}

		public void Update()
		{
			transform.localScale = new Vector3(pointerOver ? 1.3f : 1, pointerOver ? 1.3f : 1, 1);
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
