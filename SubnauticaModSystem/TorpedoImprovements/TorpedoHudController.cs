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
		private TorpedoHudIcon hudIcon;

		private void Awake()
		{
			for (int i = 0; i < 4; ++i)
			{
				var hud = new GameObject("Torpedo" + i, typeof(RectTransform)).AddComponent<TorpedoHud>();
				hud.transform.SetParent(transform, false);
				hud.Initialize(i);
				huds.Add(hud);
			}

			hudIcon = new GameObject("HudIcon", typeof(RectTransform)).AddComponent<TorpedoHudIcon>();
			RectTransformExtensions.SetParams(hudIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
			hudIcon.gameObject.SetActive(false);
			hudIcon.rectTransform.anchoredPosition = new Vector2(0, -60);
		}

		private void Update()
		{
			SeaMoth seamoth = Player.main?.GetVehicle() as SeaMoth;

			hudIcon.gameObject.SetActive(false);
			for (int i = 0; i < 4; ++i)
			{
				var hud = huds[i];
				var torpedoStorage = seamoth?.GetStorageInSlot(i, TechType.SeamothTorpedoModule);
				hud.gameObject.SetActive(torpedoStorage != null);

				if (hud.IsActive && torpedoStorage != null && torpedoStorage.count > 0)
				{
					var primaryTorpedoType = hud.GetNextTorpedoType(seamoth);
					hudIcon.gameObject.SetActive(primaryTorpedoType != TechType.None);
					if (primaryTorpedoType != TechType.None)
					{
						hudIcon.SetTechType(primaryTorpedoType);
						hudIcon.SetCount(torpedoStorage.GetCount(primaryTorpedoType));
					}
				}
			}

			if (seamoth != null && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
			{
				seamoth.GetComponent<PrimaryTorpedo>().Next();
				foreach (var hud in huds)
				{
					hud.Reset(seamoth);
				}
			}
		}
	}
}
