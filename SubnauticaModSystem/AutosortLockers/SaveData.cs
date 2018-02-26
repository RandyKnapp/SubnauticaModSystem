using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutosortLockers
{
[Serializable]
public class SaveDataEntry
{
	public string Id;
	public List<AutosorterFilter> FilterData;
}

[Serializable]
public class SaveData
{
	public List<SaveDataEntry> Entries = new List<SaveDataEntry>();
}
}
