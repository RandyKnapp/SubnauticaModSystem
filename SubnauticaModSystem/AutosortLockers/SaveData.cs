using Common.Mod;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutosortLockers
{
	[Serializable]
	public class SaveDataEntry
	{
		public string Id;
		public List<AutosorterFilter> FilterData = new List<AutosorterFilter>();
		public string Label = "";
		public SerializableColor LabelColor = Color.white;
		public SerializableColor IconColor = Color.white;
		public SerializableColor OtherTextColor = Color.white;
		public SerializableColor ButtonsColor = Color.white;
		public SerializableColor LockerColor = new Color(0.3f, 0.3f, 0.3f);
	}

	[Serializable]
	public class SaveData
	{
		public List<SaveDataEntry> Entries = new List<SaveDataEntry>();
	}
}