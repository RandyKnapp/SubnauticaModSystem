using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class BeaconColorPicker : MonoBehaviour
	{
		private const float Spacing = 35;
		private const float StartX = -(Spacing * 2);

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

		public void Initialize(HabitatControlPanel target, int initialIndex)
		{
			this.target = target;

			if (background.sprite == null)
			{
				background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("SmallBackground.png"));
			}

			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				button.Initialize(i, PingManager.colorOptions[i], i == initialIndex);
				button.rectTransform.anchoredPosition = new Vector2(StartX + (Spacing * i), 0);
				button.onClick += OnClick;
			}
		}

		public void OnClick(int index)
		{
			target.BeaconColorIndex = index;
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
		public static BeaconColorPicker Create(HabitatControlPanel target, Transform parent)
		{
			var beaconColorPicker = new GameObject("BeaconColorPicker", typeof(RectTransform)).AddComponent<BeaconColorPicker>();
			var rt = beaconColorPicker.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, 1000 / 5.0f, 400 / 5.0f);

			beaconColorPicker.background = beaconColorPicker.gameObject.AddComponent<Image>();

			for (int i = 0; i < PingManager.colorOptions.Length; ++i)
			{
				var color = PingManager.colorOptions[i];
				var colorButton = ColorPickerButton.Create(beaconColorPicker.transform, 35, 25);
				beaconColorPicker.buttons.Add(colorButton);
			}

			return beaconColorPicker;
		}
	}
}
