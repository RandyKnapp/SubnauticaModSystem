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
		private static readonly Color upColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
		private static readonly Color hoverColor = new Color(1, 1, 1);

		private bool hover;
		private TechType techType;

		public Action<TechType> onClick = delegate { };

		[SerializeField]
		private Image background;
		[SerializeField]
		private Text text;

		public TechType GetTechType()
		{
			return techType;
		}

		public void SetTechType(TechType value)
		{
			techType = value;
			text.text = Language.main.Get(techType);

			gameObject.SetActive(techType != TechType.None);
		}

		public void Update()
		{
			background.color = hover ? hoverColor : upColor;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			onClick.Invoke(GetTechType());
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hover = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			hover = false;
		}



		public static PickerButton Create(Transform parent, TechType techType, Text textPrefab, Action<TechType> action)
		{
			var button = new GameObject("PickerButton", typeof(RectTransform)).AddComponent<PickerButton>();
			button.transform.SetParent(parent, false);

			int width = 100;
			int height = 18;
			int slice = 15;

			button.background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
			RectTransformExtensions.SetParams(button.background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(button.background.rectTransform, width * 5, height * 5);
			button.background.rectTransform.localScale = new Vector3(0.2f, 0.2f, 1);
			button.background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(slice, slice, slice, slice));
			button.background.color = upColor;
			button.background.type = Image.Type.Sliced;

			button.text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
			RectTransformExtensions.SetParams(button.text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(button.text.rectTransform, width, height);
			button.text.text = Language.main.Get(techType);
			button.text.color = new Color(1, 1, 1);
			button.text.font = textPrefab.font;
			button.text.fontSize = 10;
			button.text.alignment = TextAnchor.MiddleCenter;

			button.SetTechType(techType);
			button.onClick += action;

			return button;
		}
	}
}
