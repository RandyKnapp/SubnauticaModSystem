using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	class Picker : MonoBehaviour
	{
		protected const float ButtonSize = 30;
		protected const int ButtonsPerRow = 5;
		protected const float Spacing = 35;
		protected const float StartX = -(Spacing * 2);
		protected const float StartY = (Spacing * 1.75f);
		protected const int ButtonsPerPage = 20;

		public RectTransform rectTransform;

		protected int page;

		public Action<int> onSelect;

		[SerializeField]
		private Image background;
		[SerializeField]
		private PickerPageButton prevPageButton;
		[SerializeField]
		private PickerPageButton nextPageButton;
		[SerializeField]
		private Text pageText;
		[SerializeField]
		protected List<PickerButton> buttons = new List<PickerButton>();

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public virtual void Initialize()
		{
			bool firstInitialize = false;
			if (background.sprite == null)
			{
				if (buttons.Count <= 10)
				{
					background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("SmallBackground.png"));
				}
				else
				{
					background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("PickerBackground.png"));
				}
				firstInitialize = true;
			}

			var startY = buttons.Count <= 5 ? 0 : buttons.Count <= 10 ? (Spacing * 0.5f) : StartY;
			for (int i = 0; i < this.buttons.Count; ++i)
			{
				var button = this.buttons[i];
				int page = i / ButtonsPerPage;
				int row = (i / ButtonsPerRow) % (ButtonsPerPage / ButtonsPerRow);
				int col = i % ButtonsPerRow;
				button.rectTransform.anchoredPosition = new Vector2(StartX + (Spacing * col), startY - (row * Spacing));
				button.onClick += OnClick;
				button.gameObject.SetActive(page == 0);
				button.toggled = false;
			}

			prevPageButton.Initialize("Left.png", Color.white);
			nextPageButton.Initialize("Right.png", Color.white);

			if (firstInitialize)
			{
				prevPageButton.onClick += OnPrevPage;
				nextPageButton.onClick += OnNextPage;
			}

			page = 0;
			int pages = GetPageCount();
			prevPageButton.gameObject.SetActive(pages > 1);
			nextPageButton.gameObject.SetActive(pages > 1);
			pageText.gameObject.SetActive(pages > 1);

			UpdateText();
		}

		private int GetPageCount()
		{
			return Mathf.CeilToInt((float)buttons.Count / ButtonsPerPage);
		}

		private void UpdateText()
		{
			int pages = GetPageCount();
			pageText.text = (page + 1) + "/" + pages;
		}

		private void OnPrevPage()
		{
			ShowPage(page - 1);
		}

		private void OnNextPage()
		{
			ShowPage(page + 1);
		}

		protected void ShowPage(int newPage)
		{
			int pages = GetPageCount();
			page = newPage;
			if (page < 0)
			{
				page = pages - 1;
			}
			else if (page >= pages)
			{
				page = 0;
			}

			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				int buttonPage = i / ButtonsPerPage;
				button.gameObject.SetActive(page == buttonPage);
			}
			UpdateText();
		}

		protected void OnClick(int index)
		{
			onSelect(index);

			for (int i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				button.toggled = index == i;
				button.OnPointerExit(null);
				button.onClick -= OnClick;
				button.Update();
			}
		}

		protected static void Create(Transform parent, Picker instance, int buttonCount)
		{
			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
			textPrefab.fontSize = 12;
			textPrefab.color = HabitatControlPanel.ScreenContentColor;

			var rt = instance.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, 1000 / 5.0f, (buttonCount <= 10 ? 400 : 900) / 5.0f);

			instance.background = instance.gameObject.AddComponent<Image>();

			for (int i = 0; i < buttonCount; ++i)
			{
				var colorButton = PickerButton.Create(instance.transform, ButtonSize, ButtonSize * 0.7f);
				instance.buttons.Add(colorButton);
			}

			instance.prevPageButton = PickerPageButton.Create(instance.transform, Color.white);
			(instance.prevPageButton.transform as RectTransform).anchoredPosition = new Vector2(-20, -70);

			instance.nextPageButton = PickerPageButton.Create(instance.transform, Color.white);
			(instance.nextPageButton.transform as RectTransform).anchoredPosition = new Vector2(20, -70);

			instance.pageText = LockerPrefabShared.CreateText(instance.transform, textPrefab, Color.white, 0, 10, "X/X");
			RectTransformExtensions.SetSize(instance.pageText.rectTransform, 20, 20);
			instance.pageText.rectTransform.anchoredPosition = new Vector2(0, -70);
		}
	}
}
