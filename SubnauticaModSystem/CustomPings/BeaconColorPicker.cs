using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CustomBeacons
{
	class BeaconColorPicker : Picker
	{
		private PingInstance target;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void Initialize(PingInstance target)
		{
			base.Initialize();

			this.target = target;

			var sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"), new Vector2(0.5f, 0.5f));
			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				button.Initialize(i, CustomPings.GetColor(i), i == target.colorIndex, sprite);
			}

			onSelect = OnSelect;
		}

		public void OnSelect(int index)
		{
			Logger.Log("OnSelect: " + index);
			target.SetColor(index);
			Logger.Log("    result=" + target.colorIndex);
			Close();
		}

		public override void Open()
		{
			base.Open();
			int buttonPage = target.colorIndex / ButtonsPerPage;
			ShowPage(buttonPage);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static BeaconColorPicker Create(Transform parent)
		{
			var beaconColorPicker = new GameObject("BeaconColorPicker", typeof(RectTransform)).AddComponent<BeaconColorPicker>();

			beaconColorPicker.ButtonSize = 50;
			beaconColorPicker.Spacing = 50;
			beaconColorPicker.ButtonsPerPage = 20;
			beaconColorPicker.ButtonsPerRow = 10;

			Picker.Create(parent, beaconColorPicker, PingManager.colorOptions.Length);

			return beaconColorPicker;
		}
	}
}
