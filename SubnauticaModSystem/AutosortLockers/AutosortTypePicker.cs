using Common.Mod;
using Common.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class AutosortTypePicker : MonoBehaviour
	{
		private enum Mode { Categories, Items }

		private Mode currentMode = Mode.Categories;
		private int currentPageCategories = 0;
		private int currentPageItems = 0;
		private List<AutosorterFilter> availableTypes;

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
		[SerializeField]
		private PickerButton categoriesTabButton;
		[SerializeField]
		private PickerButton itemsTabButton;

		public void Initialize(AutosortTarget locker)
		{
			this.locker = locker;
			closeButton.target = locker;
			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Background.png"));
			background.color = new Color(1, 1, 1);

			RefreshCurrentFilters();
			UpdateAvailableTypes();
		}

		private void RefreshCurrentFilters()
		{
			List<AutosorterFilter> filters = locker.GetCurrentFilters();
			int i = 0;
			foreach (var filter in filters)
			{
				currentList[i].SetFilter(filter);
				i++;
			}
			while (i < AutosortTarget.MaxTypes)
			{
				currentList[i].SetFilter(null);
				i++;
			}
		}

		private List<AutosorterFilter> GetAvailableTypes()
		{
			if (currentMode == Mode.Categories)
			{
				return AutosorterList.GetEntries().Where((e) => e.IsCategory()).ToList();
			}
			else
			{
				return AutosorterList.GetEntries().Where((e) => !e.IsCategory()).ToList();
			}
		}

		private void UpdateAvailableTypes()
		{
			availableTypes = GetAvailableTypes();
			int start = GetCurrentPage() * AutosortTarget.MaxTypes;
			for (int i = 0; i < AutosortTarget.MaxTypes; ++i)
			{
				var filter = (start + i) >= availableTypes.Count ? null : availableTypes[start + i];
				availableList[i].SetFilter(filter);
			}
			pageText.text = string.Format("{0}/{1}", GetCurrentPage() + 1, GetCurrentPageCount());
			prevPageButton.canChangePage = (GetCurrentPage() > 0);
			nextPageButton.canChangePage = (GetCurrentPage() + 1) < GetCurrentPageCount();

			categoriesTabButton.SetTabActive(currentMode == Mode.Categories);
			itemsTabButton.SetTabActive(currentMode == Mode.Items);
		}

		public void OnCurrentListItemClick(AutosorterFilter filter)
		{
			if (filter == null)
			{
				return;
			}

			locker.RemoveFilter(filter);
			RefreshCurrentFilters();
		}

		public void OnAvailableListItemClick(AutosorterFilter filter)
		{
			if (filter == null)
			{
				return;
			}

			locker.AddFilter(filter);
			RefreshCurrentFilters();
		}

		internal void ChangePage(int pageOffset)
		{
			var pageCount = GetCurrentPageCount();
			SetCurrentPage(Mathf.Clamp(GetCurrentPage() + pageOffset, 0, pageCount - 1));
			UpdateAvailableTypes();
		}

		private int GetCurrentPageCount()
		{
			return (int)Mathf.Ceil((float)availableTypes.Count / AutosortTarget.MaxTypes);
		}

		private void OnCategoriesButtonClick(AutosorterFilter obj)
		{
			if (currentMode != Mode.Categories)
			{
				currentMode = Mode.Categories;
				UpdateAvailableTypes();
			}
		}

		private void OnItemsButtonClick(AutosorterFilter obj)
		{
			if (currentMode != Mode.Items)
			{
				currentMode = Mode.Items;
				UpdateAvailableTypes();
			}
		}

		private int GetCurrentPage()
		{
			return currentMode == Mode.Categories ? currentPageCategories : currentPageItems;
		}

		private void SetCurrentPage(int page)
		{
			if (currentMode == Mode.Categories)
			{
				currentPageCategories = page;
			}
			else
			{
				currentPageItems = page;
			}
		}



		public static AutosortTypePicker Create(Transform parent, Text textPrefab)
		{
			var picker = LockerPrefabShared.CreateCanvas(parent).gameObject.AddComponent<AutosortTypePicker>();
			picker.GetComponent<Canvas>().sortingLayerID = 0;
			picker.gameObject.SetActive(false);

			var t = picker.transform;
			t.localEulerAngles = new Vector3(0, 180, 0);
			t.localPosition = new Vector3(0, 0, 0.4f);

			picker.background = LockerPrefabShared.CreateBackground(picker.transform);
			picker.background.color = new Color(0, 0, 0, 1);
			picker.background.type = Image.Type.Simple;
			RectTransformExtensions.SetSize(picker.background.rectTransform, 240, 220);

			int spacing = 20;
			int startY = 60;
			int x = 55;

			picker.underlines[0] = CreateUnderline(picker.background.transform, x);
			picker.underlines[1] = CreateUnderline(picker.background.transform, -x);

			var currentText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 12, "Current");
			currentText.rectTransform.anchoredPosition = new Vector2(-x, 90);

			picker.categoriesTabButton = CreatePickerButton(picker.background.transform, x - 23 + 2, 90, textPrefab, picker.OnCategoriesButtonClick, 60);
			picker.categoriesTabButton.Override("Categories", true);

			picker.itemsTabButton = CreatePickerButton(picker.background.transform, x + 30 + 2, 90, textPrefab, picker.OnItemsButtonClick, 38);
			picker.itemsTabButton.Override("Items", false);

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

		public static PickerButton CreatePickerButton(Transform parent, int x, int y, Text textPrefab, Action<AutosorterFilter> action, int width = 100)
		{
			var button = PickerButton.Create(parent, textPrefab, action, width);

			var rt = button.transform as RectTransform;
			rt.anchoredPosition = new Vector2(x, y);

			return button;
		}
	}
}
