using Common.Utility;
using UnityEngine;

namespace AutosortLockers
{
	class ColorPicker : Picker
	{
		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void Initialize(Color initialColor)
		{
			base.Initialize();

			var sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"), new Vector2(0.5f, 0.5f));
			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				var color = Mod.colors[i];
				button.Initialize(i, color, color == initialColor, sprite);
			}

			onSelect = OnSelect;
		}

		public void OnSelect(int index)
		{
			Close();
		}

		public override void Open()
		{
			base.Open();
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static ColorPicker Create(Transform parent)
		{
			var beaconColorPicker = new GameObject("ColorPicker", typeof(RectTransform)).AddComponent<ColorPicker>();

			beaconColorPicker.ButtonSize = 15;
			beaconColorPicker.Spacing = 15;
			beaconColorPicker.ButtonsPerPage = 72;
			beaconColorPicker.ButtonsPerRow = 8;

			Picker.Create(parent, beaconColorPicker, Mod.colors.Count);

			return beaconColorPicker;
		}
	}
}
