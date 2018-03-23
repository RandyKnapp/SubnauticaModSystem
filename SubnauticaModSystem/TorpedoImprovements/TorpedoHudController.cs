using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TorpedoImprovements
{
	class TorpedoHudController : MonoBehaviour
	{
		public RectTransform rectTransform { get => transform as RectTransform; }

		private List<TorpedoHud> huds = new List<TorpedoHud>();

		private void Awake()
		{
			for (int i = 0; i < 4; ++i)
			{
				var hud = new GameObject("Torpedo" + i, typeof(RectTransform)).AddComponent<TorpedoHud>();
				hud.transform.SetParent(transform, false);
				hud.slotID = i;
				huds.Add(hud);
			}
		}

		private void Update()
		{
			SeaMoth seamoth = Player.main?.GetVehicle() as SeaMoth;
			//bool active = seamoth != null;

			for (int i = 0; i < 4; ++i)
			{
				var hud = huds[i];
				var torpedoStorage = seamoth?.GetStorageInSlot(i, TechType.SeamothTorpedoModule);
				hud.gameObject.SetActive(torpedoStorage != null);
			}
		}
	}
}
