using QModManager.API.ModLoading;

namespace HabitatControlPanel
{
	[QModCore]
	public static class QPatch
	{
		[QModPatch]
		public static void Patch()
		{
			Mod.Patch("QMods/HabitatControlPanel");
		}
	}
}