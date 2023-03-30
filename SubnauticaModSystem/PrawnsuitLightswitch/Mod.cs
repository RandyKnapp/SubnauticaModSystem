using HarmonyLib;
using BepInEx;
using System;
using System.Reflection;
using SMLHelper.V2.Handlers;

namespace PrawnsuitLightswitch
{
	[BepInPlugin(GUID, PluginName, VersionString)]
	public class Mod : BaseUnityPlugin
	{
		public static Config config;

		private const string PluginName = "PrawnsuitLightswitch";
		private const string VersionString = "1.0.3";
		private const string GUID = "com.PrawnsuitLightswitch.mod";

		private void Awake()
		{
			Harmony harmony = new Harmony(GUID);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			config = OptionsPanelHandler.Main.RegisterModOptions<Config>();
		}
	}
}