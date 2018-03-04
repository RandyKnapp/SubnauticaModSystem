using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DockedVehicleStorageAccess
{
[Serializable]
public class SaveDataEntry
{
	public string Id;
}

[Serializable]
public class SaveData
{
	public List<SaveDataEntry> Entries = new List<SaveDataEntry>();
}
}
