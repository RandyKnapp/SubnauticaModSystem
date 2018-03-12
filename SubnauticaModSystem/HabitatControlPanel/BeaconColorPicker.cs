using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class BeaconColorPicker : Picker
	{
		private HabitatControlPanel target;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void Initialize(HabitatControlPanel target, int initialIndex)
		{
			base.Initialize();

			this.target = target;

			var sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"), new Vector2(0.5f, 0.5f));
			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				button.Initialize(i, PingManager.colorOptions[i], i == initialIndex, sprite);
			}

			onSelect += OnSelect;
		}

		public void OnSelect(int index)
		{
			target.BeaconColorIndex = index;
			target.CloseSubmenu();
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static BeaconColorPicker Create(Transform parent)
		{
			var beaconColorPicker = new GameObject("BeaconColorPicker", typeof(RectTransform)).AddComponent<BeaconColorPicker>();

			Picker.Create(parent, beaconColorPicker, PingManager.colorOptions.Length);

			return beaconColorPicker;
		}
	}
}
