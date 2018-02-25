using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class AutosortTypePicker : MonoBehaviour
	{
		[SerializeField]
		private AutosortTarget locker;
		[SerializeField]
		private VerticalLayoutGroup currentList;
		[SerializeField]
		private VerticalLayoutGroup availableList;
		[SerializeField]
		private Image background;
		[SerializeField]
		private Image[] underlines = new Image[2];
		[SerializeField]
		private PickerCloseButton closeButton;

		public void Initialize(AutosortTarget locker)
		{
			this.locker = locker;
			closeButton.target = locker;
			background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
		}

		public void OnCurrentListItemClick(TechType techType)
		{
			var allowedTypes = locker.GetTechTypes();
			allowedTypes.Remove(techType);
			locker.SetTechTypes(allowedTypes);
		}

		public void OnAvailableListItemClick(TechType techType)
		{
			var allowedTypes = locker.GetTechTypes();
			if (allowedTypes.Count >= AutosortTarget.MaxTypes)
			{
				return;
			}
			if (allowedTypes.Contains(techType))
			{
				return;
			}

			allowedTypes.Add(techType);
			locker.SetTechTypes(allowedTypes);
		}


		public static AutosortTypePicker Create(Transform parent, Text textPrefab)
		{
			var picker = LockerPrefabShared.CreateCanvas(parent).gameObject.AddComponent<AutosortTypePicker>();
			picker.GetComponent<Canvas>().sortingLayerID = 0;

			var t = picker.transform;
			t.localEulerAngles = new Vector3(0, 180, 0);
			t.localPosition = new Vector3(0, 0, 0.4f);

			picker.background = LockerPrefabShared.CreateBackground(picker.transform);
			RectTransformExtensions.SetSize(picker.background.rectTransform, 240, 220);
			picker.background.color = new Color(1, 1, 1);

			picker.underlines[0] = CreateUnderline(picker.background.transform, 60);
			picker.underlines[1] = CreateUnderline(picker.background.transform, -60);

			var currentText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 14, "Current");
			currentText.rectTransform.anchoredPosition = new Vector2(-60, 90);

			var availableText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 14, "Available");
			availableText.rectTransform.anchoredPosition = new Vector2(60, 90);

			picker.closeButton = AddCloseButton(picker.background.transform);

			// Create current list

			// Create Available list

			// Create available list pagination

			return picker;
		}

		private static Image CreateUnderline(Transform parent, int x)
		{
			var underline = new GameObject("Underline", typeof(RectTransform)).AddComponent<Image>();
			RectTransformExtensions.SetParams(underline.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(underline.rectTransform, 272 / 3, 78 / 3);
			underline.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("TitleUnderline.png"));
			underline.rectTransform.anchoredPosition = new Vector2(x, 90);

			return underline;
		}

		public static PickerCloseButton AddCloseButton(Transform parent)
		{
			var closeImage = new GameObject("CloseButton", typeof(RectTransform)).AddComponent<Image>();
			RectTransformExtensions.SetParams(closeImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(closeImage.rectTransform, 20, 20);
			closeImage.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Close.png"));
			closeImage.rectTransform.anchoredPosition = new Vector2(0, -90);

			var closeButton = closeImage.gameObject.AddComponent<PickerCloseButton>();

			return closeButton;
		}
	}
}
