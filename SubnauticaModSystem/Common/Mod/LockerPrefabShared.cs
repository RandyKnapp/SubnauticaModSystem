using System;
using UnityEngine;
using UnityEngine.UI;
#if BELOWZERO
using TMPro;
#endif

namespace Common.Mod
{
	public static class LockerPrefabShared
	{
		internal static Canvas CreateCanvas(Transform parent)
		{
			var canvas = new GameObject("Canvas", typeof(RectTransform)).AddComponent<Canvas>();
			var t = canvas.transform;
			t.SetParent(parent, false);
			canvas.sortingLayerID = 1;

			var raycaster = canvas.gameObject.AddComponent<uGUI_GraphicRaycaster>();

			var rt = t as RectTransform;
			// Positions the ??
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
			RectTransformExtensions.SetSize(rt, 1.7f, 3.0f);
			// Seems to be the front to back position of the small locker
			t.localPosition = new Vector3(0, 0, 0.345f);
			t.localRotation = new Quaternion(0, 1, 0, 0);
			t.localScale = new Vector3(0.5f, 0.5f, 0.5f);

			canvas.scaleFactor = 0.01f;
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.referencePixelsPerUnit = 100;

			var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
			scaler.dynamicPixelsPerUnit = 20;

			return canvas;
		}

#if SUBNAUTICA
    internal static Image CreateBackground(Transform parent)
    {
      var background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
      var rt = background.rectTransform;
      RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
      RectTransformExtensions.SetSize(rt, 114, 241);
      background.color = new Color(0, 0, 0);

      background.transform.localScale = new Vector3(0.01f, 0.01f, 1);
      background.type = Image.Type.Sliced;

      return background;
    }
#elif BELOWZERO
		internal static Image CreateBackground(Transform parent, string lockerType)
		{
			var background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
			var rt = background.rectTransform;

			// The size of the rectangle on the locker, adjusted for locker type
			if (lockerType == "Locker(Clone)")
			{ // The second Vector2 positions the background on the locker, the 1st number is horizontal and the 2nd is vertical
				RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.43f, 0.5f), parent);
				RectTransformExtensions.SetSize(rt, 150, 280);
			}
			else //(lockerType == "SmallLocker(Clone)")
			{
				RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.52f, 0.49f), parent);
				RectTransformExtensions.SetSize(rt, 128, 264);
			}
			// Overrides the color of the png image
			background.color = new Color(0, 0, 0);
			background.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
			background.type = Image.Type.Sliced;

			return background;
		}
#endif

		internal static Image CreateIcon(Transform parent, Color color, int y)
		{
			var icon = new GameObject("Text", typeof(RectTransform)).AddComponent<Image>();
			var rt = icon.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			// The size of the locker icon
			RectTransformExtensions.SetSize(rt, 40, 40);

			rt.anchoredPosition = new Vector2(0, y);
			icon.color = color;

			return icon;
		}

#if SUBNAUTICA
		internal static Text CreateText(Transform parent, Text prefab, Color color, int y, int size, string initial)
		{
			var text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
#elif BELOWZERO
		internal static TextMeshProUGUI CreateText(Transform parent, TextMeshProUGUI prefab, Color color, int y, int size, string initial, string lockerType)
		{
			var text = new GameObject("TextMeshProUGUI", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
#endif
			var rt = text.rectTransform;
			// Positions the "Locker" label on the locker
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);

			// The size of the Filter rectangle for each locker type
			if (lockerType == "Locker(Clone)")
			{
				RectTransformExtensions.SetSize(rt, 138, 200);
			}
			else
			{
				RectTransformExtensions.SetSize(rt, 112, 200);
			}

			rt.anchoredPosition = new Vector2(0, y);
			
			text.font = prefab.font;
			text.fontSize = size;
			text.color = color;
			text.text = initial;

			return text;
		}
	}
}