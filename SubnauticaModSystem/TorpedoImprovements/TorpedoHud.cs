using Common.Mod;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Common.Utility;
using System.Linq;

namespace TorpedoImprovements
{
	class TorpedoHud : MonoBehaviour
	{
		private const int TextSize = 40;
		private const int IconSize = 60;

		public RectTransform rectTransform { get => transform as RectTransform; }
		public bool IsActive { get => activeIndicator.gameObject.activeSelf; }

		private int slotID;
		private Image background;
		private Image activeIndicator;
		private Text slotText;
		private List<TorpedoHudIcon> icons = new List<TorpedoHudIcon>();

		public void Initialize(int slot)
		{
			var textPrefab = ModUtils.GetTextPrefab();

			slotID = slot;

			background = new GameObject("Background").AddComponent<Image>();
			RectTransformExtensions.SetParams(background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
			background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("Background.png"), new RectOffset(IconSize, IconSize, 0, 0));
			background.type = Image.Type.Sliced;
			background.color = new Color(0, 0, 0, 0.9f);
			background.raycastTarget = false;

			activeIndicator = new GameObject("ActiveIndicator").AddComponent<Image>();
			activeIndicator.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Selected.png"));
			RectTransformExtensions.SetParams(activeIndicator.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
			RectTransformExtensions.SetSize(activeIndicator.rectTransform, IconSize, IconSize);
			activeIndicator.raycastTarget = false;
			ModUtils.CopyComponent(textPrefab.GetComponent<Outline>(), activeIndicator.gameObject);
			ModUtils.CopyComponent(textPrefab.GetComponent<Shadow>(), activeIndicator.gameObject);
			//activeArrow.rectTransform.anchoredPosition = new Vector2((slotID % 2 == 0 ? 1 : -1) * IconSize, 0);
			activeIndicator.rectTransform.localScale = new Vector3((slotID % 2 == 0 ? -1 : 1), 1, 1);
			activeIndicator.gameObject.SetActive(false);

			slotText = new GameObject("SlotText", typeof(RectTransform)).AddComponent<Text>();
			RectTransformExtensions.SetParams(slotText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
			RectTransformExtensions.SetSize(slotText.rectTransform, 100, 100);
			slotText.font = textPrefab.font;
			slotText.color = Color.white;
			slotText.fontSize = TextSize;
			slotText.alignment = TextAnchor.MiddleCenter;
			slotText.text = (slotID + 1).ToString();
			slotText.raycastTarget = false;

			ModUtils.CopyComponent(textPrefab.GetComponent<Outline>(), slotText.gameObject);
			ModUtils.CopyComponent(textPrefab.GetComponent<Shadow>(), slotText.gameObject);

			const int xOffset = 200;
			const int xSpacing = -30;
			const int yOffset = -480;
			const int ySpacing = 70;
			Vector2[] positions = {
				new Vector2(-xOffset, yOffset),
				new Vector2(xOffset, yOffset),
				new Vector2(-xOffset - xSpacing, yOffset + ySpacing),
				new Vector2(xOffset + xSpacing, yOffset + ySpacing)
			};
			rectTransform.anchoredPosition = positions[slotID];
		}

		internal TechType GetNextTorpedoType(SeaMoth seamoth)
		{
			var torpedoStorage = seamoth?.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
			if (torpedoStorage == null || torpedoStorage.count == 0)
			{
				return TechType.None;
			}

			foreach (var t in seamoth.torpedoTypes)
			{
				var count = torpedoStorage.GetCount(t.techType);
				if (count > 0)
				{
					return t.techType;
				}
			}

			return TechType.None;
		}

		private void Update()
		{
			SeaMoth seamoth = Player.main?.GetVehicle() as SeaMoth;
			if (seamoth == null)
			{
				return;
			}

			if (icons.Count != seamoth.torpedoTypes.Length)
			{
				CreateIcons(seamoth);

				seamoth.onToggle += OnTorpedoToggle;
			}

			foreach (var icon in icons)
			{
				icon.Refresh(seamoth, slotID);
			}
		}

		private void OnTorpedoToggle(int slotID, bool state)
		{
			if (slotID == this.slotID)
			{
				activeIndicator.gameObject.SetActive(state);
			}
		}

		private void CreateIcons(SeaMoth seamoth)
		{
			DestroyIcons();

			foreach (var type in seamoth.torpedoTypes)
			{
				var tech = type.techType;

				var icon = new GameObject(tech + "Icon", typeof(RectTransform)).AddComponent<TorpedoHudIcon>();
				RectTransformExtensions.SetParams(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
				icon.SetTechType(tech);
				icon.rectTransform.anchoredPosition = new Vector2((slotID % 2 == 0 ? -1 : 1) * (icons.Count + 1) * IconSize, 0);
				icons.Add(icon);
			}

			var bgWidth = (icons.Count + 1) * IconSize + 10;
			var bgHeight = IconSize + 10;
			RectTransformExtensions.SetSize(background.rectTransform, bgWidth, bgHeight);
			background.rectTransform.anchoredPosition = new Vector2((slotID % 2 == 0 ? -1 : 1) * (icons.Count) * IconSize / 2.0f, 0);
		}

		private void DestroyIcons()
		{
			foreach (var icon in icons)
			{
				Destroy(icon.gameObject);
			}
			icons.Clear();
		}

		public void Reset(SeaMoth seamoth)
		{
			for (var i = 0; i < seamoth.torpedoTypes.Length; ++i)
			{
				var icon = icons[i];
				var tech = seamoth.torpedoTypes[i].techType;

				icon.SetTechType(tech);
				icon.Refresh(seamoth, slotID);
			}
		}
	}
}
