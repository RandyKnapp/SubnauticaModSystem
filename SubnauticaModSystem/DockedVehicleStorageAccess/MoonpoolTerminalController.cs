using Common.Mod;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DockedVehicleStorageAccess
{
	//console.transform.localPosition = new Vector3(4.96f, 1.4f, 3.23f);
	//console.transform.localEulerAngles = new Vector3(0, 42.5f, 0);
	[Serializable]
	class MoonpoolTerminalSaveData
	{
		public int Position = 0;
	}

	class MoonpoolTerminalController : MonoBehaviour, IProtoEventListener
	{
		private static Vector3[] Positions = {
			new Vector3(4.96f, 1.4f, 3.23f),
			new Vector3(-4.96f, 1.4f, 3.23f),
			new Vector3(-4.96f, 1.4f, -3.23f),
			new Vector3(4.96f, 1.4f, -3.23f)
		};
		private static float[] Angles = {
			42.5f,
			-42.5f,
			180 + 42.5f,
			180 - 42.5f,
		};

		private MoonpoolTerminalSaveData saveData;
		private int positionIndex;
		private List<CheckboxButton> positionButtons = new List<CheckboxButton>();

		private void Awake()
		{
			OnProtoDeserialize(null);

			var buttonPositionCenter = new Vector2(0, 0);
			var buttonSpacing = 104;
			positionIndex = saveData != null ? saveData.Position : 0;
			var parent = GetComponentInChildren<Canvas>().transform;
			var color = new Color32(189, 255, 255, 255);

			var text = new GameObject("label", typeof(RectTransform)).AddComponent<Text>();
			RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(text.rectTransform, 500, 140);
			text.rectTransform.anchoredPosition = buttonPositionCenter + new Vector2(0, 80);
			text.color = GetComponentInChildren<Text>().color;
			text.text = "Position";
			text.fontSize = Mathf.FloorToInt(GetComponentInChildren<Text>().fontSize * 1.8f);
			text.font = GetComponentInChildren<Text>().font;
			text.alignment = TextAnchor.MiddleCenter;
			text.raycastTarget = false;

			for (int i = 0; i < 4; ++i)
			{
				var button = CheckboxButton.CreateCheckboxNoText(parent, color, 100);
				button.Initialize();
				button.toggled = positionIndex == i;
				button.rectTransform.anchoredPosition = buttonPositionCenter + new Vector2((-1.5f + i) * buttonSpacing, 0);
				var buttonIndex = i;
				button.onToggled = (bool toggled) => {
					SetPosition(buttonIndex);
				};
				positionButtons.Add(button);
			}

			SetPosition(positionIndex);

			ModUtils.PrintObject(gameObject);
		}

		private void SetPosition(int index)
		{
			Logger.Log("SetPosition " + index);
			if (index < 0 && index >= Positions.Length)
			{
				index = 0;
			}
			positionIndex = index;

			gameObject.transform.localPosition = Positions[index];
			gameObject.transform.localEulerAngles = new Vector3(0, Angles[index], 0);

			for (int i = 0; i < 4; ++i)
			{
				var button = positionButtons[i];
				button.toggled = index == i;
			}

			OnProtoSerialize(null);
		}

		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
			Logger.Log("Serialize");
			var saveDataFile = GetSaveDataPath();
			saveData = CreateSaveData();
			if (!Directory.Exists(GetSaveDataDir()))
			{
				Directory.CreateDirectory(GetSaveDataDir());
			}
			string fileContents = JsonConvert.SerializeObject(saveData, Formatting.Indented);
			File.WriteAllText(saveDataFile, fileContents);
		}

		private MoonpoolTerminalSaveData CreateSaveData()
		{
			MoonpoolTerminalSaveData saveData = new MoonpoolTerminalSaveData();

			saveData.Position = positionIndex;

			return saveData;
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			Logger.Log("Deserialize");
			var saveDataFile = GetSaveDataPath();
			if (File.Exists(saveDataFile))
			{
				string fileContents = File.ReadAllText(saveDataFile);
				saveData = JsonConvert.DeserializeObject<MoonpoolTerminalSaveData>(fileContents);
			}
			else
			{
				saveData = new MoonpoolTerminalSaveData();
			}
		}

		private string GetSaveDataDir()
		{
			return Path.Combine(ModUtils.GetSaveDataDirectory(), "DockedVehicleStorageAccess");
		}

		public string GetSaveDataPath()
		{
			var prefabIdentifier = GetComponentInParent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			var saveFile = Path.Combine(GetSaveDataDir(), id + ".json");
			return saveFile;
		}
	}
}
