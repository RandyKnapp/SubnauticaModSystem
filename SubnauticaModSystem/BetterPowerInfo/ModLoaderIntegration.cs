﻿using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace BetterPowerInfo
{
	// QMods by qwiso https://github.com/Qwiso/QModManager
	public static class QPatch
	{
		public static void Patch()
		{
			Mod.Patch("QMods/BetterPowerInfo");
		}
	}
}