using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace MoreQuickSlots
{
	public class GameController : MonoBehaviour
	{
		private uGUI_QuickSlots _quickSlots;
		private bool _tryAddLabels;

		[UsedImplicitly]
        public void Awake()
		{
			_quickSlots = GetComponent<uGUI_QuickSlots>();
		}

		[UsedImplicitly]
        public void Update()
		{
			if (uGUI.main == null || uGUI.main.loading.isLoading)
				return;

			if (CanSetQuickSlots())
			{
				for (var i = Player.quickSlotButtonsCount; i < MoreQuickSlots.SlotCount.Value; ++i)
				{
					if (MoreQuickSlots.GetKeyDownForSlot(i))
					{
						SelectQuickSlot(i);
					}
				}
			}

			if (_tryAddLabels)
			{
				AddHotkeyLabels(_quickSlots);
			}
		}

		private static bool CanSetQuickSlots()
		{
			if (Inventory.main == null)
			{
				return false;
			}

            var isIntroActive = EscapePod.main.IsPlayingIntroCinematic();
			if (isIntroActive)
			{
				return false;
			}

			var player = Player.main;
			return player != null && player.GetMode() != Player.Mode.Piloting && player.GetCanItemBeUsed();
		}
		
		private static void SelectQuickSlot(int slotID)
		{
			Inventory.main.quickSlots.SlotKeyDown(slotID);
		}

		public void AddHotkeyLabels(uGUI_QuickSlots instance)
		{
			if (instance == null || !MoreQuickSlots.ShowInputText.Value)
			{
				_tryAddLabels = true;
				return;
			}

            var icons = instance.icons;
			if (icons == null || icons.Length == 0)
			{
				_tryAddLabels = true;
				return;
			}

			var textPrefab = GetTextPrefab();
			if (textPrefab == null)
			{
				_tryAddLabels = true;
				return;
			}

			for (var i = 0; i < icons.Length; ++i)
			{
				var icon = icons[i];
                var inputText = MoreQuickSlots.GetInputForSlot(i);
				CreateNewText(textPrefab, icon.transform, $"<color=#ADF8FFFF>{inputText}</color>", i);
			}
			_tryAddLabels = false;
		}

		private static TextMeshProUGUI GetTextPrefab()
		{
			var prefabObject = FindObjectOfType<HandReticle>();
			if (prefabObject == null)
			{
				return null;
			}

            var prefab = prefabObject.compTextHand;
			if (prefab == null)
			{
				return null;
			}

			return prefab;
		}

		private static void CreateNewText(TextMeshProUGUI prefab, Transform parent, string newText, int index = -1)
		{
			var text = Instantiate(prefab);
			text.gameObject.layer = parent.gameObject.layer;
			text.gameObject.name = "QuickSlotText" + (index >= 0 ? index.ToString() : "");
			text.transform.SetParent(parent, false);
			text.transform.localScale = new Vector3(1, 1, 1);
			text.gameObject.SetActive(true);
			text.enabled = true;
			text.text = newText;
			text.fontSize = 17;
			RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
			text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
			text.rectTransform.anchoredPosition = new Vector3(0, -36);
            text.alignment = TextAlignmentOptions.Center;
			text.raycastTarget = false;
		}
	}
}
