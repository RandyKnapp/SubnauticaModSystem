using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class BeaconIconPicker : Picker
	{
		private HabitatControlPanel target;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void Initialize(HabitatControlPanel target, PingType initialType)
		{
			base.Initialize();

			this.target = target;
			
			var names = Enum.GetNames(typeof(PingType));
			var values = (PingType[])Enum.GetValues(typeof(PingType));
			var color = PingManager.colorOptions[target.BeaconColorIndex];
			int initialPage = 0;
			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				var name = names[i + 1];
				var value = values[i + 1];
				var sprite = SpriteManager.Get(SpriteManager.Group.Pings, name);
				button.Initialize((int)value, color, value == initialType, sprite);
				
				if (value == initialType)
				{
					initialPage = i / ButtonsPerPage;
				}
			}

			onSelect = OnSelect;
			ShowPage(initialPage);
		}

		public void OnSelect(int index)
		{
			target.BeaconPingType = (PingType)index;
			target.CloseSubmenu();
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static BeaconIconPicker Create(Transform parent)
		{
			var beaconIconPicker = new GameObject("BeaconIconPicker", typeof(RectTransform)).AddComponent<BeaconIconPicker>();

			var count = Enum.GetNames(typeof(PingType)).Length - 1;
			Picker.Create(parent, beaconIconPicker, count);

			return beaconIconPicker;
		}
	}
}
