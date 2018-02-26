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
		private int currentPage = 0;
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

		public void Initialize(AutosortTarget locker)
		{
			this.locker = locker;
			closeButton.target = locker;
			background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("MainMenuStandardSprite.png"), new RectOffset(20, 20, 20, 20));

			SetCurrentFilters(locker.GetCurrentFilters());
			UpdateAvailableTypes();
		}

		private void SetCurrentFilters(List<AutosorterFilter> filters)
		{
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
			return AutosorterList.GetEntries();
		}

		private void UpdateAvailableTypes()
		{
			availableTypes = GetAvailableTypes();
			int start = currentPage * AutosortTarget.MaxTypes;
			for (int i = 0; i < AutosortTarget.MaxTypes; ++i)
			{
				var filter = (start + i) >= availableTypes.Count ? null : availableTypes[start + i];
				availableList[i].SetFilter(filter);
			}
			pageText.text = string.Format("{0}/{1}", currentPage + 1, GetCurrentPageCount());
			prevPageButton.canChangePage = (currentPage > 0);
			nextPageButton.canChangePage = (currentPage + 1) < GetCurrentPageCount();
		}

		public void OnCurrentListItemClick(AutosorterFilter filter)
		{
			if (filter == null)
			{
				return;
			}

			var currentFilters = locker.GetCurrentFilters();
			currentFilters.Remove(filter);
			locker.SetFilters(currentFilters);

			SetCurrentFilters(currentFilters);
		}

		public void OnAvailableListItemClick(AutosorterFilter filter)
		{
			if (filter == null)
			{
				return;
			}

			var currentFilters = locker.GetCurrentFilters();
			if (currentFilters.Count >= AutosortTarget.MaxTypes)
			{
				return;
			}
			if (currentFilters.Contains(filter))
			{
				return;
			}

			currentFilters.Add(filter);
			locker.SetFilters(currentFilters);

			SetCurrentFilters(currentFilters);
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



		public static IEnumerator Create(Transform parent, Text textPrefab, AutosortTarget target)
		{
			var picker = LockerPrefabShared.CreateCanvas(parent).gameObject.AddComponent<AutosortTypePicker>();
			picker.GetComponent<Canvas>().sortingLayerID = 0;
			picker.gameObject.SetActive(false);

			var t = picker.transform;
			t.localEulerAngles = new Vector3(0, 180, 0);
			t.localPosition = new Vector3(0, 0, 0.4f);

			picker.background = LockerPrefabShared.CreateBackground(picker.transform);
			picker.background.type = Image.Type.Simple;
			RectTransformExtensions.SetSize(picker.background.rectTransform, 240, 220);

			yield return null;

			int spacing = 20;
			int startY = 60;
			int x = 55;

			picker.underlines[0] = CreateUnderline(picker.background.transform, x);
			picker.underlines[1] = CreateUnderline(picker.background.transform, -x);
			yield return null;

			var currentText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 12, "Current");
			currentText.rectTransform.anchoredPosition = new Vector2(-x, 90);

			var availableText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 12, "Items / Categories");
			availableText.rectTransform.anchoredPosition = new Vector2(x, 90);
			yield return null;

			picker.pageText = LockerPrefabShared.CreateText(picker.background.transform, textPrefab, Color.white, 90, 10, "1/X");
			picker.pageText.rectTransform.anchoredPosition = new Vector2(x, -80);

			picker.prevPageButton = AddPageButton(picker.background.transform, picker, -1, x - 20, -80);
			picker.nextPageButton = AddPageButton(picker.background.transform, picker, +1, x + 20, -80);

			picker.closeButton = AddCloseButton(picker.background.transform);
			yield return null;

			for (int i = 0; i < AutosortTarget.MaxTypes; ++i)
			{
				picker.currentList[i] = CreatePickerButton(picker.background.transform, -x, startY - (i * spacing), textPrefab, picker.OnCurrentListItemClick);
				yield return null;
				picker.availableList[i] = CreatePickerButton(picker.background.transform, x, startY - (i * spacing), textPrefab, picker.OnAvailableListItemClick);
				yield return null;
			}

			target.SetPicker(picker);
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

		public static PickerButton CreatePickerButton(Transform parent, int x, int y, Text textPrefab, Action<AutosorterFilter> action)
		{
			var button = PickerButton.Create(parent, textPrefab, action);

			var rt = button.transform as RectTransform;
			rt.anchoredPosition = new Vector2(x, y);

			return button;
		}
	}
}
