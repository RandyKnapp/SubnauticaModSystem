using BlueprintTracker.Utility;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlueprintTracker
{
	class BlueprintTrackerPdaEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public TechType techType;

		private PinButton addPinButton;
		private PinButton removePinButton;
		private bool hover;

		public void Awake()
		{
			addPinButton = CreateButton("AddPin", PinButton.Mode.Add, OnAddPinButtonClicked);
			removePinButton = CreateButton("RemovePin", PinButton.Mode.Remove, OnRemovePinButtonClicked);

			BlueprintTracker.onTrackingChanged += Refresh;

			Refresh();
		}

		public void OnDestroy()
		{
			BlueprintTracker.onTrackingChanged -= Refresh;
		}

		private void Refresh()
		{
			bool isTracked = BlueprintTracker.IsTracked(techType);
			bool canTrack = BlueprintTracker.CanTrack(techType);

			if (isTracked)
			{
				addPinButton.gameObject.SetActive(false);
				removePinButton.gameObject.SetActive(true);
			}
			else
			{
				removePinButton.gameObject.SetActive(false);
				addPinButton.gameObject.SetActive(hover && canTrack);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hover = true;
			Refresh();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			hover = false;
			Refresh();
		}

		private void OnAddPinButtonClicked()
		{
			BlueprintTracker.StartTracking(techType);
			Refresh();
		}

		private void OnRemovePinButtonClicked()
		{
			BlueprintTracker.StopTracking(techType);
			Refresh();
		}

		public PinButton CreateButton(string name, PinButton.Mode mode, Action onClick)
		{
			var button = new GameObject(name, typeof(RectTransform)).AddComponent<PinButton>();
			button.transform.SetParent(transform, false);

			button.SetMode(mode);
			button.onClick += onClick;

			return button;
		}
	}
}
