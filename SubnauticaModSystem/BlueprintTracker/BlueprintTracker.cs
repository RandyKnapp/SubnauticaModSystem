using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTracker
{
	class BlueprintTracker : MonoBehaviour
	{
		public const float Spacing = 10;
		public const float Width = 400;
		public const float Height = (BlueprintTrackerEntry.Height * Mod.MaxPins) + (Spacing * (Mod.MaxPins - 1));

		public RectTransform rectTransform;
		private VerticalLayoutGroup layout;

		public static BlueprintTracker Create(Transform parent)
		{
			var go = new GameObject("BlueprintTracker", typeof(RectTransform));
			go.transform.SetParent(parent, false);
			go.layer = parent.gameObject.layer;
			var tracker = go.AddComponent<BlueprintTracker>();
			Logger.Log("Tracker Created");

			return tracker;
		}

		public void Awake()
		{
			rectTransform = (RectTransform)transform;
			rectTransform.anchorMin = new Vector2(0, 1);
			rectTransform.anchorMax = new Vector2(0, 1);
			rectTransform.pivot = new Vector2(0, 1);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Width);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
			rectTransform.anchoredPosition = new Vector2(20, -20);

			/*var image = gameObject.AddComponent<Image>();
			image.color = new Color(0, 0, 0, 0.5f);
			image.raycastTarget = false;*/

			layout = gameObject.AddComponent<VerticalLayoutGroup>();
			layout.spacing = Spacing;
			layout.childAlignment = TextAnchor.UpperRight;
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.padding = new RectOffset(10, 10, 10, 10);

			BlueprintTrackerEntry.Create(transform, TechType.ComputerChip);
			BlueprintTrackerEntry.Create(transform, TechType.Battery);
			BlueprintTrackerEntry.Create(transform, TechType.TitaniumIngot);
			BlueprintTrackerEntry.Create(transform, TechType.Seamoth);

			//Logger.Log("Printing Tracker:");
			//Mod.PrintObject(gameObject);
		}
	}
}
