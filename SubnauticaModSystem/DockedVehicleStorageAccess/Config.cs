using Oculus.Newtonsoft.Json;

namespace DockedVehicleStorageAccess
{
	[JsonObject]
	internal class Config
	{
		public int LockerWidth { get; set; } = 6;
		public int LockerHeight { get; set; } = 8;
		public float CheckVehiclesInterval { get; set; } = 2.0f;
		public float ExtractInterval { get; set; } = 0.25f;
		public float AutosortTransferInterval { get; set; } = 0.25f;

		[JsonIgnore]
		internal bool UseAutosortMod { get; set; }
	}
}
