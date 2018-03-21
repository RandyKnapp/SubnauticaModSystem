using Common.Mod;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AutosortLockers
{
	class CustomizeScreen : MonoBehaviour
	{
		public static readonly Color ScreenContentColor = new Color32(188, 254, 254, 255);

		public SaveDataEntry target;
		public RectTransform rectTransform;
		public Action onModified = delegate { };

		[SerializeField]
		private Image background;
		[SerializeField]
		private Text labelLabel;
		[SerializeField]
		private LabelController label;
		[SerializeField]
		private ConfigureButton exitButton;
		[SerializeField]
		private ColorSetting labelColorSetting;
		[SerializeField]
		private ColorSetting iconColorSetting;
		[SerializeField]
		private ColorSetting textColorSetting;
		[SerializeField]
		private ColorSetting buttonsColorSetting;
		[SerializeField]
		private ColorPicker colorPicker;
		[SerializeField]
		private ColorSetting lockerColorSetting;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		private void OnDisable()
		{
			colorPicker.Close();
		}

		internal void Initialize(SaveDataEntry saveData)
		{
			target = saveData;

			exitButton.GetComponentInChildren<Image>().sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Close.png"));

			label.text.color = saveData.LabelColor.ToColor();

			label.onModified += OnLabelChanged;
			exitButton.onClick += OnExitButtonClicked;

			labelColorSetting.SetInitialValue(saveData.LabelColor.ToColor());
			labelColorSetting.onClick += OnLabelColorSettingClicked;

			iconColorSetting.SetInitialValue(saveData.IconColor.ToColor());
			iconColorSetting.onClick += OnIconColorSettingClicked;

			textColorSetting.SetInitialValue(saveData.OtherTextColor.ToColor());
			textColorSetting.onClick += OnTextColorSettingClicked;

			buttonsColorSetting.SetInitialValue(saveData.ButtonsColor.ToColor());
			buttonsColorSetting.onClick += OnButtonsColorSettingClicked;

			lockerColorSetting.SetInitialValue(saveData.LockerColor.ToColor());
			lockerColorSetting.onClick += OnLockerColorSettingClicked;
		}

		private void SetColor(SaveDataEntry saveData)
		{
			label.text.color = saveData.LabelColor.ToColor();
			labelColorSetting.SetColor(saveData.LabelColor.ToColor());
			iconColorSetting.SetColor(saveData.IconColor.ToColor());
			textColorSetting.SetColor(saveData.OtherTextColor.ToColor());
			buttonsColorSetting.SetColor(saveData.ButtonsColor.ToColor());
			lockerColorSetting.SetColor(saveData.LockerColor.ToColor());
		}

		private void OnLabelChanged(string newLabel)
		{
			onModified();
		}

		private void OnExitButtonClicked()
		{
			gameObject.SetActive(false);
		}

		private void OnLabelColorSettingClicked()
		{
			colorPicker.Initialize(target.LabelColor.ToColor());
			colorPicker.Open();
			colorPicker.onSelect += OnLabelColorPicked;
		}

		private void OnLabelColorPicked(int index)
		{
			target.LabelColor = Mod.colors[index];
			onModified();
			colorPicker.onSelect -= OnLabelColorPicked;
			SetColor(target);
		}

		private void OnIconColorSettingClicked()
		{
			colorPicker.Initialize(target.IconColor.ToColor());
			colorPicker.Open();
			colorPicker.onSelect += OnIconColorPicked;
		}

		private void OnIconColorPicked(int index)
		{
			target.IconColor = Mod.colors[index];
			onModified();
			colorPicker.onSelect -= OnIconColorPicked;
			SetColor(target);
		}

		private void OnTextColorSettingClicked()
		{
			colorPicker.Initialize(target.OtherTextColor.ToColor());
			colorPicker.Open();
			colorPicker.onSelect += OnTextColorPicked;
		}

		private void OnTextColorPicked(int index)
		{
			target.OtherTextColor = Mod.colors[index];
			onModified();
			colorPicker.onSelect -= OnTextColorPicked;
			SetColor(target);
		}

		private void OnButtonsColorSettingClicked()
		{
			colorPicker.Initialize(target.ButtonsColor.ToColor());
			colorPicker.Open();
			colorPicker.onSelect += OnButtonsColorPicked;
		}

		private void OnButtonsColorPicked(int index)
		{
			target.ButtonsColor = Mod.colors[index];
			onModified();
			colorPicker.onSelect -= OnButtonsColorPicked;
			SetColor(target);
		}

		
		private void OnLockerColorSettingClicked()
		{
			colorPicker.Initialize(target.LockerColor.ToColor());
			colorPicker.Open();
			colorPicker.onSelect += OnLockerColorPicked;
		}

		private void OnLockerColorPicked(int index)
		{
			target.LockerColor = Mod.colors[index];
			onModified();
			colorPicker.onSelect -= OnLockerColorPicked;
			SetColor(target);
		}



		//////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static CustomizeScreen Create(Transform parent, SaveDataEntry data)
		{
			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
			textPrefab.fontSize = 12;
			textPrefab.color = CustomizeScreen.ScreenContentColor;

			var screen = new GameObject("CustomizeScreen", typeof(RectTransform)).AddComponent<CustomizeScreen>();
			RectTransformExtensions.SetParams(screen.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(screen.rectTransform, 114, 241);

			screen.background = new GameObject("Background").AddComponent<Image>();
			screen.background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("CustomizeScreen.png"));
			RectTransformExtensions.SetParams(screen.background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), screen.transform);
			RectTransformExtensions.SetSize(screen.background.rectTransform, 114, 241);

			screen.labelLabel = LockerPrefabShared.CreateText(screen.background.transform, textPrefab, ScreenContentColor, 100, 9, "Label:");
			RectTransformExtensions.SetSize(screen.labelLabel.rectTransform, 90, 40);
			screen.labelLabel.alignment = TextAnchor.MiddleLeft;

			screen.label = LabelController.Create(data, screen.background.transform);
			screen.label.rectTransform.anchoredPosition = new Vector2(0, 80);

			screen.exitButton = ConfigureButton.Create(screen.background.transform, Color.white, 40);

			var startX = 0;
			var startY = 30;
			screen.labelColorSetting = ColorSetting.Create(screen.background.transform, "Label Color");
			screen.labelColorSetting.rectTransform.anchoredPosition = new Vector2(startX, startY);

			screen.iconColorSetting = ColorSetting.Create(screen.background.transform, "Icon Color");
			screen.iconColorSetting.rectTransform.anchoredPosition = new Vector2(startX, startY - 19);

			screen.textColorSetting = ColorSetting.Create(screen.background.transform, "Filters Color");
			screen.textColorSetting.rectTransform.anchoredPosition = new Vector2(startX, startY - (19 * 2));

			screen.buttonsColorSetting = ColorSetting.Create(screen.background.transform, "Misc Color");
			screen.buttonsColorSetting.rectTransform.anchoredPosition = new Vector2(startX, startY - (19 * 3));

			screen.lockerColorSetting = ColorSetting.Create(screen.background.transform, "Locker Color");
			screen.lockerColorSetting.rectTransform.anchoredPosition = new Vector2(startX, startY - (19 * 4));

			screen.colorPicker = ColorPicker.Create(screen.background.transform);
			screen.colorPicker.gameObject.SetActive(false);
			screen.colorPicker.rectTransform.anchoredPosition = new Vector2(0, 30);

			return screen;
		}
	}
}
