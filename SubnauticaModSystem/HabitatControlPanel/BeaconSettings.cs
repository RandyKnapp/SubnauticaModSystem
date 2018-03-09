using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class BeaconSettings : MonoBehaviour
	{
		private bool hasPower = false;
		public RectTransform rectTransform;

		[SerializeField]
		private HabitatControlPanel target;
		[SerializeField]
		private CheckboxButton activeButton;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		private void Initialize(HabitatControlPanel controlPanel, Text textPrefab)
		{
			target = controlPanel;

			activeButton = CheckboxButton.CreateCheckbox(transform, HabitatControlPanel.ScreenContentColor, textPrefab, "", 150);
			activeButton.text.supportRichText = true;
			UpdateText();
		}

		internal void SetInitialValue(bool enabled)
		{
			activeButton.Initialize();
			activeButton.toggled = enabled;
			activeButton.onToggled += OnBeaconToggled;
			UpdateText();
		}

		private void OnBeaconToggled(bool toggled)
		{
			target.BeaconEnabled = toggled;
			UpdateText();
		}

		private void Update()
		{
			var prevHasPower = hasPower;
			hasPower = target != null && target.GetPower() > 0;
			if (prevHasPower != hasPower)
			{
				UpdateText();
			}
			activeButton.isEnabled = hasPower;
		}

		private void UpdateText()
		{
			var text = !hasPower ? "UNPOWERED" : (activeButton.toggled ? "ON" : "OFF");
			var color = (activeButton.toggled && hasPower) ? "lime" : "red";
			activeButton.text.text = string.Format("Beacon [<color={1}>{0}</color>]", text, color);
			Logger.Log("UpdateText: t=" + text + " c=" + color + " full=" + string.Format("Beacon [<color={1}>{0}</color>]", text, color));
		}


		///////////////////////////////////////////////////////////////////////////////////////////
		public static BeaconSettings Create(HabitatControlPanel controlPanel, Transform parent)
		{
			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = lockerPrefab.GetComponentInChildren<Text>();
			textPrefab.fontSize = 12;
			textPrefab.color = HabitatControlPanel.ScreenContentColor;

			var beaconController = new GameObject("BeaconController", typeof(RectTransform)).AddComponent<BeaconSettings>();
			var rt = beaconController.gameObject.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			beaconController.Initialize(controlPanel, textPrefab);

			return beaconController;
		}
	}
}
