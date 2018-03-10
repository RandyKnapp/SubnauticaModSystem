using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class HabitatColorPicker : MonoBehaviour
	{
		private const float ButtonSize = 30;
		private const int ButtonsPerRow = 5;
		private const float Spacing = 35;
		private const float StartX = -(Spacing * 2);
		private const float StartY = (Spacing / 2);

		private static readonly List<Color> Colors = new List<Color>() {
			Color.white, Color.red, new Color32(255, 165, 0, 255), Color.yellow, Color.green,
			Color.cyan, Color.blue, new Color32(131, 0, 255, 255), Color.magenta, Color.gray
		};

		private HabitatControlPanel target;

		public RectTransform rectTransform;

		[SerializeField]
		private Image background;
		[SerializeField]
		private List<ColorPickerButton> buttons = new List<ColorPickerButton>();

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void Initialize(HabitatControlPanel target, Color initialColor)
		{
			this.target = target;

			if (background.sprite == null)
			{
				background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("SmallBackground.png"));
			}

			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				int row = i / ButtonsPerRow;
				int col = i % ButtonsPerRow;
				button.Initialize(i, Colors[i], Colors[i] == initialColor);
				button.rectTransform.anchoredPosition = new Vector2(StartX + (Spacing * col), StartY - (row * Spacing));
				button.onClick += OnClick;
			}
		}

		public void OnClick(int index)
		{
			target.ExteriorColor = Colors[index];
			target.CloseSubmenu();

			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				button.toggled = index == i;
				button.OnPointerExit(null);
				button.onClick -= OnClick;
				button.Update();
			}
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static HabitatColorPicker Create(HabitatControlPanel target, Transform parent)
		{
			var habitatColorPicker = new GameObject("HabitatColorPicker", typeof(RectTransform)).AddComponent<HabitatColorPicker>();
			var rt = habitatColorPicker.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, 1000 / 5.0f, 400 / 5.0f);

			habitatColorPicker.background = habitatColorPicker.gameObject.AddComponent<Image>();

			for (int i = 0; i < Colors.Count; ++i)
			{
				var color = Colors[i];
				var colorButton = ColorPickerButton.Create(habitatColorPicker.transform, ButtonSize, ButtonSize * 0.7f);
				habitatColorPicker.buttons.Add(colorButton);
			}

			return habitatColorPicker;
		}
	}
}
