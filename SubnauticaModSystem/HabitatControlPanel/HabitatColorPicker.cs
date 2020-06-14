using Common.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HabitatControlPanel
{
    class HabitatColorPicker : Picker
	{
		private static readonly List<Color> Colors = new List<Color>() {
			Color.white, Color.red, new Color32(255, 165, 0, 255), Color.yellow, Color.green,
			Color.cyan, Color.blue, new Color32(131, 0, 255, 255), Color.magenta, Color.gray
		};

		private HabitatControlPanel target;

		public Action<Color> onColorSelect = delegate { };

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void Initialize(HabitatControlPanel target, Color initialColor)
		{
			base.Initialize();

			this.target = target;

			var sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"), new Vector2(0.5f, 0.5f));
			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				button.Initialize(i, Colors[i], Colors[i] == initialColor, sprite);
			}

			onSelect = OnSelect;
		}

		public void OnSelect(int index)
		{
			onColorSelect(Colors[index]);
			target.CloseSubmenu();
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static HabitatColorPicker Create(Transform parent)
		{
			var habitatColorPicker = new GameObject("HabitatColorPicker", typeof(RectTransform)).AddComponent<HabitatColorPicker>();

			Picker.Create(parent, habitatColorPicker, Colors.Count);

			return habitatColorPicker;
		}
	}
}
