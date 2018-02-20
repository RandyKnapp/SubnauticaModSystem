using BetterPowerInfo.Producers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	class PowerDisplayEntry : MonoBehaviour
	{
		public const float LineHeight = 30;
		public const float Width = 550;
		public const float LineSpacing = 5;

		protected RectTransform rectTransform;
		protected LayoutElement layoutElement;
		protected VerticalLayoutGroup vLayout;
		protected GameObject[] lines = new GameObject[2];
		protected Text nameText;
		protected Text customText;
		protected Text totalText;
		protected List<PowerSourceInfo> sources = new List<PowerSourceInfo>();

		public TechType techType;

		public static PowerDisplayEntry Create(string name, Transform parent, TechType techType)
		{
			var obj = new GameObject(name, typeof(RectTransform)).AddComponent<PowerDisplayEntry>();
			obj.transform.SetParent(parent, false);
			obj.techType = techType;

			return obj;
		}

		private void Awake()
		{
			rectTransform = transform as RectTransform;
			RectTransformExtensions.SetParams(rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Width);

			//layoutElement = gameObject.AddComponent<LayoutElement>();
			//layoutElement.minHeight = LineHeight;// (LineHeight * lines.Length) + LineSpacing;

			vLayout = gameObject.AddComponent<VerticalLayoutGroup>();
			vLayout.childAlignment = TextAnchor.UpperRight;
			vLayout.childControlHeight = vLayout.childControlWidth = false;
			vLayout.childForceExpandHeight = vLayout.childForceExpandWidth = false;

			lines[0] = CreateLine("Line1");
			lines[1] = CreateLine("Line2");

			nameText = CreateAutoFitText("Name", 0);
			nameText.text = "Test";

			customText = CreateAutoFitText("Custom", 0);
			customText.text = "Custom";

			totalText = CreateAutoFitText("Power", 0);
			totalText.color = new Color(0, 1, 0);
			totalText.text = "+XX";

			var test = CreateAutoFitText("Test", 1);
			test.text = "Line 2 text";

			var image = gameObject.AddComponent<Image>();
			image.color = new Color(0, 0, 0, 0.5f);
			image.raycastTarget = false;
		}

		private GameObject CreateLine(string name)
		{
			var line = new GameObject(name, typeof(RectTransform)).AddComponent<HorizontalLayoutGroup>();
			line.transform.SetParent(transform, false);
			line.spacing = LineSpacing;
			line.childAlignment = TextAnchor.UpperRight;
			line.childControlHeight = line.childControlWidth = false;
			line.childForceExpandHeight = line.childForceExpandWidth = false;

			var l = line.gameObject.AddComponent<LayoutElement>();
			l.minHeight = LineHeight;

			var rt = line.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Width);
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LineHeight);

			return line.gameObject;
		}

		private Text CreateAutoFitText(string name, int line, bool richText = true)
		{
			var text = Mod.InstantiateNewText(name, lines[line].transform);
			text.alignment = TextAnchor.UpperRight;
			text.supportRichText = richText;

			var rt = text.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(1, 1), new Vector2(1, 1));

			var contentFitter = text.gameObject.AddComponent<ContentSizeFitter>();
			contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			return text;
		}

		public virtual void Refresh()
		{
			if (sources.Count == 0)
			{
				return;
			}

			var source = sources[0];
			nameText.text = source.Name + (sources.Count > 1 ? " x" + sources.Count : "");
			totalText.text = "+" + Math.Round(GetTotalPowerProduction(), 1);

			lines[1].gameObject.SetActive(sources.Count > 1);
		}

		protected float GetTotalPowerProduction()
		{
			return sources.Sum((source) => source.ProductionPerMinute);
		}

		public bool ShouldShow()
		{
			return sources.Count > 0;
		}

		public void SetSources(List<PowerSourceInfo> sources)
		{
			this.sources = sources;
		}
	}
}
