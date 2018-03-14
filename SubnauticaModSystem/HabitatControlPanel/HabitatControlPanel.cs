using Common.Mod;
using Common.Utility;
using HabitatControlPanel.Secret;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	[Serializable]
	public class HabitatControlPanelSaveData
	{
		public TechType PowerCellType = TechType.None;
		public float PowerCellCharge = 0;
		public bool PingVisible = true;
		public string PingLabel = "Habitat";
		public int PingColorIndex = 0;
		public int PingIcon = (int)PingType.Beacon;
		public SerializableColor ExteriorColor = Color.white;
		public SerializableColor InteriorColor = Color.white;
	}

	public class HabitatControlPanel : MonoBehaviour, IProtoEventListener, IPowerInterface, IObstacle
	{
		private static readonly HashSet<TechType> CompatibleTech = new HashSet<TechType>
		{
			TechType.PowerCell,
			TechType.PrecursorIonPowerCell
		};
		private const string SlotName = "PowerCellCharger1";
		public static readonly Color ScreenContentColor = new Color32(188, 254, 254, 255);
		private const string InitialHabitatLabel = "Habitat";
		private const int MaxDistance = 3;

		private bool initialized;
		private Constructable constructable;
		private Equipment equipment;
		private HabitatControlPanelSaveData saveData;
		private PowerRelay connectedRelay;
		private string habitatLabel = InitialHabitatLabel;
		private int pingType = 0;
		private Color exteriorColor;
		private Color interiorColor;
		private PingInstance ping;
		private GameObject currentSubMenu;

		public ChildObjectIdentifier equipmentRoot;

		[SerializeField]
		private GameObject pictureFrameMesh;
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
		[SerializeField]
		private HabitatNameController habitatNameController;
		[SerializeField]
		private BeaconSettings beaconSettings;
		[SerializeField]
		private BeaconColorSettings beaconColorSettings;
		[SerializeField]
		private Image scrim;
		[SerializeField]
		private BeaconColorPicker beaconColorPicker;
		[SerializeField]
		private SecretButton secretButton;
		[SerializeField]
		private SecretGame game;
		[SerializeField]
		private HabitatColorSettings habitatExteriorColorSettings;
		[SerializeField]
		private HabitatColorSettings habitatInteriorColorSettings;
		[SerializeField]
		private HabitatColorPicker habitatColorPicker;
		[SerializeField]
		private BeaconIconSettings beaconIconSettings;
		[SerializeField]
		private BeaconIconPicker beaconIconPicker;

		public string HabitatLabel
		{
			get => habitatLabel;
			set
			{
				habitatLabel = value;
				ping.SetLabel(value);
				PingManager.NotifyRename(ping);
			}
		}

		public bool BeaconVisible
		{
			get
			{
				return ping != null && ping.visible;
			}
			internal set
			{
				ping.visible = value;
				PingManager.NotifyVisible(ping);
			}
		}

		public int BeaconColorIndex
		{
			get
			{
				return ping.colorIndex;
			}
			internal set
			{
				ping.colorIndex = value;
				ping.SetColor(value);
				PingManager.NotifyColor(ping);
			}
		}

		public Color ExteriorColor
		{
			get => exteriorColor;
			set
			{
				exteriorColor = value;
				ColorBaseExterior(exteriorColor);
				habitatExteriorColorSettings.SetColor(exteriorColor);

				var subRoot = GetComponentInParent<SubRoot>();
				var habitatControlPanels = subRoot.GetComponentsInChildren<HabitatControlPanel>();
				foreach (var hcp in habitatControlPanels)
				{
					hcp.exteriorColor = value;
					hcp.habitatExteriorColorSettings.SetColor(exteriorColor);
				}
			}
		}

		public Color InteriorColor
		{
			get => interiorColor;
			set
			{
				interiorColor = value;
				ColorBaseInterior(interiorColor);
				habitatInteriorColorSettings.SetColor(interiorColor);

				var subRoot = GetComponentInParent<SubRoot>();
				var habitatControlPanels = subRoot.GetComponentsInChildren<HabitatControlPanel>();
				foreach (var hcp in habitatControlPanels)
				{
					hcp.interiorColor = value;
					hcp.habitatInteriorColorSettings.SetColor(interiorColor);
				}
			}
		}

		public PingType BeaconPingType
		{
			get => (PingType)pingType;
			set
			{
				pingType = (int)value;
				ping.pingType = value;
				beaconIconSettings.SetValue(value, BeaconColorIndex);
				PingManager.NotifyVisible(ping);
			}
		}

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			constructable.allowedInSub = false;
			constructable.allowedInBase = true;

			if (ping != null)
			{
				DestroyImmediate(ping);
				ping = null;
			}
			ping = GetComponent<PingInstance>();
			if (ping != null)
			{
				DestroyImmediate(ping);
				ping = null;
			}
		}

		private void Update()
		{
			pictureFrameMesh.SetActive(!constructable._constructed);
			if (initialized)
			{
				background.gameObject.SetActive(constructable._constructed);
				powerCellSlot.SetActive(constructable._constructed);

				var item = equipment.GetItemInSlot(SlotName);
				constructable.deconstructionAllowed = item == null;
			}

			if (!initialized && constructable._constructed && transform.parent != null)
			{
				Initialize();
			}

			if (!initialized || !constructable._constructed)
			{
				return;
			}

			UpdateBatteryIndicator();
			UpdatePing();
			UpdateSecret();

			if (Mod.config.RequireBatteryToUse)
			{
				if (currentSubMenu != null && !HasBatteryPower())
				{
					CloseSubmenu();
				}
			}

			UpdateDistanceFromPlayer();

			//PositionStuff(secretButton.gameObject);
		}

		private void UpdateDistanceFromPlayer()
		{
			if (Player.main != null)
			{
				float distSq = (Player.main.transform.position - transform.position).sqrMagnitude;
				bool playerInRange = distSq <= (MaxDistance * MaxDistance);

				if (currentSubMenu != null && !playerInRange)
				{
					CloseSubmenu();
				}
			}
		}

		private void ColorBaseExterior(Color color)
		{
			SubRoot subRoot = gameObject.GetComponentInParent<SubRoot>();
			HashSet<MeshRenderer> allMeshes = new HashSet<MeshRenderer>(subRoot.GetAllComponentsInChildren<MeshRenderer>());
			HashSet<MeshRenderer> exteriorRenderers = new HashSet<MeshRenderer>();

			var includeIf = new string[] { "Exterior", "exterior", "_ext", "Platform", "BaseRoomObservatory" };
			var rejectIf = new string[] { "_int", "glass", "Glass", "InteriorStairs", "ladder", "ground" };
			var forceInclude = new string[] { "ExteriorShell" };

			var exteriorMeshes = allMeshes.Where(x => includeIf.Any(x.gameObject.name.Contains)).ToList();
			exteriorMeshes.RemoveAll(x => rejectIf.Any(x.gameObject.name.Contains));
			exteriorMeshes.AddRange(allMeshes.Where(x => forceInclude.Any(x.gameObject.name.Contains)));

			exteriorRenderers.UnionWith(exteriorMeshes);

			foreach (var meshRenderer in exteriorRenderers)
			{
				meshRenderer.material.SetColor(ShaderPropertyID._Color, color);
			}
		}

		private void ColorBaseInterior(Color color)
		{
			SubRoot subRoot = gameObject.GetComponentInParent<SubRoot>();
			HashSet<MeshRenderer> allMeshes = new HashSet<MeshRenderer>(subRoot.GetAllComponentsInChildren<MeshRenderer>());
			HashSet<MeshRenderer> exteriorRenderers = new HashSet<MeshRenderer>();

			var includeIf = new string[] { "Interior", "interior", "_int" };
			var rejectIf = new string[] { "_ext", "glass", "Glass", "InteriorStairs", "ladder", "stairs", "Stairs", "_Ivy", "ground" };
			var forceInclude = new string[] { };

			var exteriorMeshes = allMeshes.Where(x => includeIf.Any(x.gameObject.name.Contains)).ToList();
			exteriorMeshes.RemoveAll(x => rejectIf.Any(x.gameObject.name.Contains));
			exteriorMeshes.AddRange(allMeshes.Where(x => forceInclude.Any(x.gameObject.name.Contains)));

			exteriorRenderers.UnionWith(exteriorMeshes);

			foreach (var meshRenderer in exteriorRenderers)
			{
				meshRenderer.material.SetColor(ShaderPropertyID._Color, color);
			}
		}

		private void UpdatePing()
		{
			if (ping != null)
			{
				beaconColorSettings.SetColor(ping.colorIndex);
				beaconIconSettings.SetValue(ping.pingType, ping.colorIndex);
			}
		}
		
		private void Initialize()
		{
			background.gameObject.SetActive(true);
			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Background.png"));

			scrim.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Scrim.png"));

			transform.Find("mesh").gameObject.SetActive(false);

			secretButton.onActivate += ToggleGame;
			game.onLoseGame = OnLoseGame;

			var handTarget = powerCellTrigger.gameObject.AddComponent<GenericHandTarget>();
			handTarget.onHandHover = new HandTargetEvent();
			handTarget.onHandClick = new HandTargetEvent();
			handTarget.onHandHover.AddListener(OnPowerCellHandHover);
			handTarget.onHandClick.AddListener(OnPowerCellHandClick);

			equipment = new Equipment(gameObject, equipmentRoot.transform);
			equipment.SetLabel("Habitat Power");
			equipment.isAllowedToAdd = new IsAllowedToAdd(IsAllowedToAdd);
			equipment.onEquip += OnEquipmentChanged;
			equipment.onUnequip += OnEquipmentChanged;
			equipment.AddSlot(SlotName);

			ping = gameObject.AddComponent<PingInstance>();
			ping.SetLabel(InitialHabitatLabel);
			ping.pingType = PingType.Beacon;
			ping.origin = transform;
			HabitatLabel = InitialHabitatLabel;

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

				if (Enum.GetName(typeof(PingType), saveData.PingIcon) == null)
				{
					saveData.PingIcon = (int)PingType.Beacon;
				}
				BeaconPingType = (PingType)saveData.PingIcon;
				BeaconColorIndex = Mathf.Clamp(saveData.PingColorIndex, 0, PingManager.colorOptions.Length - 1);
				BeaconVisible = saveData.PingVisible;
				HabitatLabel = saveData.PingLabel;

				ExteriorColor = saveData.ExteriorColor.ToColor();
				InteriorColor = saveData.InteriorColor.ToColor();
			}
			else
			{
				ExteriorColor = Color.white;
			}
			
			habitatNameController.SetLabel(HabitatLabel);
			beaconSettings.SetInitialValue(ping.visible);
			beaconIconSettings.SetInitialValue(ping.pingType, ping.colorIndex);
			beaconIconSettings.onClick += OnBeaconIconButtonClick;
			beaconColorSettings.SetInitialValue(ping.colorIndex);
			beaconColorSettings.onClick += OnBeaconColorButtonClick;
			habitatExteriorColorSettings.SetInitialValue(ExteriorColor);
			habitatExteriorColorSettings.onClick += OnExteriorColorButtonClick;
			habitatInteriorColorSettings.SetInitialValue(InteriorColor);
			habitatInteriorColorSettings.onClick += OnInteriorColorButtonClick;

			PingManager.NotifyRename(ping);
			PingManager.NotifyColor(ping);
			PingManager.NotifyVisible(ping);

			base.InvokeRepeating("UpdatePowerRelay", 0, 1);
			base.InvokeRepeating("UpdateBaseColor", UnityEngine.Random.Range(0.5f, 1.0f), 3);

			UpdatePowerCell();
			initialized = true;
		}

		private void UpdatePowerRelay()
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

			if (connectedRelay != null)
			{
				connectedRelay.RemoveInboundPower(this);
				connectedRelay.AddInboundPower(this);
			}
		}

		private void UpdateBaseColor()
		{
			ColorBaseExterior(ExteriorColor);
			ColorBaseInterior(InteriorColor);
		}

		private void OnEquipmentChanged(string slot, InventoryItem item)
		{
			UpdatePowerCell();
		}

		private void UpdatePowerCell()
		{
			var equippedPowerCell = equipment.GetItemInSlot(SlotName);
			powerCellMesh.SetActive(equippedPowerCell != null && equippedPowerCell.item.GetTechType() == TechType.PowerCell);
			ionPowerCellMesh.SetActive(equippedPowerCell != null && equippedPowerCell.item.GetTechType() == TechType.PrecursorIonPowerCell);
		}

		private void UpdateBatteryIndicator()
		{
			var equippedPowerCell = equipment.GetItemInSlot(SlotName);
			if (equippedPowerCell == null)
			{
				batteryIndicator.UpdateNoBattery();
			}
			else
			{
				var battery = equippedPowerCell.item.GetComponent<IBattery>();
				batteryIndicator.UpdateBattery(battery.charge, battery.capacity);
			}
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
			var powerCell = equipment.GetItemInSlot(SlotName);
			if (powerCell == null)
			{
				return 0;
			}
			var battery = powerCell.item.GetComponent<IBattery>();
			return battery == null || battery.charge < 1 ? 0 : battery.charge;
		}

		public float GetMaxPower()
		{
			var powerCell = equipment.GetItemInSlot(SlotName);
			if (powerCell == null)
			{
				return 0;
			}
			var battery = powerCell.item.GetComponent<IBattery>();
			return battery == null ? 0 : battery.capacity;
		}

		public bool ModifyPower(float amount, out float modified)
		{
			modified = 0f;
			var powerCell = equipment.GetItemInSlot(SlotName);
			if (powerCell == null)
			{
				return false;
			}
			var battery = powerCell.item.GetComponent<IBattery>();

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
					modified = -Mathf.Min(-amount, battery.charge);
					battery.charge += modified;
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

		public bool CanDeconstruct(out string reason)
		{
			var item = equipment.GetItemInSlot(SlotName);
			reason = "Remove habitat control panel power cell";
			return item == null;
		}

		private void OnBeaconIconButtonClick()
		{
			beaconIconPicker.Initialize(this, ping.pingType);
			OpenSubmenu(beaconIconPicker.gameObject);
		}

		private void OnBeaconColorButtonClick()
		{
			beaconColorPicker.Initialize(this, ping.colorIndex);
			OpenSubmenu(beaconColorPicker.gameObject);
		}

		private void OnExteriorColorButtonClick()
		{
			habitatColorPicker.Initialize(this, ExteriorColor);
			habitatColorPicker.onColorSelect = OnExteriorColorSelect;
			OpenSubmenu(habitatColorPicker.gameObject);
		}

		private void OnExteriorColorSelect(Color color)
		{
			ExteriorColor = color;
		}

		private void OnInteriorColorButtonClick()
		{
			habitatColorPicker.Initialize(this, InteriorColor);
			habitatColorPicker.onColorSelect = OnInteriorColorSelect;
			OpenSubmenu(habitatColorPicker.gameObject);
		}

		private void OnInteriorColorSelect(Color color)
		{
			InteriorColor = color;
		}

		internal void OpenSubmenu(GameObject subMenu)
		{
			currentSubMenu = subMenu;
			currentSubMenu.SetActive(true);
			scrim.gameObject.SetActive(true);
		}

		internal void CloseSubmenu()
		{
			currentSubMenu?.SetActive(false);
			currentSubMenu = null;
			scrim.gameObject.SetActive(false);
		}

		private void UpdateSecret()
		{
			if (Mod.config.RequireBatteryToUse)
			{
				if (!HasBatteryPower() && game.isActiveAndEnabled)
				{
					ShowGame(false);
				}
			}
		}

		private bool CanShowGame()
		{
			return Mod.config.RequireBatteryToUse ? HasBatteryPower() : true;
		}

		private void ToggleGame()
		{
			if (CanShowGame())
			{
				var gameShowing = game.gameObject.activeSelf;
				ShowGame(!gameShowing);
			}
		}

		private void ShowGame(bool show)
		{
			if (show && !CanShowGame())
			{
				return;
			}

			CloseSubmenu();

			game.gameObject.SetActive(show);
			if (show)
			{
				game.StartGame();
			}

			batteryIndicator.gameObject.SetActive(!show);
			habitatNameController.gameObject.SetActive(!show);
			beaconSettings.gameObject.SetActive(!show);
			beaconColorSettings.gameObject.SetActive(!show);
		}

		private bool HasBatteryPower()
		{
			var item = equipment.GetItemInSlot(SlotName);
			return item != null && GetPower() > 0;
		}

		private void OnLoseGame()
		{
			var powerCell = equipment.GetItemInSlot(SlotName);
			if (powerCell != null)
			{
				var battery = powerCell.item.GetComponent<IBattery>();
				battery.charge = 0;
			}
		}

		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			saveData = CreateSaveData();
			if (!Directory.Exists(GetSaveDataDir()))
			{
				Directory.CreateDirectory(GetSaveDataDir());
			}
			string fileContents = JsonConvert.SerializeObject(saveData, Formatting.Indented);
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

			saveData.PingColorIndex = ping.colorIndex;
			saveData.PingLabel = HabitatLabel;
			saveData.PingVisible = ping.visible;
			saveData.PingIcon = (int)BeaconPingType;

			saveData.ExteriorColor = ExteriorColor;
			saveData.InteriorColor = InteriorColor;

			return saveData;
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			if (File.Exists(saveDataFile))
			{
				string fileContents = File.ReadAllText(saveDataFile);
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

		/*public void PositionStuff(GameObject thing)
		{
			var t = thing.transform;
			var amount = 1f;

			if (Input.GetKeyDown(KeyCode.Keypad8))
			{
				t.localPosition += new Vector3(0, amount, 0);
				PrintStuff(thing);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad5))
			{
				t.localPosition += new Vector3(0, -amount, 0);
				PrintStuff(thing);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				t.localPosition += new Vector3(amount, 0, 0);
				PrintStuff(thing);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad4))
			{
				t.localPosition += new Vector3(-amount, 0, 0);
				PrintStuff(thing);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad1))
			{
				t.localPosition += new Vector3(0, 0, amount);
				PrintStuff(thing);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad7))
			{
				t.localPosition += new Vector3(0, 0, -amount);
				PrintStuff(thing);
			}

			//var scaleAmount = 0.01f;
			//if (Input.GetKeyDown(KeyCode.KeypadPlus))
			//{
			//	bc.size += new Vector3(scaleAmount, scaleAmount, scaleAmount);
			//	PrintBatteryIndicator();
			//}
			//else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			//{
			//	bc.size -= new Vector3(scaleAmount, scaleAmount, scaleAmount);
			//	PrintBatteryIndicator();
			//}
		}

		private void PrintStuff(GameObject thing)
		{
			var t = thing.transform as RectTransform;
			Logger.Log(thing.name + " p=" + t.anchoredPosition);
		}*/



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
				tooltip = "Adds a built-in beacon with customizable name and a power cell slot that can power your habitat.",
				techTypeKey = CustomTechType.HabitatControlPanel.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(Mod.GetAssetPath("BlueprintIcon.png"))),
				recipe = new List<CustomIngredient>
				{
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 1
					},
					new CustomIngredient() {
						techType = TechType.CopperWire,
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

			var controlPanel = prefab.AddComponent<HabitatControlPanel>();
			controlPanel.powerCellSlot = powerCellSlot;
			controlPanel.powerCellMesh = powerCellSlot.transform.GetChild(1).gameObject;
			controlPanel.ionPowerCellMesh = powerCellSlot.transform.GetChild(2).gameObject;
			controlPanel.background = CreateScreen(prefab.transform);
			controlPanel.pictureFrameMesh = mesh;

			var slotGeo = powerCellSlot.transform.GetChild(0).gameObject;
			var collider = slotGeo.AddComponent<BoxCollider>();
			controlPanel.powerCellTrigger = collider;

			var equipmentRoot = new GameObject("EquipmentRoot");
			equipmentRoot.transform.SetParent(prefab.transform, false);
			controlPanel.equipmentRoot = equipmentRoot.AddComponent<ChildObjectIdentifier>();
			equipmentRoot.SetActive(false);

			CreateScreenElements(controlPanel, controlPanel.background.transform);

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
			controlPanel.batteryIndicator.rectTransform.anchoredPosition = new Vector2(23, -85);

			controlPanel.habitatNameController = HabitatNameController.Create(controlPanel, parent);
			controlPanel.habitatNameController.rectTransform.anchoredPosition = new Vector2(0, 118);

			controlPanel.beaconSettings = BeaconSettings.Create(controlPanel, parent);
			controlPanel.beaconSettings.rectTransform.anchoredPosition = new Vector2(0, 82);

			controlPanel.beaconIconSettings = BeaconIconSettings.Create(controlPanel, parent);
			controlPanel.beaconIconSettings.rectTransform.anchoredPosition = new Vector2(-25, 63);

			controlPanel.beaconColorSettings = BeaconColorSettings.Create(controlPanel, parent);
			controlPanel.beaconColorSettings.rectTransform.anchoredPosition = new Vector2(-25, 44);

			controlPanel.habitatExteriorColorSettings = HabitatColorSettings.Create(controlPanel, parent, "Exterior Color");
			controlPanel.habitatExteriorColorSettings.rectTransform.anchoredPosition = new Vector2(0, 25);

			controlPanel.habitatInteriorColorSettings = HabitatColorSettings.Create(controlPanel, parent, "Interior Color");
			controlPanel.habitatInteriorColorSettings.rectTransform.anchoredPosition = new Vector2(0, 6);

			controlPanel.secretButton = SecretButton.Create(parent);
			(controlPanel.secretButton.transform as RectTransform).anchoredPosition = new Vector2(63, -126);

			controlPanel.game = SecretGame.Create(parent);
			controlPanel.game.gameObject.SetActive(false);

			var scrim = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
			var rt = scrim.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, 178, 298);
			controlPanel.scrim = scrim;
			controlPanel.scrim.gameObject.SetActive(false);
			var closeButton = controlPanel.scrim.gameObject.AddComponent<SubmenuCloseButton>();
			closeButton.target = controlPanel;

			controlPanel.beaconColorPicker = BeaconColorPicker.Create(parent);
			controlPanel.beaconColorPicker.rectTransform.anchoredPosition = new Vector2(0, 50);
			controlPanel.beaconColorPicker.gameObject.SetActive(false);

			controlPanel.habitatColorPicker = HabitatColorPicker.Create(parent);
			controlPanel.habitatColorPicker.rectTransform.anchoredPosition = new Vector2(0, 50);
			controlPanel.habitatColorPicker.gameObject.SetActive(false);

			controlPanel.beaconIconPicker = BeaconIconPicker.Create(parent);
			controlPanel.beaconIconPicker.rectTransform.anchoredPosition = new Vector2(0, 50);
			controlPanel.beaconIconPicker.gameObject.SetActive(false);
		}
	}
}
