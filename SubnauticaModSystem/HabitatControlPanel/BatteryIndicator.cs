using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class BatteryIndicator : MonoBehaviour
	{
		private RectTransform rectTransform;
		private Pickupable item;
		private IBattery battery;

		[SerializeField]
		private Text text;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void Initialize(Text textPrefab)
		{
			text = GameObject.Instantiate(textPrefab);
			text.gameObject.name = "Text";
			text.rectTransform.SetParent(transform, false);
			RectTransformExtensions.SetSize(text.rectTransform, 100, 100);
			text.text = "Battery Indicator";

			SetBattery(null);
		}

		public void SetBattery(Pickupable item)
		{
			this.item = item;
			this.battery = item?.GetComponent<IBattery>();
		}

		private void Update()
		{
			if (item == null)
			{
				text.text = "No Power Cell";
			}
			else
			{
				var name = Language.main.Get(item.GetTechType());
				var charge = "[" + Mathf.RoundToInt(battery.charge) + "/" + Mathf.RoundToInt(battery.capacity) + "]";
				text.text = name + "\n" + charge;
			}
		}
	}
}
