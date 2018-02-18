using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BlueprintTracker
{
	class BlueprintTrackerPdaEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		private static bool once = false;

		public TechType techType;

		public void Awake()
		{
			if (once)
			{
				return;
			}
			once = true;

			Logger.Log("Printing Game Object:");
			Mod.PrintObject(gameObject);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			Logger.Log("OnPointerEnter: " + techType);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Logger.Log("OnPointerExit: " + techType);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Logger.Log("OnPointerClick: " + techType);
		}
	}
}
