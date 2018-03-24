using Common.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TorpedoImprovements
{
	class TorpedoHudIcon : MonoBehaviour
	{
		private const int TextSize = 18;
		private const float IconSize = 60;
		private static readonly Color TextNoneColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		private static readonly Color IconNoneColor = new Color(1, 1, 1, 0.3f);

		public RectTransform rectTransform { get => transform as RectTransform; }

		private uGUI_Icon icon;
		private Text text;
		private TechType techType = TechType.None;
		private int count = -1;

		private void InitializeIcon()
		{
			if (icon == null)
			{
				icon = new GameObject("Icon", typeof(RectTransform)).AddComponent<uGUI_Icon>();
				RectTransformExtensions.SetParams(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
				RectTransformExtensions.SetSize(icon.rectTransform, IconSize, IconSize);
				icon.raycastTarget = false;
			}
		}

		private void InitializeText()
		{
			if (text == null)
			{
				var textPrefab = ModUtils.GetTextPrefab();

				text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
				RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
				RectTransformExtensions.SetSize(text.rectTransform, IconSize, IconSize);
				text.font = textPrefab.font;
				text.color = Color.white;
				text.fontSize = TextSize;
				text.alignment = TextAnchor.UpperRight;
				text.raycastTarget = false;

				ModUtils.CopyComponent(textPrefab.GetComponent<Outline>(), text.gameObject);
				ModUtils.CopyComponent(textPrefab.GetComponent<Shadow>(), text.gameObject);
			}
		}

		public void SetTechType(TechType type)
		{
			if (techType != type)
			{
				this.techType = type;
				InitializeIcon();
				icon.sprite = SpriteManager.Get(techType);
			}
		}

		public void SetCount(int count)
		{
			if (count != this.count)
			{
				this.count = count;
				InitializeText();
				text.text = "x" + count;
				text.color = count == 0 ? TextNoneColor : Color.white;

				InitializeIcon();
				icon.color = count == 0 ? IconNoneColor : Color.white;
			}
		}

		public void Refresh(SeaMoth seamoth, int slotID)
		{
			var torpedoStorage = seamoth.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
			if (torpedoStorage != null)
			{
				SetCount(torpedoStorage.GetCount(techType));
			}
		}
	}
}
