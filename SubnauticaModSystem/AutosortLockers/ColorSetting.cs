using System;
using UnityEngine;
#if SUBNAUTICA
using UnityEngine.UI;
#elif BELOWZERO
using TMPro;
#endif

namespace AutosortLockers
{
	class ColorSetting : MonoBehaviour
	{
		public RectTransform rectTransform;
		public Action onClick = delegate { };

		[SerializeField]
		private ColoredIconButton activeButton;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}
#if SUBNAUTICA
		private void Initialize(Text textPrefab, string label)
		{
			activeButton = ColoredIconButton.Create(transform, CustomizeScreen.ScreenContentColor, textPrefab, label, 100, 15);
			activeButton.text.supportRichText = true;
		}
#elif BELOWZERO
		private void Initialize(TextMeshProUGUI textPrefab, string label)
		{
			activeButton = ColoredIconButton.Create(transform, CustomizeScreen.ScreenContentColor, textPrefab, label, 100, 15);
		}
#endif

		internal void SetInitialValue(Color initialColor)
		{
			SetColor(initialColor);
			activeButton.onClick += OnClick;
		}

		internal void SetColor(Color initialColor)
		{
			activeButton.Initialize("Circle.png", initialColor);
		}

		private void OnClick()
		{
			onClick();
		}

		/*_____________________________________________________________________________________________________*/

		public static ColorSetting Create(Transform parent, string label, GameObject lockerPrefab = null)
		{
#if SUBNAUTICA
			lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
#elif BELOWZERO
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<TextMeshProUGUI>());
#endif
			
			var beaconController = new GameObject("ColorSettings", typeof(RectTransform)).AddComponent<ColorSetting>();
			var rt = beaconController.gameObject.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			beaconController.Initialize(textPrefab, label);

			return beaconController;
		}
	}
}