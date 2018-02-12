using Harmony;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace MoreQuickSlots.Patches
{
	[HarmonyPatch(typeof(uGUI_QuickSlots))]
	[HarmonyPatch("Init")]
	class uGUI_QuickSlots_Init_Patch
	{
		private static void Postfix(uGUI_QuickSlots __instance)
		{
			Text textPrefab = GetTextPrefab();
			if (textPrefab == null)
			{
				return;
			}

			Transform parent = GetParentForNewText();
			if (parent == null)
			{
				return;
			}

			var iconsField = typeof(uGUI_QuickSlots).GetField("icons", BindingFlags.NonPublic | BindingFlags.Instance);
			uGUI_ItemIcon[] icons = (uGUI_ItemIcon[])iconsField.GetValue(__instance);
			for (int i = 0; i < icons.Length; ++i)
			{
				uGUI_ItemIcon icon = icons[i];
				CreateNewText(textPrefab, icon.transform, Mod.GetHintTextForSlot(i), i);
			}
		}

		private static Text GetTextPrefab()
		{
			Text prefab = GameObject.FindObjectOfType<HandReticle>().interactPrimaryText;
			if (prefab == null)
			{
				Logger.Log("Could not find text prefab! (HandReticle.interactPrimaryText)");
				return null;
			}

			return prefab;
		}

		private static Transform GetParentForNewText()
		{
			GameObject hud = GameObject.FindObjectOfType<uGUI_PowerIndicator>().gameObject;
			if (hud == null)
			{
				Logger.Log("Could not find HUD!");
				return null;
			}

			Transform content = hud.transform.Find("Content");
			if (content == null)
			{
				Logger.Log("Could not find child named 'Content' in HUD!");
				return null;
			}
			return content;
		}

		private static Text CreateNewText(Text prefab, Transform parent, string newText, int index = -1)
		{
			Text text = GameObject.Instantiate(prefab);
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
			text.alignment = TextAnchor.MiddleCenter;
			text.raycastTarget = false;

			return text;
		}

	}
}
