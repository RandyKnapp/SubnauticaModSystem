using Harmony;
using System;
using System.Reflection;

namespace LongLockerNames
{
	// QMods by qwiso https://github.com/Qwiso/QModManager
	public static class QPatch
	{
		public static void Patch()
		{
			HarmonyInstance harmony = HarmonyInstance.Create("com.LongLockerNames.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Console.WriteLine("[LongLockerNames] Patched");
		}
	}
}