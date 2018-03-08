using Common.Mod;
using Common.Utility;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace HabitatControlPanel
{
	public class HabitatControlPanel : MonoBehaviour, IProtoEventListener
	{
		private static readonly HashSet<TechType> CompatibleTech = new HashSet<TechType>
		{
			TechType.PowerCell,
			TechType.PrecursorIonPowerCell
		};
		private const string SlotName = "PowerCellCharger1";

		private bool initialized;
		private Constructable constructable;
		private Equipment equipment;
		private Dictionary<string, string> serializedSlots;

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

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			//AlternativeSerializer.RegisterCustomSerializer<HabitatControlPanel>((int)CustomTechType.HabitatControlPanel, this);
		}

		private void OnDestroy()
		{

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

			//PositionTrigger();
			//DrawBoxCollider();
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

			if (serializedSlots != null)
			{
				Dictionary<string, InventoryItem> items = StorageHelper.ScanItems(equipmentRoot.transform);
				equipment.RestoreEquipment(serializedSlots, items);
				serializedSlots = null;
			}

			UpdatePowerCellMesh();
			initialized = true;
		}

		private void OnUnequip(string slot, InventoryItem item)
		{
			Logger.Log("Unequip " + slot + ":" + item.item.GetTechType());
			UpdatePowerCellMesh();
		}

		private void OnEquip(string slot, InventoryItem item)
		{
			Logger.Log("Equip " + slot + ":" + item.item.GetTechType());
			UpdatePowerCellMesh();
		}

		private void UpdatePowerCellMesh()
		{
			var equippedPowerCell = equipment.GetItemInSlot(SlotName);
			powerCellMesh.SetActive(equippedPowerCell != null && equippedPowerCell.item.GetTechType() == TechType.PowerCell);
			ionPowerCellMesh.SetActive(equippedPowerCell != null && equippedPowerCell.item.GetTechType() == TechType.PrecursorIonPowerCell);
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

		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			Logger.Log("OnProtoSerialize = " + saveDataFile);
			serializedSlots = equipment.SaveEquipment();
			if (!Directory.Exists(GetSaveDataDir()))
			{
				Directory.CreateDirectory(GetSaveDataDir());
			}
			string fileContents = JsonConvert.SerializeObject(serializedSlots, Formatting.Indented);
			Logger.Log("File Contents=" + fileContents);
			File.WriteAllText(saveDataFile, fileContents);
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			Logger.Log("OnProtoDeserialize = " + saveDataFile);
			if (File.Exists(saveDataFile))
			{
				string fileContents = File.ReadAllText(saveDataFile);
				Logger.Log("File Contents=" + fileContents);
				serializedSlots = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
			}
			else
			{
				serializedSlots = new Dictionary<string, string>();
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

		/*public object Deserialize(object obj, ProtoReader reader, ProtobufSerializerPrecompiled model)
		{
			Logger.Log("Deserialize(" + string.Join(",", new string[] { obj.ToString(), reader.ToString(), model.ToString() }));
			return obj;
		}

		public void Serialize(object obj, ProtoWriter writer, ProtobufSerializerPrecompiled model)
		{
			Logger.Log("Serialize(" + string.Join(",", new string[] { obj.ToString(), writer.ToString(), model.ToString() }));
		}*/

		/*public void PositionTrigger()
		{
			var t = powerCellTrigger.transform;
			var bc = powerCellTrigger;
			var amount = 0.01f;

			if (Input.GetKeyDown(KeyCode.Keypad8))
			{
				t.localPosition += new Vector3(0, amount, 0);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad5))
			{
				t.localPosition += new Vector3(0, -amount, 0);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				t.localPosition += new Vector3(amount, 0, 0);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad4))
			{
				t.localPosition += new Vector3(-amount, 0, 0);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad1))
			{
				t.localPosition += new Vector3(0, 0, amount);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad7))
			{
				t.localPosition += new Vector3(0, 0, -amount);
				PrintBoxCollider();
			}

			var scaleAmount = 0.01f;
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				bc.size += new Vector3(scaleAmount, scaleAmount, scaleAmount);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				bc.size -= new Vector3(scaleAmount, scaleAmount, scaleAmount);
				PrintBoxCollider();
			}
		}

		private void PrintBoxCollider()
		{
			var bc = powerCellTrigger;
			var t = powerCellTrigger.transform;
			Logger.Log("Trigger p=" + t.localPosition + " s=" + bc.bounds.size);
		}

		private void DrawBoxCollider()
		{
			var bc = powerCellTrigger;
			var t = bc.transform;
			var verts = new Vector3[8];
			var worldTransform = bc.transform.localToWorldMatrix;
			var storedRotation = bc.transform.rotation;
			bc.transform.rotation = Quaternion.identity;

			var extents = bc.bounds.extents;
			verts[0] = worldTransform.MultiplyPoint3x4(extents);
			verts[1] = worldTransform.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, extents.z));
			verts[2] = worldTransform.MultiplyPoint3x4(new Vector3(extents.x, extents.y, -extents.z));
			verts[3] = worldTransform.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, -extents.z));
			verts[4] = worldTransform.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, extents.z));
			verts[5] = worldTransform.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, extents.z));
			verts[6] = worldTransform.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, -extents.z));
			verts[7] = worldTransform.MultiplyPoint3x4(-extents);

			bc.transform.rotation = storedRotation;

			DrawLine(verts, 0, 1);
			DrawLine(verts, 1, 2);
			DrawLine(verts, 2, 3);
			DrawLine(verts, 3, 0);

			DrawLine(verts, 0, 4);
			DrawLine(verts, 1, 5);
			DrawLine(verts, 2, 6);
			DrawLine(verts, 3, 7);

			DrawLine(verts, 4, 5);
			DrawLine(verts, 5, 6);
			DrawLine(verts, 6, 7);
			DrawLine(verts, 7, 4);
		}

		private void DrawLine(Vector3[] verts, int a, int b)
		{
			Debug.DrawLine(verts[a], verts[b], Color.green, 0);
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

			/*GameObject batteryInput = GameObject.Instantiate(seamothPrefab.transform.Find("BatteryInput").gameObject);
			batteryInput.transform.localPosition += offset;
			batteryInput.transform.SetParent(powerCellSlot.transform, false);*/

			/*GameObject batterySlot = GameObject.Instantiate(seamothPrefab.transform.Find("BatterySlot").gameObject);
			batterySlot.transform.localPosition += offset;
			batterySlot.transform.SetParent(powerCellSlot.transform, false);*/

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
	}
}
