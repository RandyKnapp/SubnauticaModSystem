using System;

namespace BetterScannerBlips
{
	[Serializable]
	class Config
	{
		public float MaxRange = 200.0f;
		public float MaxRangeScale = 0.2f;
		public float CloseRange = 12.0f;
		public float CloseRangeScale = 1.0f;
		public float MinRange = 1.0f;
		public float MinRangeScale = 10.0f;
		public float TextRange = 100.0f;
		public float AlphaOutRange = 150.0f;
		public float MaxAlpha = 1f;
		public float MinAlpha = 0.4f;
		public bool CustomColors = false;
		public string CircleColor = "#00FF00FF";
		public string TextColor = "#00FF00FF";
		public bool ShowDistance = true;
		public bool NoText = false;
		public string ToggleKey = "l";
	}
}
