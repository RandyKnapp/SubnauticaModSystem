using System;

namespace DockedVehicleStorageAccess
{
	[Serializable]
	class Config
	{
		public int LockerWidth = 6;
		public int LockerHeight = 8;
		public float CheckVehiclesInterval = 2.0f;
		public float ExtractInterval = 0.25f;
		public float AutosortTransferInterval = 0.25f;
	}
}
