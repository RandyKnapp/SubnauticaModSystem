using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CustomBeacons
{
	class BeaconIconPicker : Picker
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
			
			var names = Enum.GetNames(typeof(PingType));
			var values = (PingType[])Enum.GetValues(typeof(PingType));
			var color = CustomPings.GetColor(target.colorIndex);
			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				var name = names[i + 1];
				var value = values[i + 1];
				var sprite = SpriteManager.Get(SpriteManager.Group.Pings, name);
				button.Initialize(i, color, value == target.pingType, sprite);
			}

			onSelect = OnSelect;
			int initialPage = GetPageForType(target.pingType);
			ShowPage(initialPage);
		}

		public int GetPageForType(PingType type)
		{
			var values = (PingType[])Enum.GetValues(typeof(PingType));
			for (int i = 0; i < buttons.Count; ++i)
			{
				var value = values[i + 1];
				if (value == target.pingType)
				{
					return i / ButtonsPerPage;
				}
			}
			return 0;
		}

		public void OnSelect(int index)
		{
			var values = (PingType[])Enum.GetValues(typeof(PingType));
			var value = values[index + 1];
			target.pingType = value;
			PingManager.NotifyVisible(target);
			Close();
		}

		public override void Open()
		{
			base.Open();
			int initialPage = GetPageForType(target.pingType);
			ShowPage(initialPage);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static BeaconIconPicker Create(Transform parent)
		{
			var beaconIconPicker = new GameObject("BeaconIconPicker", typeof(RectTransform)).AddComponent<BeaconIconPicker>();

			beaconIconPicker.ButtonSize = 50;
			beaconIconPicker.Spacing = 50;
			beaconIconPicker.ButtonsPerPage = 20;
			beaconIconPicker.ButtonsPerRow = 10;

			var count = Enum.GetNames(typeof(PingType)).Length - 1;
			Picker.Create(parent, beaconIconPicker, count);

			return beaconIconPicker;
		}
	}
}
