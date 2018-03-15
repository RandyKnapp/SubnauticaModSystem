using Common.Mod;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CustomBeacons
{
	class Picker : MonoBehaviour
	{
		public static readonly Color ScreenContentColor = new Color32(188, 254, 254, 255);

		protected float ButtonSize = 30;
		protected int ButtonsPerRow = 5;
		protected float Spacing = 35;
		protected int ButtonsPerPage = 20;

		public RectTransform rectTransform;

		protected int page;

		public Action<int> onSelect = delegate { };
		public Action onClose = delegate { };

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
				background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("PickerBackground.png"), new RectOffset(25, 25, 25, 25));
				firstInitialize = true;
			}

			int pages = GetPageCount();

			int rows = Mathf.CeilToInt(ButtonsPerPage / (float)ButtonsPerRow);
			float StartX = -(Spacing * (ButtonsPerRow - 1) / 2.0f);
			float StartY = (Spacing * ((rows - 0.5f) / 2.0f));
			for (int i = 0; i < this.buttons.Count; ++i)
			{
				var button = this.buttons[i];
				int page = i / ButtonsPerPage;
				int row = (i / ButtonsPerRow) % (ButtonsPerPage / ButtonsPerRow);
				int col = i % ButtonsPerRow;
				button.rectTransform.anchoredPosition = new Vector2(StartX + (Spacing * col), StartY - (row * Spacing));
				button.onClick += OnClick;
				button.gameObject.SetActive(page == 0);
				button.toggled = false;
			}

			prevPageButton.Initialize("Left.png", Color.white);
			nextPageButton.Initialize("Right.png", Color.white);

			var paginationY = -Spacing * rows / 2.0f;
			(prevPageButton.transform as RectTransform).anchoredPosition = new Vector2(-20, paginationY);
			(nextPageButton.transform as RectTransform).anchoredPosition = new Vector2(20, paginationY);
			pageText.rectTransform.anchoredPosition = new Vector2(0, paginationY);

			if (firstInitialize)
			{
				prevPageButton.onClick += OnPrevPage;
				nextPageButton.onClick += OnNextPage;
			}

			page = 0;
			//prevPageButton.gameObject.SetActive(pages > 1);
			//nextPageButton.gameObject.SetActive(pages > 1);
			//pageText.gameObject.SetActive(pages > 1);

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
				button.Update();
			}
		}

		public void Toggle()
		{
			if (gameObject.activeSelf)
			{
				Close();
			}
			else
			{
				Open();
			}
		}

		public virtual void Open()
		{
			gameObject.SetActive(true);
		}

		public void Close()
		{
			onClose();
			gameObject.SetActive(false);
		}



		///////////////////////////////////////////////////////////////////////////////////////////////////////
		protected static void Create(Transform parent, Picker instance, int buttonCount)
		{
			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
			textPrefab.fontSize = 16;
			textPrefab.color = ScreenContentColor;

			float padding = 30;
			float width = padding + instance.ButtonSize + ((instance.ButtonsPerRow - 1) * instance.Spacing);
			int rowCount = Mathf.CeilToInt(instance.ButtonsPerPage / (float)instance.ButtonsPerRow);
			float height = padding + instance.ButtonSize + ((rowCount - 0.5f) * instance.Spacing) - 10;

			var rt = instance.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, width, height);

			instance.background = instance.gameObject.AddComponent<Image>();
			instance.background.type = Image.Type.Sliced;
			instance.background.rectTransform.anchoredPosition = new Vector2(0, -20);

			instance.pageText = LockerPrefabShared.CreateText(instance.transform, textPrefab, Color.white, 0, 10, "X/X");
			RectTransformExtensions.SetSize(instance.pageText.rectTransform, 30, 20);

			for (int i = 0; i < buttonCount; ++i)
			{
				var colorButton = PickerButton.Create(instance.transform, instance.ButtonSize, instance.ButtonSize * 0.7f);
				instance.buttons.Add(colorButton);
			}

			instance.prevPageButton = PickerPageButton.Create(instance.transform, Color.white);
			instance.nextPageButton = PickerPageButton.Create(instance.transform, Color.white);
		}
	}
}
