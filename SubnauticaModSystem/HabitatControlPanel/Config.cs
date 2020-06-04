using Oculus.Newtonsoft.Json;

namespace HabitatControlPanel
{
	[JsonObject]
	public class Config
	{
		[JsonProperty]
		public bool RequireBatteryToUse { get; set; } = false;
	}
}
