#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
using SMLHelper.V2.Json;
#endif

namespace DockedVehicleStorageAccess
{
#if !BELOWZERO
	[JsonObject]
	internal class Config
#else
	internal class Config : ConfigFile
#endif
	{
		public int LockerWidth { get; set; } = 6;
		public int LockerHeight { get; set; } = 8;
		public float CheckVehiclesInterval { get; set; } = 2.0f;
		public float ExtractInterval { get; set; } = 0.25f;
		public float AutosortTransferInterval { get; set; } = 0.25f;

#if !BELOWZERO
		[JsonIgnore]
#endif
		internal bool UseAutosortMod { get; set; }
	}
}
