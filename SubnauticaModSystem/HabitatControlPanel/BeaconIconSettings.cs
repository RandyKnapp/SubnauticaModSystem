using System;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
    class BeaconIconSettings : MonoBehaviour
	{
		private bool hasPower = false;

		public RectTransform rectTransform;
		public Action onClick = delegate { };

		[SerializeField]
		private HabitatControlPanel target;
		[SerializeField]
		private uGUI_ColoredIconButton activeButton;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		private void Initialize(HabitatControlPanel controlPanel, Text textPrefab)
		{
			target = controlPanel;

			activeButton = uGUI_ColoredIconButton.Create(transform, HabitatControlPanel.ScreenContentColor, textPrefab, "Beacon Icon", 100, 15);
			activeButton.text.supportRichText = true;
		}

		internal void SetInitialValue(PingType type, int colorIndex)
		{
			SetValue(type, colorIndex);
			activeButton.onClick += OnClick;
		}

		internal void SetValue(PingType type, int colorIndex)
		{
			var color = PingManager.colorOptions[colorIndex];

			var spriteName = Enum.GetName(typeof(PingType), type);
			var sprite = SpriteManager.Get(SpriteManager.Group.Pings, spriteName);
			activeButton.Initialize(sprite, color);
		}

		private void OnClick()
		{
			onClick();
		}

		private void Update()
		{
			if (Mod.config.RequireBatteryToUse)
			{
				hasPower = target != null && target.GetPower() > 0;
				activeButton.isEnabled = hasPower;
			}
		}


		///////////////////////////////////////////////////////////////////////////////////////////
		public static BeaconIconSettings Create(HabitatControlPanel controlPanel, Transform parent)
		{
			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
			textPrefab.fontSize = 12;
			textPrefab.color = HabitatControlPanel.ScreenContentColor;

			var beaconController = new GameObject("BeaconIconSettings", typeof(RectTransform)).AddComponent<BeaconIconSettings>();
			var rt = beaconController.gameObject.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			beaconController.Initialize(controlPanel, textPrefab);

			return beaconController;
		}
	}
}
