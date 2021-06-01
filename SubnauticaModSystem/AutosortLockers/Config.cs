#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif

namespace AutosortLockers
{
	[JsonObject]
	public class Config
	{
		public bool EasyBuild { get; set; } = false;
		public float SortInterval { get; set; } = 1.0f;
		public bool ShowAllItems { get; set; } = false;
		public int AutosorterWidth { get; set; } = 5;
		public int AutosorterHeight { get; set; } = 6;
		public int ReceptacleWidth { get; set; } = 6;
		public int ReceptacleHeight { get; set; } = 8;
		public int StandingReceptacleWidth { get; set; } = 6;
		public int StandingReceptacleHeight { get; set; } = 8;
	}
}
