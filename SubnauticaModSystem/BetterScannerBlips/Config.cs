using System;

namespace BetterScannerBlips
{
	[Serializable]
	public class Config
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

		// Added by DaWrecka; if bUseScreenDistance, then distance is counted as distance on screen from the crosshair, expressed as a fraction of total screen real estate.
		// The maximum such distance is 0.5, because the crosshair is always dead-centre of the screen.
		public bool bUseScreenDistance = false;
		public float MaxScreenRange = 0.5f;
		public float MaxScreenRangeScale = 0.2f;
		public float CloseScreenRange = 0.15f;
		public float CloseScreenRangeScale = 1.0f;
		public float MinScreenRange = 0.1f;
		public float MinScreenRangeScale = 10.0f;
		public float TextScreenRange = 0.2f;
		public float AlphaOutScreenRange = 0.2f;
	}
}
