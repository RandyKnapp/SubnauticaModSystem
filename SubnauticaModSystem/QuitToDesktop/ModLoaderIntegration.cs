using HarmonyLib;
using System;
using System.Reflection;
using QModManager.API.ModLoading;
namespace QuitToDesktop
{
	// QMods by qwiso https://github.com/Qwiso/QModManager
	[QModCore]
	public static class QPatch
	{
		private static Assembly myAssembly = Assembly.GetExecutingAssembly();
		[QModPatch]
		public static void Patch()
		{
			Harmony.CreateAndPatchAll(myAssembly, $"RandyKnapp_{myAssembly.GetName().Name}");

			Console.WriteLine("[QuitToDesktop] Patched");
		}
	}
}