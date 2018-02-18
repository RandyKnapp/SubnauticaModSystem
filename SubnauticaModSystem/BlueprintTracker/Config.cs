using System;

namespace BlueprintTracker
{
	[Serializable]
	class Config
	{
		public int MaxPinnedBlueprints = 4;
		public string Position = "TopRight";
		public int CornerOffsetX = 20;
		public int CornerOffsetY = 20;
		public float TrackerScale = 1.0f;
		public bool ShowWhilePiloting = false;
	}
}
