using Harmony;
using System;
using System.Reflection;

namespace QuitToDesktop
{
	// QMods by qwiso https://github.com/Qwiso/QModManager
	public static class QPatch
	{
		public static void Patch()
		{
			HarmonyInstance harmony = HarmonyInstance.Create("com.QuitToDesktop.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Console.WriteLine("[QuitToDesktop] Patched");
		}
	}
}