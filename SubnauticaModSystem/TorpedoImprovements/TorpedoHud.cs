using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TorpedoImprovements
{
	class TorpedoHud : MonoBehaviour
	{
		public RectTransform rectTransform { get => transform as RectTransform; }

		public int slotID;

		private Image image;

		private void Start()
		{
			image = gameObject.AddComponent<Image>();
		}

		private void Update()
		{
			SeaMoth seamoth = Player.main?.GetVehicle() as SeaMoth;

			Vector2[] positions = { new Vector2(-100, 0), new Vector2(100, 0), new Vector2(-150, 100), new Vector2(150, 100) };
			Color[] colors = { Color.red, Color.yellow, Color.magenta, Color.cyan };
			image.color = colors[slotID];
			rectTransform.anchoredPosition = positions[slotID];
		}
	}
}
