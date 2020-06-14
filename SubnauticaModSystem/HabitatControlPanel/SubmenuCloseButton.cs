using UnityEngine;
using UnityEngine.EventSystems;

namespace HabitatControlPanel
{
    class SubmenuCloseButton : MonoBehaviour, IPointerClickHandler
	{
		public HabitatControlPanel target;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (target != null)
			{
				target.CloseSubmenu();
			}
		}
	}
}
