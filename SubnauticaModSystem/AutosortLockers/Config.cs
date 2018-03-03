using System;

namespace AutosortLockers
{
	[Serializable]
	class Config
	{
		public bool EasyBuild = false;
		public float SortInterval = 1.0f;
		public bool ShowAllItems = false;
		public int AutosorterWidth = 5;
		public int AutosorterHeight = 6;
		public int ReceptacleWidth = 6;
		public int ReceptacleHeight = 8;
	}
}
