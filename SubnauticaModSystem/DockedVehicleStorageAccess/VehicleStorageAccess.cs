using Common.Mod;
using Common.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Newtonsoft.Json;
using System;
using System.Reflection;
using System.IO;

#if USE_AUTOSORT
using AutosortLockers;
#endif

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
#if USE_AUTOSORT
		private static readonly FieldInfo AutosortLocker_container = typeof(AutosortLocker).GetField("container", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

		private bool initialized;
		VehicleStorageAccessSaveData saveData;
		private bool extractingItems;
		private Constructable constructable;
		private StorageContainer container;
		private SubRoot subRoot;
		private List<VehicleDockingBay> dockingBays = new List<VehicleDockingBay>();
		private List<Vehicle> vehicles = new List<Vehicle>();

#if USE_AUTOSORT
		private bool transferringToAutosorter;
		private List<AutosortLocker> autosorters = new List<AutosortLocker>();
#endif

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

#if USE_AUTOSORT
		[SerializeField]
		private CheckboxButton autosortCheckbox;
#endif

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = gameObject.GetComponent<StorageContainer>();
			subRoot = gameObject.GetComponentInParent<SubRoot>();
		}

		private IEnumerator Start()
		{
			while (true)
			{
				if (initialized && constructable.constructed && enableCheckbox.toggled)
				{
					GetDockingBays();
					yield return TryExtractItems();
#if USE_AUTOSORT
					if (!extractingItems)
					{
						yield return new WaitForSeconds(Mod.config.AutosortTransferInterval);
						yield return TryMoveToAutosorter();
					}
#endif
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

#if USE_AUTOSORT
			autosorters = subRoot.GetComponentsInChildren<AutosortLocker>().ToList();
#endif
		}

		private void AddDockingBayListeners()
		{
			foreach (var dockingBay in dockingBays)
			{
				dockingBay.onDockedChanged += OnDockedVehicleChanged;
			}
		}

		private void RemoveDockingBayListeners()
		{
			foreach (var dockingBay in dockingBays)
			{
				dockingBay.onDockedChanged -= OnDockedVehicleChanged;
			}
		}

		private void UpdateDockedVehicles()
		{
			vehicles.Clear();
			foreach (var dockingBay in dockingBays)
			{
				var vehicle = dockingBay.GetDockedVehicle();
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
			Dictionary<string, int> extractionResults = new Dictionary<string, int>();

			List<Vehicle> localVehicles = vehicles.ToList();
			foreach (var vehicle in localVehicles)
			{
				var vehicleName = vehicle.GetName();
				extractionResults[vehicleName] = 0;
				var vehicleContainers = vehicle.gameObject.GetComponentsInChildren<StorageContainer>().Select((x) => x.container).ToList();
				vehicleContainers.AddRange(GetSeamothStorage(vehicle));
				bool couldNotAdd = false;
				foreach (var vehicleContainer in vehicleContainers)
				{
					foreach (var item in vehicleContainer.ToList())
					{
						if (!enableCheckbox.toggled)
						{
							break;
						}

						if (container.container.HasRoomFor(item.item))
						{
							var success = container.container.AddItem(item.item);
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
				using (var e = seamoth.modules.GetEquipment())
				{
					while (e.MoveNext())
					{
						var module = e.Current.Value;
						if (module == null || module.item == null)
						{
							continue;
						}

						var container = module.item.GetComponent<SeamothStorageContainer>();
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
			List<string> messageEntries = new List<string>();

			foreach (var entry in extractionResults)
			{
				messageEntries.Add(entry.Key + " x" + entry.Value);
			}

			string message = string.Format("Extracted items from vehicle{0}: {1}", messageEntries.Count > 0 ? "s" : "", string.Join(", ", messageEntries.ToArray()));
			ErrorMessage.AddDebug(message);
		}

#if USE_AUTOSORT
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
			foreach (var item in items)
			{
				foreach (var autosorter in autosorters)
				{
					if (!enableCheckbox.toggled || !autosortCheckbox.toggled)
					{
						break;
					}

					var aContainer = GetAutosorterContainer(autosorter);
					if (aContainer.container.HasRoomFor(item.item))
					{
						var success = aContainer.container.AddItem(item.item);
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
#endif

		private void UpdateText()
		{
			var dockingBayCount = dockingBays.Count;
			if (subRoot is BaseRoot)
			{
				text.text = dockingBayCount > 0 ? ("Moonpools: " + dockingBayCount) : "No Moonpools";
			}
			else
			{
				text.text = "Cyclops Docking Bay";
			}

#if USE_AUTOSORT
			autosortCheckbox.isEnabled = autosorters.Count > 0;
			text.text += autosorters.Count == 0 ? "\nNo Autosorters" : "";
#endif

			int seamothCount = 0;
			int exosuitCount = 0;
			foreach (var vehicle in vehicles)
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
#if USE_AUTOSORT
			else if (autosorters.Count == 0 && transferringToAutosorter)
			{
				text.text += "\n\n<color=lime>TRANSFERRING...</color>";
			}
#endif
			else
			{
				text.text += "\n\n<color=lime>READY</color>";
			}
		}

#if USE_AUTOSORT
		private StorageContainer GetAutosorterContainer(AutosortLocker autosorter)
		{
			return (StorageContainer)AutosortLocker_container.GetValue(autosorter);
		}
#endif

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

			container.enabled = ShouldEnableContainer();

			UpdateText();
		}

		private bool ShouldEnableContainer()
		{
#if USE_AUTOSORT
			return !enableCheckbox.pointerOver && !autosortCheckbox.pointerOver;
#else
			return !enableCheckbox.pointerOver;
#endif
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

#if USE_AUTOSORT
			autosortCheckbox.toggled = saveData != null ? saveData.Autosort : true;
			autosortCheckbox.Initialize();
#endif

			subRoot = gameObject.GetComponentInParent<SubRoot>();
			GetDockingBays();
			UpdateDockedVehicles();
			UpdateText();

			initialized = true;
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

		private VehicleStorageAccessSaveData CreateSaveData()
		{
			VehicleStorageAccessSaveData saveData = new VehicleStorageAccessSaveData();

			saveData.Enabled = enableCheckbox.toggled;

#if USE_AUTOSORT
			saveData.Autosort = autosortCheckbox.toggled;
#else
			saveData.Autosort = true;
#endif

			return saveData;
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			var saveDataFile = GetSaveDataPath();
			if (File.Exists(saveDataFile))
			{
				string fileContents = File.ReadAllText(saveDataFile);
				saveData = JsonConvert.DeserializeObject<VehicleStorageAccessSaveData>(fileContents);
			}
			else
			{
				saveData = new VehicleStorageAccessSaveData();
			}
		}

		private string GetSaveDataDir()
		{
			return Path.Combine(ModUtils.GetSaveDataDirectory(), "DockedVehicleStorageAccess");
		}

		public string GetSaveDataPath()
		{
			var prefabIdentifier = GetComponent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			var saveFile = Path.Combine(GetSaveDataDir(), id + ".json");
			return saveFile;
		}



		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = VehicleStorageAccess.GetPrefab,
				techType = (TechType)CustomTechType.DockedVehicleStorageAccess,
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/DockedVehicleStorageAccess",
				displayString = "Docked Vehicle Storage Access",
				tooltip = "Wall locker that extracts items from any docked vehicle in the moonpool.",
				techTypeKey = CustomTechType.DockedVehicleStorageAccess.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(Mod.GetAssetPath("StorageAccess.png"))),
				recipe = new List<CustomIngredient> {
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 2
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
			GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			prefab.name = "VehicleStorageAccess";

			var container = prefab.GetComponent<StorageContainer>();
			container.width = Mod.config.LockerWidth;
			container.height = Mod.config.LockerHeight;
			container.container.Resize(Mod.config.LockerWidth, Mod.config.LockerHeight);

			var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = new Color(0, 0, 1);
			}

			var storageAccess = prefab.AddComponent<VehicleStorageAccess>();

			storageAccess.textPrefab = GameObject.Instantiate(prefab.GetComponentInChildren<Text>());
			var label = prefab.FindChild("Label");
			DestroyImmediate(label);

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
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

#if USE_AUTOSORT
			storageAccess.autosortCheckbox = CheckboxButton.CreateCheckbox(storageAccess.background.transform, PrimaryColor, storageAccess.textPrefab, "Autosort");
			storageAccess.autosortCheckbox.transform.localPosition = new Vector3(0, -104 + 19);
#endif

			storageAccess.enableCheckbox = CheckboxButton.CreateCheckbox(storageAccess.background.transform, PrimaryColor, storageAccess.textPrefab, "Enabled");
			storageAccess.enableCheckbox.transform.localPosition = new Vector3(0, -104);

			storageAccess.background.gameObject.SetActive(false);

			return prefab;
		}
	}
}
