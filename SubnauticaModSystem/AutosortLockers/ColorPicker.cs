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
			foreach (var button in buttons)
			{
				button.toggled = false;
			}
			Close();
		}

		public override void Open()
		{
			base.Open();

			var index = 0;
			for (int i = 0; i < buttons.Count; ++i)
			{
				if (buttons[i].toggled)
				{
					index = i;
					break;
				}
			}

			int buttonPage = index / ButtonsPerPage;
			ShowPage(buttonPage);
		}
		
		/*_____________________________________________________________________________________________________*/

		public static ColorPicker Create(Transform parent, GameObject lockerPrefab = null)
		{
			var beaconColorPicker = new GameObject("ColorPicker", typeof(RectTransform)).AddComponent<ColorPicker>();
			// Used to calculate the size of the color picker background
			beaconColorPicker.ButtonSize = 15;
			beaconColorPicker.Spacing = 15;
			beaconColorPicker.ButtonsPerPage = 140;
			beaconColorPicker.ButtonsPerRow = 10;

			Picker.Create(parent, beaconColorPicker, Mod.colors.Count, lockerPrefab);
			return beaconColorPicker;
		}
	}
}