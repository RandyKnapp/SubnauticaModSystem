using Common.Mod;
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
		public static readonly Color ColorEmpty = new Color(1f, 0f, 0f, 1f);
		public static readonly Color ColorHalf = new Color(1f, 1f, 0f, 1f);
		public static readonly Color ColorFull = new Color(0f, 1f, 0f, 1f);

		public RectTransform rectTransform;
		private Pickupable item;
		private IBattery battery;

		[SerializeField]
		private Text text;
		//[SerializeField]
		//private Text text2;
		[SerializeField]
		private Image bar;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void Initialize(Text textPrefab, GameObject batteryUI)
		{
			text = GameObject.Instantiate(textPrefab);
			text.gameObject.name = "PercentText";
			text.rectTransform.SetParent(transform, false);
			RectTransformExtensions.SetSize(text.rectTransform, 100, 100);

			/*text2 = GameObject.Instantiate(textPrefab);
			text2.gameObject.name = "CapacityText";
			text2.rectTransform.SetParent(transform, false);
			RectTransformExtensions.SetSize(text2.rectTransform, 100, 100);
			text2.rectTransform.anchoredPosition = new Vector2(0, -30);
			text2.fontSize = 10;*/

			float scale = 1 / 4.0f;

			var rt = batteryUI.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
			RectTransformExtensions.SetSize(rt, 136 * scale, 256 * scale);
			rt.anchoredPosition = new Vector2(0, 0);
			rt.localEulerAngles = new Vector3(0, 0, -90);
			bar = batteryUI.transform.Find("Bar").GetComponent<Image>();
			Destroy(batteryUI.transform.Find("Text").gameObject);
			Destroy(batteryUI.transform.Find("Label").gameObject);

			bar.gameObject.SetActive(true);
			bar.transform.localScale = new Vector3(scale, scale, 1);

			text.transform.SetAsLastSibling();

			SetBattery(null);
		}

		public void SetBattery(Pickupable item)
		{
			this.item = item;
			this.battery = item?.GetComponent<IBattery>();
		}

		private void Update()
		{
			Material material = bar.material;
			if (item == null)
			{
				text.text = Language.main.Get("ChargerSlotEmpty");
				//text2.text = "";

				material.SetColor(ShaderPropertyID._Color, ColorEmpty);
				material.SetFloat(ShaderPropertyID._Amount, 0f);
			}
			else
			{
				var percent = battery.charge / battery.capacity;
				text.text = string.Format("{0:P0}", percent).Replace(" %", "%");

				//text2.text = "[" + Mathf.RoundToInt(battery.charge) + "/" + Mathf.RoundToInt(battery.capacity) + "]";

				Color value = (percent >= 0.5f) ? Color.Lerp(ColorHalf, ColorFull, 2f * percent - 1f) : Color.Lerp(ColorEmpty, ColorHalf, 2f * percent);
				material.SetColor(ShaderPropertyID._Color, value);
				material.SetFloat(ShaderPropertyID._Amount, percent);
			}
		}



		///////////////////////////////////////////////////////////////////////////////////////////
		public static BatteryIndicator Create(HabitatControlPanel controlPanel, Transform parent)
		{
			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = lockerPrefab.GetComponentInChildren<Text>();
			textPrefab.fontSize = 12;
			textPrefab.color = HabitatControlPanel.ScreenContentColor;

			var chargerPrefab = Resources.Load<GameObject>("Submarine/Build/PowerCellCharger");
			var charger = chargerPrefab.GetComponent<PowerCellCharger>();
			var batteryUi = GameObject.Instantiate(charger.uiPowered.transform.Find("Battery1").gameObject);

			var batteryIndicator = new GameObject("BatteryIndicator", typeof(RectTransform)).AddComponent<BatteryIndicator>();
			var rt = batteryIndicator.gameObject.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, 100, 100);
			rt.anchoredPosition = new Vector2(0, 0);
			batteryIndicator.Initialize(textPrefab, batteryUi);

			return batteryIndicator;
		}
	}
}
