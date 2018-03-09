using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class BeaconColorSettings : MonoBehaviour
	{
		private bool hasPower = false;

		public RectTransform rectTransform;
		public Action onClick = delegate { };

		[SerializeField]
		private HabitatControlPanel target;
		[SerializeField]
		private ColoredIconButton activeButton;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		private void Initialize(HabitatControlPanel controlPanel, Text textPrefab)
		{
			target = controlPanel;

			activeButton = ColoredIconButton.Create(transform, HabitatControlPanel.ScreenContentColor, textPrefab, "Beacon Color", 100, 15);
			activeButton.text.supportRichText = true;
			UpdateText();
		}

		internal void SetInitialValue(int colorIndex)
		{
			SetColor(colorIndex);
			activeButton.onClick += OnClick;
			UpdateText();
		}

		internal void SetColor(int colorIndex)
		{
			var color = PingManager.colorOptions[colorIndex];
			activeButton.Initialize("Circle.png", color);
		}

		private void OnClick()
		{
			Logger.Log("BeaconColorSettings OnClick");
			onClick();
		}

		private void Update()
		{
			hasPower = target != null && target.GetPower() > 0;
			activeButton.isEnabled = hasPower;
		}

		private void UpdateText()
		{
		}


		///////////////////////////////////////////////////////////////////////////////////////////
		public static BeaconColorSettings Create(HabitatControlPanel controlPanel, Transform parent)
		{
			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = lockerPrefab.GetComponentInChildren<Text>();
			textPrefab.fontSize = 12;
			textPrefab.color = HabitatControlPanel.ScreenContentColor;

			var beaconController = new GameObject("BeaconColorSettings", typeof(RectTransform)).AddComponent<BeaconColorSettings>();
			var rt = beaconController.gameObject.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			beaconController.Initialize(controlPanel, textPrefab);

			return beaconController;
		}
	}
}
