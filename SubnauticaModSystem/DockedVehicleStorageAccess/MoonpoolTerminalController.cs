using Common.Mod;
#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
using TMPro;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DockedVehicleStorageAccess
{
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
			Vector2 buttonPositionCenter = new Vector2(0, 0);
			int buttonSpacing = 104;

			Transform parent = GetComponentInChildren<Canvas>().transform;
			Color32 color = new Color32(189, 255, 255, 255);

#if SN1
			Text text = new GameObject("label", typeof(RectTransform)).AddComponent<Text>();
			RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(text.rectTransform, 500, 140);
			text.rectTransform.anchoredPosition = buttonPositionCenter + new Vector2(0, 80);
			text.color = GetComponentInChildren<Text>().color;
			text.text = "Position";
			text.fontSize = Mathf.FloorToInt(GetComponentInChildren<Text>().fontSize * 1.8f);
			text.font = GetComponentInChildren<Text>().font;
			text.alignment = TextAnchor.MiddleCenter;
#elif BZ
			TextMeshProUGUI text = new GameObject("label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
			RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(text.rectTransform, 500, 140);
			text.rectTransform.anchoredPosition = buttonPositionCenter + new Vector2(0, 80);
			text.color = GetComponentInChildren<TextMeshProUGUI>().color;
			text.text = "Position";
			text.fontSize = Mathf.FloorToInt(GetComponentInChildren<TextMeshProUGUI>().fontSize * 1.8f);
			text.font = GetComponentInChildren<TextMeshProUGUI>().font;
			text.alignment = TextAlignmentOptions.Midline;
#endif
			text.raycastTarget = false;

			for (int i = 0; i < 4; i++)
			{
				CheckboxButton button = CheckboxButton.CreateCheckboxNoText(parent, color, 100);
				button.Initialize();
				button.toggled = false;
				button.rectTransform.anchoredPosition = buttonPositionCenter + new Vector2((-1.5f + i) * buttonSpacing, 0);
				var buttonIndex = i;
				button.onToggled = (bool toggled) => {
					SetPosition(buttonIndex);
				};
				positionButtons.Add(button);
			}

			OnProtoDeserialize(null);
		}

		private void Initialize()
		{
			positionIndex = saveData != null ? saveData.Position : 0;
			SetPosition(positionIndex);
		}

		private void SetPosition(int index)
		{
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
			var userStorage = PlatformUtils.main.GetUserStorage();
			userStorage.CreateContainerAsync(Path.Combine(SaveLoadManager.main.GetCurrentSlot(), "DockedVehicleStorageAccess"));

			var saveDataFile = GetSaveDataPath();
			saveData = CreateSaveData();
			ModUtils.Save(saveData, saveDataFile);
		}

		private MoonpoolTerminalSaveData CreateSaveData()
		{
			MoonpoolTerminalSaveData saveData = new MoonpoolTerminalSaveData();

			saveData.Position = positionIndex;

			return saveData;
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			ModUtils.LoadSaveData<MoonpoolTerminalSaveData>(saveDataFile, (data) => {
				saveData = data;
				Initialize();
			});
		}

		public string GetSaveDataPath()
		{
			var prefabIdentifier = GetComponentInParent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			var saveFile = Path.Combine("DockedVehicleStorageAccess", id + ".json");
			return saveFile;
		}
	}
}
