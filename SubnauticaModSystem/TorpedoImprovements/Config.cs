using Common.Mod;
using System;
using UnityEngine;

namespace TorpedoImprovements
{
	[Serializable]
	class Config
	{
		public float TorpedoShotCooldown = 3;
		public int TorpedoStorageWidth = 6;
		public int TorpedoStorageHeight = 2;
		public int HudXOffset = 200;
		public int HudXSpacing = -30;
		public int HudYOffset = -480;
		public int HudYSpacing = 70;
		public float HudBackgroundAlpha = 0.9f;
		public float HudIconYOffset = -60;
	}
}
