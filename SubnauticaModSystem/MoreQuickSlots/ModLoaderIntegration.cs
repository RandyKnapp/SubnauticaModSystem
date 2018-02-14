using Harmony;
using System;
using System.Reflection;
//using UM4SN;
using UnityEngine;

namespace MoreQuickSlots
{
	// Injector
	public static class InjectorPatcher
	{
		public static void Patch()
		{
			Mod.Patch("Subnautica_Data\\Managed");
		}
	}

	// QMods by qwiso https://github.com/Qwiso/QModManager
	public static class QPatch
	{
		public static void Patch()
		{
			Mod.Patch("QMods\\MoreQuickSlots");
		}
	}

	// Nexus ModLoader by newman55 https://www.nexusmods.com/subnautica/mods/13/
	/*public class Controller
	{
		public static string dir { get { return Controller.entry.path; } }
		public static ModLoader.ModEntry entry;

		public static void Load(ModLoader.ModEntry obj = null)
		{
			Logger.Log("Mod Loaded by Nexus ModLoader (" + obj.path + ")");
			Controller.entry = obj;
			Mod.Patch("Subnautica_Data\\Mods\\MoreQuickSlots");
		}
	}*/

	// UM4SN by nesrak1 https://github.com/nesrak1/UM4SN
	/*public class UM4SNPatcher : SubnauticaMod
	{
		public override void OnEnable()
		{
			Logger.Log("Mod Loaded by UM4SN");
			Mod.Patch("SNUnityMod");
		}
	}*/

	// SubnauticaModLoader by dumbdiscord https://github.com/dumbdiscord/SubnauticaModLoader
	// ERROR: Currently this modloader is targetting .NET 4.6 and is incompatible
	/*public class CableWayPatcher : ModEntryPoint
	{
		public override void Patch()
		{
			Logger.Log("Mod Loaded by SubnauticaModLoader by dumbdiscord");
			Mod.Patch("Mods\\MoreQuickSlots");
		}
	}*/
}