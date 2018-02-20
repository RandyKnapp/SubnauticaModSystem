using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LongLockerNames.Patches
{
	[HarmonyPatch(typeof(StorageContainer))]
	[HarmonyPatch("OnHandHover")]
	class StorageContainer_OnHandHover_Patch
	{
		private static void Postfix()
		{
			// TODO
		}

		/*public void OnHandHover(GUIHand hand)
		{
			if (!base.enabled)
			{
				return;
			}
			Constructable component = base.gameObject.GetComponent<Constructable>();
			if (!component || component.constructed)
			{
				HandReticle.main.SetInteractText(this.hoverText, (!this.IsEmpty()) ? string.Empty : "Empty");
				HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}*/
	}
}
