using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTracker
{
	class BlueprintTracker : MonoBehaviour
	{
		public const float Spacing = 10;
		public const float Width = 400;
		public const float Height = (BlueprintTrackerEntry.Height * Mod.MaxPins) + (Spacing * (Mod.MaxPins - 1));

		private static BlueprintTracker instance;

		public RectTransform rectTransform;
		private VerticalLayoutGroup layout;
		private List<TechType> tracked = new List<TechType>();

		public static BlueprintTracker Create(Transform parent)
		{
			if (instance != null)
			{
				DestroyImmediate(instance);
			}

			var go = new GameObject("BlueprintTracker", typeof(RectTransform));
			go.transform.SetParent(parent, false);
			go.layer = parent.gameObject.layer;
			var tracker = go.AddComponent<BlueprintTracker>();
			Logger.Log("Tracker Created");

			instance = tracker;
			return tracker;
		}

		public static bool IsTracked(TechType techType)
		{
			if (instance == null)
			{
				Logger.Error("IsTracked: Instance is null!");
				return false;
			}

			return instance.tracked.Contains(techType);
		}

		public static bool CanTrack(TechType techType)
		{
			bool locked = !CrafterLogic.IsCraftRecipeUnlocked(techType);
			if (locked)
			{
				//Logger.Error("Can't start tracking " + techType + " because it is locked");
				return false;
			}
			if (IsTracked(techType))
			{
				//Logger.Error("Can't start tracking " + techType + " because it is already being tracked");
				return false;
			}
			if (instance.tracked.Count >= Mod.GetMaxPins())
			{
				//Logger.Error("Can't start tracking " + techType + " because we are tracking maximum tech");
				return false;
			}

			return true;
		}

		public static bool StartTracking(TechType techType)
		{
			if (!CanTrack(techType))
			{
				return false;
			}

			instance.AddTracker(techType);
			return true;
		}

		public static bool StopTracking(TechType techType)
		{
			if (!IsTracked(techType))
			{
				Logger.Error("Can't stop tracking " + techType + " because it was not tracked");
				return false;
			}

			instance.RemoveTracker(techType);
			return true;
		}

		private void Awake()
		{
			rectTransform = (RectTransform)transform;
			rectTransform.anchorMin = new Vector2(0, 1);
			rectTransform.anchorMax = new Vector2(0, 1);
			rectTransform.pivot = new Vector2(0, 1);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Width);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
			rectTransform.anchoredPosition = new Vector2(20, -20);

			layout = gameObject.AddComponent<VerticalLayoutGroup>();
			layout.spacing = Spacing;
			layout.childAlignment = TextAnchor.UpperRight;
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.padding = new RectOffset(10, 10, 10, 10);
		}

		private void AddTracker(TechType techType)
		{
			tracked.Add(techType);
			BlueprintTrackerEntry.Create(transform, techType);
			LogStatus();
		}

		private void RemoveTracker(TechType techType)
		{
			tracked.Remove(techType);

			foreach (Transform child in transform)
			{
				BlueprintTrackerEntry entry = child.GetComponent<BlueprintTrackerEntry>();
				if (entry != null && entry.techType == techType)
				{
					Destroy(child.gameObject);
					break;
				}
			}

			LogStatus();
		}

		private void LogStatus()
		{
			Logger.Log("Tracking: " + string.Join(", ", tracked.Select(x => x.ToString()).ToArray()));
		}

		private void OnDestroy()
		{
			instance = null;
		}
	}
}
