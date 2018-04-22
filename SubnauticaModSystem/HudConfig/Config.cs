using Common.Mod;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HudConfig
{
	[Serializable]
	public class ConfigEntry
	{
		public string Name;
		public float Scale = 1;
		public float XOffset = 0;
		public float YOffset = 0;
	}

	[Serializable]
	public class Config
	{
		public List<ConfigEntry> HudElements;
	}
}
