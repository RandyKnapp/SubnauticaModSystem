using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HabitatControlPanel.Secret
{
	class SecretButton : MonoBehaviour, IPointerClickHandler
	{
		public Action onActivate = delegate { };

		public void OnPointerClick(PointerEventData eventData)
		{
			onActivate();
		}



		public static SecretButton Create(Transform parent)
		{
			var button = new GameObject("SecretButton", typeof(RectTransform)).AddComponent<SecretButton>();
			RectTransformExtensions.SetParams(button.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(button.transform as RectTransform, 15, 15);

			var image = button.gameObject.AddComponent<Image>();
			image.color = new Color(0, 0, 0, 0.0001f);

			return button;
		}
	}
}
