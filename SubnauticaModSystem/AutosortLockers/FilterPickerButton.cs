using Common.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if BELOWZERO
using TMPro;
#endif

namespace AutosortLockers
{
	public class FilterPickerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private static readonly Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
		private static readonly Color inactiveHoverColor = new Color(0.7f, 0.7f, 0.7f, 1f);
		private static readonly Color upColor = new Color(0.9f, 0.9f, 0.9f, 1f);
		private static readonly Color hoverColor = new Color(1, 1, 1);
		private const int Slice = 70;
		private bool hover;
		private bool tabActive = true;
		private AutosorterFilter filter;

		public Action<AutosorterFilter> onClick = delegate { };

		[SerializeField]
		private Image background;
		[SerializeField]
#if SUBNAUTICA
		private Text text;
#elif BELOWZERO
		private TextMeshProUGUI text;
#endif

		public AutosorterFilter GetTechType()
		{
			return filter;
		}

		public void Override(string text, bool category)
		{
			filter = null;
			this.text.text = text;
			SetBackgroundSprite(category);
			gameObject.SetActive(true);
		}

		public void SetFilter(AutosorterFilter value)
		{
			filter = value;
			if (filter != null)
			{
				// The filter text displayed on the lockers
				text.text = filter.GetString();

				SetBackgroundSprite(filter.IsCategory());
			}
			gameObject.SetActive(filter != null);
		}

		private void SetBackgroundSprite(bool category)
		{
			if (background != null)
			{
				var spriteName = category ? "MainMenuPressedSprite.png" : "MainMenuStandardSprite.png";
				background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath(spriteName), new RectOffset(Slice, Slice, Slice, Slice));
			}
		}

		public void Update()
		{
			if (background != null)
			{
				if (tabActive)
				{
					background.color = hover ? hoverColor : upColor;
				}
				else
				{
					background.color = hover ? inactiveHoverColor : inactiveColor;
				}
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			onClick.Invoke(filter);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hover = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			hover = false;
		}

		public void SetTabActive(bool active)
		{
			tabActive = active;
		}

		public static FilterPickerButton Create(Transform parent,
#if SUBNAUTICA
			Text textPrefab,
#elif BELOWZERO
			TextMeshProUGUI textPrefab,
#endif
			// The size of the picker buttons, only the height is useful, the width is overwritten later
			Action<AutosorterFilter> action, int width = 100, int height = 18)
		{
			var button = new GameObject("PickerButton", typeof(RectTransform)).AddComponent<FilterPickerButton>();
			button.transform.SetParent(parent, false);

			button.background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
			RectTransformExtensions.SetParams(button.background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(button.background.rectTransform, width * 10, height * 10);
			button.background.rectTransform.localScale = new Vector3(0.1f, 0.1f, 0.5f);
			button.background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("MainMenuStandardSprite.png"), new RectOffset(Slice, Slice, Slice, Slice));
			button.background.color = upColor;
			button.background.type = Image.Type.Sliced;
#if SUBNAUTICA
			button.text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
			button.text.alignment = TextAnchor.MiddleCenter;
#elif BELOWZERO
			button.text = new GameObject("TextMeshProUGUI", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
#endif
			RectTransformExtensions.SetParams(button.text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), button.transform);
			RectTransformExtensions.SetSize(button.text.rectTransform, width, height);
			button.text.color = new Color(1, 1, 1);
			button.text.font = textPrefab.font;
			button.text.fontSize = 10;
			button.onClick += action;
			button.text.alignment = TextAlignmentOptions.Left;
			// Set the left margin
			button.text.margin = new Vector4(5.0f, 0.0f);

			return button;
		}
	}
}