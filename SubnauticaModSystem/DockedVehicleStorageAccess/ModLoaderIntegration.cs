using QModManager.API.ModLoading;

namespace DockedVehicleStorageAccess
{
	// https://github.com/Qwiso/QModManager
	[QModCore]
	public static class QPatch
	{
		[QModPatch]
		public static void Patch()
		{
			Mod.Patch("QMods/DockedVehicleStorageAccessSML");
		}
	}
}