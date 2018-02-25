using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class PickerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private Image background;
		[SerializeField]
		private Image hover;
		[SerializeField]
		private Text text;

		public void OnPointerClick(PointerEventData eventData)
		{
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		public void OnPointerExit(PointerEventData eventData)
		{
		}



		public static PickerButton Create(string t)
		{
			var button = new GameObject("PickerButton", typeof(RectTransform)).AddComponent<PickerButton>();

			int width = 80;
			int height = 30;
			int slice = 15;

			button.background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
			RectTransformExtensions.SetParams(button.background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(button.background.rectTransform, width, height);
			button.background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(slice, slice, slice, slice));
			button.background.color = new Color(1, 1, 1, 0.8f);

			button.hover = new GameObject("Hover", typeof(RectTransform)).AddComponent<Image>();
			RectTransformExtensions.SetParams(button.hover.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(button.hover.rectTransform, width, height);
			button.hover.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(slice, slice, slice, slice));
			button.hover.color = new Color(1, 1, 1, 1);

			button.text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
			RectTransformExtensions.SetParams(button.text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(button.text.rectTransform, width, height);
			button.text.text = t;
			button.text.color = new Color(1, 1, 1);
			button.text.font = new Font("Aller");
			button.text.fontSize = 20;

			return button;
		}
	}
}
