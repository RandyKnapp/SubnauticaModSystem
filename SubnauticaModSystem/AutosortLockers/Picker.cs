using Common.Mod;
using Common.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if BZ
using TMPro;
#endif

namespace AutosortLockers
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
		protected List<ColorPickerButton> buttons = new List<ColorPickerButton>();

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public virtual void Initialize()
		{
			int rows = Mathf.CeilToInt(ButtonsPerPage / (float)ButtonsPerRow);
			float StartX = -(Spacing * (ButtonsPerRow - 1) / 2.0f);
			float StartY = (Spacing * ((rows - 0.5f) / 2.0f));
			for (int i = 0; i < this.buttons.Count; ++i)
			{
				var button = this.buttons[i];
				int page = i / ButtonsPerPage;
				int row = (i / ButtonsPerRow) % (ButtonsPerPage / ButtonsPerRow);
				int col = i % ButtonsPerRow;
				button.rectTransform.anchoredPosition = new Vector2(StartX + (Spacing * col), StartY - (row * Spacing) - 3);
				button.onClick += OnClick;
				button.gameObject.SetActive(page == 0);
				button.toggled = false;
			}
		}

		private int GetPageCount()
		{
			return Mathf.CeilToInt((float)buttons.Count / ButtonsPerPage);
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

		/*__________________________________________________________________________________________________________*/

		protected static void Create(Transform parent, Picker colorGrid, int buttonCount, GameObject lockerPrefab = null)
		{
#if SN
			lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
#elif BZ
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<TextMeshProUGUI>());
#endif

			textPrefab.fontSize = 12;
			textPrefab.color = ScreenContentColor;

			float padding = 10;
			float width = padding + colorGrid.ButtonSize + (colorGrid.ButtonsPerRow * colorGrid.Spacing);
			int rowCount = Mathf.CeilToInt(colorGrid.ButtonsPerPage / (float)colorGrid.ButtonsPerRow);
			float height = 20 + colorGrid.ButtonSize + ((rowCount - 0.5f) * colorGrid.Spacing);

			var rt = colorGrid.rectTransform;
			// The first Vector2 positions the colorGrid on the locker horz / vert.
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.6f), parent);
			RectTransformExtensions.SetSize(rt, width, height);

			colorGrid.background = colorGrid.gameObject.AddComponent<Image>();
			colorGrid.background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Background.png"));
			colorGrid.background.color = new Color(1, 1, 1);

			for (int i = 0; i < buttonCount; ++i)
			{
				var colorButton = ColorPickerButton.Create(colorGrid.transform, colorGrid.ButtonSize, colorGrid.ButtonSize * 0.7f);
				colorGrid.buttons.Add(colorButton);
			}
		}
	}
}