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
	public class AutosortTypePicker : MonoBehaviour
	{
		private int currentPage = 0;
		private bool mode = false;
		private List<TechType> availableTypes;

		[SerializeField]
		private AutosortTarget locker;
		[SerializeField]
		private PickerButton[] currentList = new PickerButton[AutosortTarget.MaxTypes];
		[SerializeField]
		private PickerButton[] availableList = new PickerButton[AutosortTarget.MaxTypes];
		[SerializeField]
		private Image background;
		[SerializeField]
		private Image[] underlines = new Image[2];
		[SerializeField]
		private PickerCloseButton closeButton;
		[SerializeField]
		private Text pageText;
		[SerializeField]
		private PickerPageButton prevPageButton;
		[SerializeField]
		private PickerPageButton nextPageButton;

		public void Initialize(AutosortTarget locker)
		{
			this.locker = locker;
			closeButton.target = locker;
			background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
			background.color = new Color(1, 1, 1);

			SetCurrentTypes(locker.GetTechTypes());
			UpdateAvailableTypes();
		}

		private void SetCurrentTypes(HashSet<TechType> types)
		{
			int i = 0;
			foreach (TechType tech in types)
			{
				currentList[i].SetTechType(tech);
				i++;
			}
			while (i < AutosortTarget.MaxTypes)
			{
				currentList[i].SetTechType(TechType.None);
				i++;
			}
		}

		private bool IsInCategoryMode()
		{
			return mode;
		}

		private bool IsInItemsMode()
		{
			return !mode;
		}

		private List<TechType> GetAvailableTypes()
		{
			if (IsInItemsMode())
			{
				return AutosorterCategoryData.IndividualItems;
			}
			else
			{
				return AutosorterCategoryData.IndividualItems;
			}
		}

		private void UpdateAvailableTypes()
		{
			availableTypes = GetAvailableTypes();
			int start = currentPage * AutosortTarget.MaxTypes;
			for (int i = 0; i < AutosortTarget.MaxTypes; ++i)
			{
				var techType = (start + i) >= availableTypes.Count ? TechType.None : availableTypes[start + i];
				availableList[i].SetTechType(techType);
			}
			pageText.text = string.Format("{0}/{1}", currentPage + 1, GetCurrentPageCount());
			prevPageButton.canChangePage = (currentPage > 0);
			nextPageButton.canChangePage = (currentPage + 1) < GetCurrentPageCount();
		}

		public void OnCurrentListItemClick(TechType techType)
		{
			if (techType == TechType.None)
			{
				return;
			}

			var allowedTypes = locker.GetTechTypes();
			allowedTypes.Remove(techType);
			locker.SetTechTypes(allowedTypes);

			SetCurrentTypes(allowedTypes);
		}

		public void OnAvailableListItemClick(TechType techType)
		{
			if (techType == TechType.None)
			{
				return;
			}

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

			SetCurrentTypes(allowedTypes);
		}

		internal void ChangePage(int pageOffset)
		{
			var pageCount = GetCurrentPageCount();
			currentPage = Mathf.Clamp(currentPage + pageOffset, 0, pageCount - 1);
			UpdateAvailableTypes();
		}

		private int GetCurrentPageCount()
		{
			return (int)Mathf.Ceil((float)availableTypes.Count / AutosortTarget.MaxTypes);
		}



		public static AutosortTypePicker Create(Transform parent, Text textPrefab)
		{
			var picker = LockerPrefabShared.CreateCanvas(parent).gameObject.AddComponent<AutosortTypePicker>();
			picker.GetComponent<Canvas>().sortingLayerID = 0;

			var t = picker.transform;
			t.localEulerAngles = new Vector3(0, 180, 0);
			t.localPosition = new Vector3(0, 0, 0.4f);

			picker.background = LockerPrefabShared.CreateBackground(picker.transform);
			picker.background.type = Image.Type.Simple;
			RectTransformExtensions.SetSize(picker.background.rectTransform, 240, 220);
			picker.background.color = new Color(1, 1, 1);

			int spacing = 20;
			int startY = 60;
			int x = 55;

			picker.underlines[0] = CreateUnderline(picker.background.transform, x);
			picker.underlines[1] = CreateUnderline(picker.background.transform, -x);

			var currentText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 12, "Current");
			currentText.rectTransform.anchoredPosition = new Vector2(-x, 90);

			var availableText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 12, "Items / Categories");
			availableText.rectTransform.anchoredPosition = new Vector2(x, 90);

			picker.pageText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 10, "1/X");
			picker.pageText.rectTransform.anchoredPosition = new Vector2(x, -80);

			picker.prevPageButton = AddPageButton(picker.background.transform, picker, -1, x - 20, -80);
			picker.nextPageButton = AddPageButton(picker.background.transform, picker, +1, x + 20, -80);

			picker.closeButton = AddCloseButton(picker.background.transform);

			for (int i = 0; i < AutosortTarget.MaxTypes; ++i)
			{
				picker.currentList[i] = CreatePickerButton(picker.background.transform, -x, startY - (i * spacing), textPrefab, picker.OnCurrentListItemClick);
				picker.availableList[i] = CreatePickerButton(picker.background.transform, x, startY - (i * spacing), textPrefab, picker.OnAvailableListItemClick);
			}

			// Create available list pagination

			return picker;
		}

		private static PickerPageButton AddPageButton(Transform parent, AutosortTypePicker target, int pageOffset, int x, int y)
		{
			var pageButton = LockerPrefabShared.CreateIcon(parent, Color.white, y);
			pageButton.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath(pageOffset < 0 ? "ArrowLeft.png" : "ArrowRight.png"));
			pageButton.rectTransform.anchoredPosition = new Vector2(x, y);
			RectTransformExtensions.SetSize(pageButton.rectTransform, 44 / 4.0f, 73 / 4.0f);

			var controller = pageButton.gameObject.AddComponent<PickerPageButton>();
			controller.target = target;
			controller.pageOffset = pageOffset;

			return controller;
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

		public static PickerButton CreatePickerButton(Transform parent, int x, int y, Text textPrefab, Action<TechType> action)
		{
			var button = PickerButton.Create(parent, TechType.None, textPrefab, action);

			var rt = button.transform as RectTransform;
			rt.anchoredPosition = new Vector2(x, y);

			return button;
		}
	}
}
