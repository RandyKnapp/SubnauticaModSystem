using Common.Mod;
using Common.Utility;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	[Serializable]
	public class HabitatControlPanelSaveData
	{
		public TechType PowerCellType = TechType.None;
		public float PowerCellCharge = 0;
	}

	public class HabitatControlPanel : MonoBehaviour, IProtoEventListener, IPowerInterface
	{
		private static readonly HashSet<TechType> CompatibleTech = new HashSet<TechType>
		{
			TechType.PowerCell,
			TechType.PrecursorIonPowerCell
		};
		private const string SlotName = "PowerCellCharger1";
		public static readonly Color ScreenContentColor = new Color32(188, 254, 254, 255);

		private bool initialized;
		private Constructable constructable;
		private Equipment equipment;
		private HabitatControlPanelSaveData saveData;
		private IBattery battery;
		private PowerRelay connectedRelay;

		public ChildObjectIdentifier equipmentRoot;

		[SerializeField]
		private Image background;
		[SerializeField]
		private GameObject powerCellSlot;
		[SerializeField]
		private GameObject powerCellMesh;
		[SerializeField]
		private GameObject ionPowerCellMesh;
		[SerializeField]
		private BoxCollider powerCellTrigger;
		[SerializeField]
		private BatteryIndicator batteryIndicator;

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
		}

		private void Update()
		{
			if (!initialized && constructable._constructed && transform.parent != null)
			{
				Initialize();
			}

			if (!initialized || !constructable._constructed)
			{
				return;
			}

			PositionBatteryIndicator();
		}
		
		private void Initialize()
		{
			Logger.Log("Initialize");

			background.gameObject.SetActive(true);
			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Background.png"));

			Destroy(transform.Find("mesh").gameObject);

			var handTarget = powerCellTrigger.gameObject.AddComponent<GenericHandTarget>();
			handTarget.onHandHover = new HandTargetEvent();
			handTarget.onHandClick = new HandTargetEvent();
			handTarget.onHandHover.AddListener(OnPowerCellHandHover);
			handTarget.onHandClick.AddListener(OnPowerCellHandClick);

			equipment = new Equipment(gameObject, equipmentRoot.transform);
			equipment.SetLabel("Habitat Power");
			equipment.isAllowedToAdd = new IsAllowedToAdd(IsAllowedToAdd);
			equipment.onEquip += OnEquip;
			equipment.onUnequip += OnUnequip;
			equipment.AddSlot(SlotName);

			if (saveData != null)
			{
				if (saveData.PowerCellType != TechType.None)
				{
					var powerCell = CraftData.InstantiateFromPrefab(saveData.PowerCellType, false);
					var battery = powerCell.GetComponent<IBattery>();
					battery.charge = saveData.PowerCellCharge;

					equipment.AddItem(SlotName, new InventoryItem(powerCell.GetComponent<Pickupable>()));
					powerCell.SetActive(false);
				}
				saveData = null;
			}

			base.InvokeRepeating("UpdateConnection", 0, 1);

			UpdatePowerCell();
			initialized = true;
		}

		private void UpdateConnection()
		{
			PowerRelay relay = PowerSource.FindRelay(transform);
			if (relay != null && relay != connectedRelay)
			{
				if (connectedRelay != null)
				{
					connectedRelay.RemoveInboundPower(this);
				}
				connectedRelay = relay;
				connectedRelay.AddInboundPower(this);
			}
			else
			{
				connectedRelay = null;
			}
		}

		private void OnUnequip(string slot, InventoryItem item)
		{
			Logger.Log("Unequip " + slot + ":" + item.item.GetTechType());
			UpdatePowerCell();
		}

		private void OnEquip(string slot, InventoryItem item)
		{
			Logger.Log("Equip " + slot + ":" + item.item.GetTechType());
			UpdatePowerCell();
		}

		private void UpdatePowerCell()
		{
			var equippedPowerCell = equipment.GetItemInSlot(SlotName);
			powerCellMesh.SetActive(equippedPowerCell != null && equippedPowerCell.item.GetTechType() == TechType.PowerCell);
			ionPowerCellMesh.SetActive(equippedPowerCell != null && equippedPowerCell.item.GetTechType() == TechType.PrecursorIonPowerCell);

			batteryIndicator.SetBattery(equippedPowerCell?.item);

			battery = equippedPowerCell?.item.GetComponent<IBattery>();
		}

		private void OnPowerCellHandHover(HandTargetEventData eventData)
		{
			HandReticle main = HandReticle.main;
			main.SetIcon(HandReticle.IconType.Hand);

			var secondText = "No Power Cell";
			var powerCell = equipment.GetItemInSlot(SlotName);
			if (powerCell != null)
			{
				var battery = powerCell.item.GetComponent<IBattery>();
				secondText = string.Format("Power {0}%", Mathf.RoundToInt((battery.charge / battery.capacity) * 100));
			}
			main.SetInteractTextRaw("Power Cell", secondText);
		}

		private void OnPowerCellHandClick(HandTargetEventData eventData)
		{
			PDA pda = Player.main.GetPDA();
			if (!pda.isInUse)
			{
				Inventory.main.SetUsedStorage(equipment, false);
				pda.Open(PDATab.Inventory, transform, null, 4f);
			}
		}

		private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
		{
			return pickupable != null && CompatibleTech.Contains(pickupable.GetTechType());
		}

		public float GetPower()
		{
			return battery == null || battery.charge < 1 ? 0 : battery.charge;
		}

		public float GetMaxPower()
		{
			return battery == null ? 0 : battery.capacity;
		}

		public bool ModifyPower(float amount, out float modified)
		{
			modified = 0f;
			if (battery == null)
			{
				return false;
			}

			bool result;
			if (amount >= 0f)
			{
				result = (amount <= battery.capacity - battery.charge);
				modified = Mathf.Min(amount, battery.capacity - battery.charge);
				battery.charge += modified;
			}
			else
			{
				result = (battery.charge >= -amount);
				if (GameModeUtils.RequiresPower())
				{
					modified = -Mathf.Min(-amount, this.battery.charge);
					this.battery.charge += modified;
				}
				else
				{
					modified = amount;
				}
			}
			return result;
		}

		public bool HasInboundPower(IPowerInterface powerInterface)
		{
			return false;
		}

		public bool GetInboundHasSource(IPowerInterface powerInterface)
		{
			return false;
		}

		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			Logger.Log("OnProtoSerialize = " + saveDataFile);
			saveData = CreateSaveData();
			if (!Directory.Exists(GetSaveDataDir()))
			{
				Directory.CreateDirectory(GetSaveDataDir());
			}
			string fileContents = JsonConvert.SerializeObject(saveData, Formatting.Indented);
			Logger.Log("File Contents=" + fileContents);
			File.WriteAllText(saveDataFile, fileContents);
		}

		private HabitatControlPanelSaveData CreateSaveData()
		{
			HabitatControlPanelSaveData saveData = new HabitatControlPanelSaveData();

			var item = equipment.GetItemInSlot(SlotName);
			if (item != null)
			{
				saveData.PowerCellType = item.item.GetTechType();
				saveData.PowerCellCharge = item.item.GetComponent<IBattery>().charge;
			}

			return saveData;
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			Logger.Log("OnProtoDeserialize = " + saveDataFile);
			if (File.Exists(saveDataFile))
			{
				string fileContents = File.ReadAllText(saveDataFile);
				Logger.Log("File Contents=" + fileContents);
				saveData = JsonConvert.DeserializeObject<HabitatControlPanelSaveData>(fileContents);
			}
			else
			{
				saveData = new HabitatControlPanelSaveData();
			}
		}

		private string GetSaveDataDir()
		{
			return Path.Combine(ModUtils.GetSaveDataDirectory(), "HabitatControlPanel");
		}

		public string GetSaveDataPath()
		{
			var prefabIdentifier = GetComponent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			var saveFile = Path.Combine(GetSaveDataDir(), id + ".json");
			return saveFile;
		}

		public void PositionBatteryIndicator()
		{
			var t = batteryIndicator.transform;
			var amount = 1f;

			if (Input.GetKeyDown(KeyCode.Keypad8))
			{
				t.localPosition += new Vector3(0, amount, 0);
				PrintBatteryIndicator();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad5))
			{
				t.localPosition += new Vector3(0, -amount, 0);
				PrintBatteryIndicator();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				t.localPosition += new Vector3(amount, 0, 0);
				PrintBatteryIndicator();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad4))
			{
				t.localPosition += new Vector3(-amount, 0, 0);
				PrintBatteryIndicator();
			}
			/*else if (Input.GetKeyDown(KeyCode.Keypad1))
			{
				t.localPosition += new Vector3(0, 0, amount);
				PrintBatteryIndicator();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad7))
			{
				t.localPosition += new Vector3(0, 0, -amount);
				PrintBatteryIndicator();
			}*/

			/*var scaleAmount = 0.01f;
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				bc.size += new Vector3(scaleAmount, scaleAmount, scaleAmount);
				PrintBatteryIndicator();
			}
			else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				bc.size -= new Vector3(scaleAmount, scaleAmount, scaleAmount);
				PrintBatteryIndicator();
			}*/
		}

		private void PrintBatteryIndicator()
		{
			var t = batteryIndicator.transform as RectTransform;
			Logger.Log("batteryIndicator p=" + t.anchoredPosition);
		}



		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = HabitatControlPanel.GetPrefab,
				techType = (TechType)CustomTechType.HabitatControlPanel,
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/HabitatControlPanel",
				displayString = "Habitat Control Panel",
				tooltip = "TODO TOOLTIP",
				techTypeKey = CustomTechType.HabitatControlPanel.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(Mod.GetAssetPath("BlueprintIcon.png"))),
				recipe = new List<CustomIngredient>
				{
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 1
					},
					new CustomIngredient() {
						techType = TechType.ComputerChip,
						amount = 1
					},
					new CustomIngredient() {
						techType = TechType.WiringKit,
						amount = 1
					}
				}
			});
		}

		public static GameObject GetPrefab()
		{
			Logger.Log("GetPrefab for HabitatControlPanel");
			GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/PictureFrame");
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			prefab.name = "HabitatControlPanel";

			GameObject.Destroy(prefab.GetComponent<PictureFrame>());
			var screen = prefab.transform.Find("Screen").gameObject;
			screen.transform.localEulerAngles = new Vector3(0, 0, 90);
			screen.SetActive(false);
			var mesh = prefab.transform.Find("mesh").gameObject;
			mesh.transform.localEulerAngles = new Vector3(0, 0, 90);
			var trigger = prefab.transform.Find("Trigger").gameObject;
			GameObject.DestroyImmediate(trigger.GetComponent<GenericHandTarget>());

			GameObject powerCellSlotPrefab = GetPowerCellSlotModel();
			GameObject powerCellSlot = GameObject.Instantiate(powerCellSlotPrefab);
			Destroy(powerCellSlotPrefab);
			powerCellSlot.transform.SetParent(prefab.transform, false);
			powerCellSlot.transform.localPosition = new Vector3(0.44f, -0.7f, -0.12f);
			powerCellSlot.transform.localEulerAngles = new Vector3(61, 180, 0);

			powerCellSlot.SetActive(false);
			var sky = powerCellSlot.AddComponent<SkyApplier>();
			sky.dynamic = true;
			sky.renderers = sky.GetAllComponentsInChildren<MeshRenderer>();
			sky.anchorSky = Skies.BaseInterior;
			powerCellSlot.SetActive(true);

			//GameObject beaconPrefab = Resources.Load<GameObject>("WorldEntities/Tools/Beacon");
			//ModUtils.PrintObject(beaconPrefab);

			var controlPanel = prefab.AddComponent<HabitatControlPanel>();
			controlPanel.powerCellSlot = powerCellSlot;
			controlPanel.powerCellMesh = powerCellSlot.transform.GetChild(1).gameObject;
			controlPanel.ionPowerCellMesh = powerCellSlot.transform.GetChild(2).gameObject;
			controlPanel.background = CreateScreen(prefab.transform);

			var slotGeo = powerCellSlot.transform.GetChild(0).gameObject;
			var collider = slotGeo.AddComponent<BoxCollider>();
			controlPanel.powerCellTrigger = collider;

			var equipmentRoot = new GameObject("EquipmentRoot");
			equipmentRoot.transform.SetParent(prefab.transform, false);
			controlPanel.equipmentRoot = equipmentRoot.AddComponent<ChildObjectIdentifier>();
			equipmentRoot.SetActive(false);

			CreateScreenElements(controlPanel, controlPanel.background.transform);

			ModUtils.PrintObject(prefab);

			return prefab;
		}

		private static Image CreateScreen(Transform parent)
		{
			var canvas = LockerPrefabShared.CreateCanvas(parent);
			canvas.transform.localPosition = new Vector3(0, 0, 0.02f);

			var background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
			var rt = background.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), canvas.transform);
			RectTransformExtensions.SetSize(rt, 178, 298);
			background.transform.localScale = new Vector3(0.01f, 0.01f, 1);

			background.type = Image.Type.Simple;
			background.color = new Color(1, 1, 1);
			background.gameObject.SetActive(false);

			return background;
		}

		private static GameObject GetPowerCellSlotModel()
		{
			GameObject seamothPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth");

			GameObject powerCellSlot = new GameObject("PowerCellSlot");
			var offset = new Vector3(0, 0, 1.72f);

			var model = seamothPrefab.transform.Find("Model");
			var Submersible_SeaMoth = model.Find("Submersible_SeaMoth");
			var Submersible_seaMoth_geo = Submersible_SeaMoth.Find("Submersible_seaMoth_geo");
			var seamoth_power_cell_slot_geo = Submersible_seaMoth_geo.Find("seamoth_power_cell_slot_geo");
			var engine_power_cell_01 = Submersible_seaMoth_geo.Find("engine_power_cell_01");
			var engine_power_cell_ion = Submersible_seaMoth_geo.Find("engine_power_cell_ion");

			GameObject slotGeo = GameObject.Instantiate(seamoth_power_cell_slot_geo.gameObject);
			slotGeo.transform.localPosition += offset;
			slotGeo.transform.SetParent(powerCellSlot.transform, false);
			slotGeo.name = "SlotGeo";

			GameObject powerCell = GameObject.Instantiate(engine_power_cell_01.gameObject);
			powerCell.transform.localPosition += offset;
			powerCell.transform.SetParent(powerCellSlot.transform, false);
			powerCell.name = "PowerCellGeo";
			powerCell.SetActive(false);

			GameObject ionPowerCell = GameObject.Instantiate(engine_power_cell_ion.gameObject);
			ionPowerCell.transform.localPosition += offset;
			ionPowerCell.transform.SetParent(powerCellSlot.transform, false);
			ionPowerCell.name = "IonPowerCellGeo";
			ionPowerCell.SetActive(false);

			return powerCellSlot;
		}

		private static void CreateScreenElements(HabitatControlPanel controlPanel, Transform parent)
		{
			controlPanel.batteryIndicator = BatteryIndicator.Create(controlPanel, parent);
			controlPanel.batteryIndicator.rectTransform.anchoredPosition = new Vector2(32.0f, -76.0f);
		}
	}
}
