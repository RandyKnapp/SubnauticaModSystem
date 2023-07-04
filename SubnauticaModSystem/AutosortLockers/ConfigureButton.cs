using Common.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AutosortLockers
{
	public class ConfigureButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public bool pointerOver;
		public RectTransform rectTransform;
		public Action onClick = delegate { };
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if (enabled)
			{
				onClick();
			}
		}

		private void Awake()
		{
			rectTransform = transform as RectTransform;
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


		public static ConfigureButton Create(Transform parent, Color color, float x, int buttonPos)
		{
			var config = LockerPrefabShared.CreateIcon(parent, color, 0);
			RectTransformExtensions.SetSize(config.rectTransform, 20, 20);
			config.rectTransform.anchoredPosition = new Vector2(x, buttonPos);

			config.gameObject.AddComponent<BoxCollider2D>();
			var button = config.gameObject.AddComponent<ConfigureButton>();

			return button;
		}
	}
}