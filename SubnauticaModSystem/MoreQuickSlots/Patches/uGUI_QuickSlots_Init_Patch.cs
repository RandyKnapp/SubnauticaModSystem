using Harmony;
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

			GameObject hud = GetHud();
			if (textPrefab == null)
			{
				return;
			}

			Logger.Log("Adding text to screen...");
			Text text = CreateNewText(textPrefab, hud.transform, "TEST TEXT");
			text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			text.rectTransform.localPosition = new Vector3(0, 0, 0);

			GameObject foundText = GameObject.Find("QuickSlotText");
			Text t = foundText.GetComponent<Text>();

			if (foundText != null)
			{
				Logger.Log("foundText active = " + foundText.activeSelf + ", " + foundText.activeInHierarchy);
				Logger.Log("foundText scale = " + foundText.transform.localScale);
				Logger.Log("foundText position = " + foundText.transform.position + ", " + foundText.transform.localPosition);
				Logger.Log("foundText parent = " + foundText.transform.parent);
				Logger.Log("foundText text = '" + t.text + "', " + t.font + ", " + t.fontSize);

				Logger.Log("foundText components:");
				var components = foundText.GetComponents(typeof(Component));
				foreach(var c in components)
				{
					Logger.Log("   " + c.name + ": " + c);
				}

				CanvasRenderer cr = foundText.GetComponent<CanvasRenderer>();
				Logger.Log("foundText.CanvasRenderer alpha/color: " + cr.GetAlpha() + ", " + cr.GetColor());
			}

			/*var targetField = typeof(uGUI_QuickSlots).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
			var iconsField = typeof(uGUI_QuickSlots).GetField("icons", BindingFlags.NonPublic | BindingFlags.Instance);
			IQuickSlots target = (IQuickSlots)targetField.GetValue(__instance);
			uGUI_ItemIcon[] icons = (uGUI_ItemIcon[])iconsField.GetValue(__instance);
			for (int i = 0; i < icons.Length; ++i)
			{
				uGUI_ItemIcon icon = icons[i];
				GameObject newText = new GameObject("Text" + i);
				newText.transform.SetParent(icon.transform, false);
				newText.transform.position = new Vector3(0, 0, 0);
				newText.AddComponent<Text>();
				newText.GetComponent<Text>().text = "XXXXXXXX";
			}*/
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

		private static GameObject GetHud()
		{
			GameObject hud = GameObject.FindObjectOfType<uGUI_PowerIndicator>().gameObject;
			if (hud == null)
			{
				Logger.Log("Could not find HUD!");
				return null;
			}

			return hud;
		}

		private static Text CreateNewText(Text prefab, Transform parent, string newText)
		{
			Text text = GameObject.Instantiate(prefab);
			text.gameObject.name = "QuickSlotText";
			text.transform.SetParent(parent, false);
			text.transform.localScale = new Vector3(1, 1, 1);
			text.gameObject.SetActive(true);
			text.enabled = true;
			text.text = newText;

			return text;
		}

	}
}
