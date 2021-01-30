using HarmonyLib;
using System;
using System.Reflection;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using QuitToDesktop.Configuration;
namespace QuitToDesktop
{
	// QMods by qwiso https://github.com/Qwiso/QModManager
	[QModCore]
	public static class QPatch
	{
		internal static Config Config { get; private set; }
		private static Assembly myAssembly = Assembly.GetExecutingAssembly();
		[QModPatch]
		public static void Patch()
		{
			Config = OptionsPanelHandler.RegisterModOptions<Config>();
			IngameMenuHandler.RegisterOnSaveEvent(Config.Save);

			Harmony.CreateAndPatchAll(myAssembly, "com.QuitToDesktop.mod");

			Console.WriteLine("[QuitToDesktop] Patched");
		}
	}
}
