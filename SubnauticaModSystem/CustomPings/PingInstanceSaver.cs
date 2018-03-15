using Common.Mod;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomBeacons
{
	[Serializable]
	class PingInstanceSaveData
	{
		public int PingType = 0;
	}

	class PingInstanceSaver : MonoBehaviour, IProtoEventListener
	{
		private PingInstance ping;
		private string id;

		private void Awake()
		{
			ping = GetComponent<PingInstance>();
			var uniqueIdentifier = GetComponent<PrefabIdentifier>();
			if (uniqueIdentifier != null)
			{
				id = uniqueIdentifier.Id;
			}
			else if (gameObject.name == "EscapePod")
			{
				id = "EscapePod";
			}
			else
			{
				Logger.Error("Created a PingInstance with no uniqueIdentifier (" + gameObject.name + ")");
				Destroy(this);
			}
		}

		private string GetSaveDataDir()
		{
			return Path.Combine(ModUtils.GetSaveDataDirectory(), "CustomBeacons");
		}

		private string GetSaveDataPath()
		{
			var saveFile = Path.Combine(GetSaveDataDir(), id + ".json");
			return saveFile;
		}

		private PingInstanceSaveData CreateSaveData()
		{
			var saveData = new PingInstanceSaveData {
				PingType = (int)ping.pingType
			};

			return saveData;
		}

		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			var saveData = CreateSaveData();
			if (!Directory.Exists(GetSaveDataDir()))
			{
				Directory.CreateDirectory(GetSaveDataDir());
			}
			string fileContents = JsonConvert.SerializeObject(saveData, Formatting.Indented);
			File.WriteAllText(saveDataFile, fileContents);
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			if (File.Exists(saveDataFile))
			{
				string fileContents = File.ReadAllText(saveDataFile);
				var saveData = JsonConvert.DeserializeObject<PingInstanceSaveData>(fileContents);
				OnLoadSaveData(saveData);
			}
		}

		private void OnLoadSaveData(PingInstanceSaveData saveData)
		{
			if (ping != null && saveData != null)
			{
				ping.pingType = (PingType)saveData.PingType;
				PingManager.NotifyColor(ping);
				PingManager.NotifyVisible(ping);
			}
		}
	}
}
