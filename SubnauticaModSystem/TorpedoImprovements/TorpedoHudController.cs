using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TorpedoImprovements.Patches;
using UnityEngine;

namespace TorpedoImprovements
{
	class TorpedoHudController : MonoBehaviour
	{
		public RectTransform rectTransform { get => transform as RectTransform; }

		private GameObject root;
		private List<TorpedoHud> huds = new List<TorpedoHud>();
		private TorpedoHudIcon hudIcon;

		private void Awake()
		{
			root = new GameObject("Root");
			root.transform.SetParent(transform, false);

			for (int i = 0; i < 4; ++i)
			{
				var hud = new GameObject("Torpedo" + i, typeof(RectTransform)).AddComponent<TorpedoHud>();
				hud.transform.SetParent(root.transform, false);
				hud.Initialize(i);
				huds.Add(hud);
			}

			hudIcon = new GameObject("HudIcon", typeof(RectTransform)).AddComponent<TorpedoHudIcon>();
			RectTransformExtensions.SetParams(hudIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), root.transform);
			hudIcon.gameObject.SetActive(false);
			hudIcon.rectTransform.anchoredPosition = new Vector2(0, Mod.config.HudIconYOffset);
		}

		private void Update()
		{
			SeaMoth seamoth = null;
			PDA pda = null;
			Player player = Player.main;
			if (player != null)
			{
				seamoth = player.GetVehicle() as SeaMoth;
				pda = player.GetPDA();
			}
			bool showing = seamoth != null && (pda == null || !pda.isInUse);
			if (root.activeSelf != showing)
			{
				root.SetActive(showing);
			}
			if (showing)
			{
				hudIcon.gameObject.SetActive(false);
				for (int i = 0; i < 4; ++i)
				{
					var hud = huds[i];
					var torpedoStorage = seamoth.GetStorageInSlot(i, TechType.SeamothTorpedoModule);
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

				if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
				{
					seamoth.GetComponent<PrimaryTorpedo>().Next();
					foreach (var hud in huds)
					{
						hud.Reset(seamoth);
					}
				}

				if (GameInput.GetButtonDown(GameInput.Button.AltTool))
				{
					for (int i = 0; i < 4; ++i)
					{
						var hud = huds[i];
						if (hud.IsActive)
						{
							seamoth.OpenTorpedoStorageEx(i, null);
						}
					}
				}
			}
		}
	}
}
