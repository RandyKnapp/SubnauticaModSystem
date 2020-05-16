using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Mod;
using Common.Utility;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;

namespace DockedVehicleStorageAccess
{
	[Serializable]
	public class VehicleStorageAccessSaveData
	{
		public bool Enabled = true;
		public bool Autosort = true;
	}

	public class VehicleStorageAccess : MonoBehaviour, IProtoEventListener
	{
		private static readonly Color PrimaryColor = new Color32(66, 134, 244, 255);
		private static readonly Color HiddenColor = new Color32(66, 134, 244, 20);
		private static readonly Type AutosortLockerType = Type.GetType("AutosortLockers.AutosortLocker, AutosortLockersSML", false, false);
		private static readonly FieldInfo AutosortLocker_container = AutosortLockerType?.GetField("container", BindingFlags.NonPublic | BindingFlags.Instance);

		private bool initialized;
		private VehicleStorageAccessSaveData saveData;
		private bool extractingItems;
		private Constructable constructable;
		private StorageContainer container;
		private SubRoot subRoot;
		private List<VehicleDockingBay> dockingBays = new List<VehicleDockingBay>();
		private List<Vehicle> vehicles = new List<Vehicle>();

		private bool transferringToAutosorter;
		private List<Component> autosorters = new List<Component>();


		[SerializeField]
		private Text textPrefab;
		[SerializeField]
		private Image background;
		[SerializeField]
		private Image icon;
		[SerializeField]
		private Text text;
		[SerializeField]
		private Image seamothIcon;
		[SerializeField]
		private Text seamothCountText;
		[SerializeField]
		private Image exosuitIcon;
		[SerializeField]
		private Text exosuitCountText;
		[SerializeField]
		private CheckboxButton enableCheckbox;

		[SerializeField]
		private CheckboxButton autosortCheckbox;


		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = this.gameObject.GetComponent<StorageContainer>();
			subRoot = this.gameObject.GetComponentInParent<SubRoot>();
		}

		private IEnumerator Start()
		{
			while (true)
			{
				if (initialized && constructable.constructed && enableCheckbox.toggled)
				{
					GetDockingBays();
					yield return TryExtractItems();

					if (Mod.config.UseAutosortMod)
					{
						if (!extractingItems)
						{
							yield return new WaitForSeconds(Mod.config.AutosortTransferInterval);
							yield return TryMoveToAutosorter();
						}
					}
				}
				yield return new WaitForSeconds(Mod.config.CheckVehiclesInterval);
			}
		}

		private void OnDisable()
		{
			RemoveDockingBayListeners();
			StopAllCoroutines();
		}

		private void GetDockingBays()
		{
			RemoveDockingBayListeners();
			dockingBays = subRoot.GetComponentsInChildren<VehicleDockingBay>().ToList();
			AddDockingBayListeners();

			UpdateDockedVehicles();

			if (Mod.config.UseAutosortMod)
			{
				autosorters = subRoot.GetComponentsInChildren(AutosortLockerType).ToList();
			}
		}

		private void AddDockingBayListeners()
		{
			foreach (VehicleDockingBay dockingBay in dockingBays)
			{
				dockingBay.onDockedChanged += OnDockedVehicleChanged;
			}
		}

		private void RemoveDockingBayListeners()
		{
			foreach (VehicleDockingBay dockingBay in dockingBays)
			{
				dockingBay.onDockedChanged -= OnDockedVehicleChanged;
			}
		}

		private void UpdateDockedVehicles()
		{
			vehicles.Clear();
			foreach (VehicleDockingBay dockingBay in dockingBays)
			{
				Vehicle vehicle = dockingBay.GetDockedVehicle();
				if (vehicle != null)
				{
					vehicles.Add(vehicle);
				}
			}
		}

		private void OnDockedVehicleChanged()
		{
			UpdateDockedVehicles();
			StartCoroutine(TryExtractItems());
		}

		private IEnumerator TryExtractItems()
		{
			if (extractingItems)
			{
				yield break;
			}
			if (!enableCheckbox.toggled)
			{
				yield break;
			}

			bool extractedAnything = false;
			var extractionResults = new Dictionary<string, int>();

			var localVehicles = vehicles.ToList();
			foreach (Vehicle vehicle in localVehicles)
			{
				string vehicleName = vehicle.GetName();
				extractionResults[vehicleName] = 0;
				var vehicleContainers = vehicle.gameObject.GetComponentsInChildren<StorageContainer>().Select((x) => x.container).ToList();
				vehicleContainers.AddRange(GetSeamothStorage(vehicle));
				bool couldNotAdd = false;
				foreach (ItemsContainer vehicleContainer in vehicleContainers)
				{
					foreach (InventoryItem item in vehicleContainer.ToList())
					{
						if (!enableCheckbox.toggled)
						{
							break;
						}

						if (container.container.HasRoomFor(item.item))
						{
							InventoryItem success = container.container.AddItem(item.item);
							if (success != null)
							{
								extractionResults[vehicleName]++;
								if (extractingItems == false)
								{
									ErrorMessage.AddDebug("Extracting items from vehicle storage...");
								}
								extractedAnything = true;
								extractingItems = true;
								yield return new WaitForSeconds(Mod.config.ExtractInterval);
							}
							else
							{
								couldNotAdd = true;
								break;
							}
						}
						else
						{
							couldNotAdd = true;
							break;
						}
					}

					if (couldNotAdd || !enableCheckbox.toggled)
					{
						break;
					}
				}
			}

			if (extractedAnything)
			{
				NotifyExtraction(extractionResults);
			}
			extractingItems = false;
		}

		private List<ItemsContainer> GetSeamothStorage(Vehicle seamoth)
		{
			var results = new List<ItemsContainer>();
			if (seamoth is SeaMoth && seamoth.modules != null)
			{
				using (Dictionary<string, InventoryItem>.Enumerator e = seamoth.modules.GetEquipment())
				{
					while (e.MoveNext())
					{
						InventoryItem module = e.Current.Value;
						if (module == null || module.item == null)
						{
							continue;
						}

						SeamothStorageContainer container = module.item.GetComponent<SeamothStorageContainer>();
						if (container != null && !container.gameObject.name.Contains("Torpedo"))
						{
							results.Add(container.container);
						}
					}
				}
			}
			return results;
		}

		private void NotifyExtraction(Dictionary<string, int> extractionResults)
		{
			var messageEntries = new List<string>();

			foreach (KeyValuePair<string, int> entry in extractionResults)
			{
				messageEntries.Add(entry.Key + " x" + entry.Value);
			}

			string message = string.Format("Extracted items from vehicle{0}: {1}", messageEntries.Count > 0 ? "s" : "", string.Join(", ", messageEntries.ToArray()));
			ErrorMessage.AddDebug(message);
		}

		// Exclusive for Autosort integration
		private IEnumerator TryMoveToAutosorter()
		{
			if (autosorters.Count == 0)
			{
				yield break;
			}
			if (!autosortCheckbox.toggled || !enableCheckbox.toggled)
			{
				yield break;
			}
			if (transferringToAutosorter)
			{
				yield break;
			}

			var items = container.container.ToList();
			bool couldNotAdd = false;
			int itemsTransferred = 0;
			foreach (InventoryItem item in items)
			{
				foreach (Component autosorter in autosorters)
				{
					if (!enableCheckbox.toggled || !autosortCheckbox.toggled)
					{
						break;
					}

					var aContainer = (StorageContainer)AutosortLocker_container.GetValue(autosorter);

					if (aContainer.container.HasRoomFor(item.item))
					{
						InventoryItem success = aContainer.container.AddItem(item.item);
						if (success != null)
						{
							itemsTransferred++;
							if (!transferringToAutosorter)
							{
								ErrorMessage.AddDebug("Transferring items to Autosorter...");
							}
							transferringToAutosorter = true;
						}
						else
						{
							couldNotAdd = true;
							break;
						}
					}
					else
					{
						couldNotAdd = true;
						break;
					}
					yield return new WaitForSeconds(Mod.config.AutosortTransferInterval);
				}

				if (couldNotAdd || !enableCheckbox.toggled || !autosortCheckbox.toggled)
				{
					break;
				}
			}

			if (itemsTransferred > 0)
			{
				ErrorMessage.AddDebug("Transfer complete");
			}
			transferringToAutosorter = false;
		}

		private void UpdateText()
		{
			int dockingBayCount = dockingBays.Count;
			if (subRoot is BaseRoot)
			{
				text.text = dockingBayCount > 0 ? ("Moonpools: " + dockingBayCount) : "No Moonpools";
			}
			else
			{
				text.text = "Cyclops Docking Bay";
			}

			if (Mod.config.UseAutosortMod)
			{
				autosortCheckbox.isEnabled = autosorters.Count > 0;
				text.text += autosorters.Count == 0 ? "\nNo Autosorters" : "";
			}

			int seamothCount = 0;
			int exosuitCount = 0;
			foreach (Vehicle vehicle in vehicles)
			{
				seamothCount += (vehicle is SeaMoth ? 1 : 0);
				exosuitCount += (vehicle is Exosuit ? 1 : 0);
			}

			seamothCountText.text = seamothCount > 1 ? "x" + seamothCount : "";
			exosuitCountText.text = exosuitCount > 1 ? "x" + exosuitCount : "";

			seamothIcon.color = seamothCount > 0 ? PrimaryColor : HiddenColor;
			exosuitIcon.color = exosuitCount > 0 ? PrimaryColor : HiddenColor;

			if (!enableCheckbox.toggled)
			{
				text.text += "\n\n<color=red>DISABLED</color>";
			}
			else if (extractingItems)
			{
				text.text += "\n\n<color=lime>EXTRACTING...</color>";
			}
			else if (Mod.config.UseAutosortMod && autosorters.Count == 0 && transferringToAutosorter)
			{
				text.text += "\n\n<color=lime>TRANSFERRING...</color>";
			}
			else
			{
				text.text += "\n\n<color=lime>READY</color>";
			}
		}

		private void Update()
		{
			if (!initialized && constructable._constructed && this.transform.parent != null)
			{
				Initialize();
			}

			if (!initialized || !constructable._constructed)
			{
				return;
			}

			container.enabled = ShouldEnableContainer();

			UpdateText();
		}

		private bool ShouldEnableContainer()
		{
			if (Mod.config.UseAutosortMod)
				return !enableCheckbox.pointerOver && !autosortCheckbox.pointerOver;
			else
				return !enableCheckbox.pointerOver;
		}

		private void Initialize()
		{
			background.gameObject.SetActive(true);
			icon.gameObject.SetActive(true);
			text.gameObject.SetActive(true);
			text.supportRichText = true;

			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("LockerScreen.png"));
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Receptacle.png"));
			seamothIcon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Seamoth.png"));
			exosuitIcon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Exosuit.png"));

			enableCheckbox.toggled = saveData != null ? saveData.Enabled : true;
			enableCheckbox.Initialize();

			if (Mod.config.UseAutosortMod)
			{
				autosortCheckbox.toggled = saveData != null ? saveData.Autosort : true;
				autosortCheckbox.Initialize();
			}

			subRoot = this.gameObject.GetComponentInParent<SubRoot>();
			GetDockingBays();
			UpdateDockedVehicles();
			UpdateText();

			initialized = true;
		}

		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
			UserStorage userStorage = PlatformUtils.main.GetUserStorage();
			userStorage.CreateContainerAsync(Path.Combine(SaveLoadManager.main.GetCurrentSlot(), "DockedVehicleStorageAccess"));

			string saveDataFile = GetSaveDataPath();
			saveData = CreateSaveData();
			ModUtils.Save(saveData, saveDataFile);
		}

		private VehicleStorageAccessSaveData CreateSaveData()
		{
			var saveData = new VehicleStorageAccessSaveData
			{
				Enabled = enableCheckbox.toggled,
				Autosort = Mod.config.UseAutosortMod && autosortCheckbox.toggled
			};

			return saveData;
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			string saveDataFile = GetSaveDataPath();
			ModUtils.LoadSaveData<VehicleStorageAccessSaveData>(saveDataFile, (data) =>
			{
				saveData = data;
				initialized = false;
			});
		}

		public string GetSaveDataPath()
		{
			PrefabIdentifier prefabIdentifier = GetComponent<PrefabIdentifier>();
			string id = prefabIdentifier.Id;

			string saveFile = Path.Combine("DockedVehicleStorageAccess", id + ".json");
			return saveFile;
		}

		internal class VehicleStorageAccessBuildable : Buildable
		{
			public VehicleStorageAccessBuildable()
				: base("DockedVehicleStorageAccess",
					  "Docked Vehicle Storage Access",
					  "Wall locker that extracts items from any docked vehicle in the moonpool or cyclops.")
			{
			}

			public override TechGroup GroupForPDA => TechGroup.InteriorModules;

			public override TechCategory CategoryForPDA => TechCategory.InteriorModule;

			public override GameObject GetGameObject()
			{
				GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
				var prefab = GameObject.Instantiate(originalPrefab);

				prefab.name = "VehicleStorageAccess";

				StorageContainer container = prefab.GetComponent<StorageContainer>();
				container.width = Mod.config.LockerWidth;
				container.height = Mod.config.LockerHeight;
				container.container.Resize(Mod.config.LockerWidth, Mod.config.LockerHeight);

				MeshRenderer[] meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer meshRenderer in meshRenderers)
				{
					meshRenderer.material.color = new Color(0, 0, 1);
				}

				VehicleStorageAccess storageAccess = prefab.AddComponent<VehicleStorageAccess>();

				storageAccess.textPrefab = GameObject.Instantiate(prefab.GetComponentInChildren<Text>());
				GameObject label = prefab.FindChild("Label");
				DestroyImmediate(label);

				Canvas canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
				storageAccess.background = LockerPrefabShared.CreateBackground(canvas.transform);
				storageAccess.icon = LockerPrefabShared.CreateIcon(storageAccess.background.transform, PrimaryColor, 15);
				storageAccess.text = LockerPrefabShared.CreateText(storageAccess.background.transform, storageAccess.textPrefab, PrimaryColor, -40, 10, "");
				storageAccess.seamothIcon = LockerPrefabShared.CreateIcon(storageAccess.background.transform, PrimaryColor, 80);
				storageAccess.seamothCountText = LockerPrefabShared.CreateText(storageAccess.background.transform, storageAccess.textPrefab, PrimaryColor, 55, 10, "none");
				storageAccess.exosuitIcon = LockerPrefabShared.CreateIcon(storageAccess.background.transform, PrimaryColor, 80);
				storageAccess.exosuitCountText = LockerPrefabShared.CreateText(storageAccess.background.transform, storageAccess.textPrefab, PrimaryColor, 55, 10, "none");

				storageAccess.seamothIcon.rectTransform.anchoredPosition += new Vector2(-23, 0);
				storageAccess.seamothCountText.rectTransform.anchoredPosition += new Vector2(-23, 0);
				storageAccess.exosuitIcon.rectTransform.anchoredPosition += new Vector2(23, 0);
				storageAccess.exosuitCountText.rectTransform.anchoredPosition += new Vector2(23, 0);

				if (Mod.config.UseAutosortMod)
				{
					storageAccess.autosortCheckbox = CheckboxButton.CreateCheckbox(storageAccess.background.transform, PrimaryColor, storageAccess.textPrefab, "Autosort");
					storageAccess.autosortCheckbox.transform.localPosition = new Vector3(0, -104 + 19);
				}

				storageAccess.enableCheckbox = CheckboxButton.CreateCheckbox(storageAccess.background.transform, PrimaryColor, storageAccess.textPrefab, "Enabled");
				storageAccess.enableCheckbox.transform.localPosition = new Vector3(0, -104);

				storageAccess.background.gameObject.SetActive(false);

				return prefab;
			}

			protected override TechData GetBlueprintRecipe()
			{
				return new TechData
				{
					craftAmount = 1,
					Ingredients =
					{
						new Ingredient(TechType.Titanium, 1),
						new Ingredient(TechType.WiringKit, 1)
					}
				};
			}

			protected override Atlas.Sprite GetItemSprite()
			{
				return SMLHelper.V2.Utility.ImageUtils.LoadSpriteFromFile(Mod.GetAssetPath("StorageAccess.png"));
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			var dvsa = new VehicleStorageAccessBuildable();
			dvsa.Patch();
		}
	}
}
