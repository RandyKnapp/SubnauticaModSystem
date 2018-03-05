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

#if USE_AUTOSORT
using AutosortLockers;
#endif

namespace DockedVehicleStorageAccess
{
	public class VehicleStorageAccess : MonoBehaviour
	{
#if USE_AUTOSORT
		private static readonly FieldInfo AutosortLocker_container = typeof(AutosortLocker).GetField("container", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

		private bool initialized;
		private bool extractingItems;
		private Constructable constructable;
		private StorageContainer container;
		private SubRoot subRoot;
		private List<VehicleDockingBay> dockingBays = new List<VehicleDockingBay>();
		private List<Vehicle> vehicles = new List<Vehicle>();

#if USE_AUTOSORT
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
			while(true)
			{
				if (initialized)
				{
					GetDockingBays();
					yield return TryExtractItems();
				}
				yield return new WaitForSeconds(1.0f);
			}
		}

		private void GetDockingBays()
		{
			foreach (var dockingBay in dockingBays)
			{
				dockingBay.onDockedChanged -= OnDockedVehicleChanged;
			}
			dockingBays = subRoot.GetComponentsInChildren<VehicleDockingBay>().ToList();
			foreach (var dockingBay in dockingBays)
			{
				dockingBay.onDockedChanged += OnDockedVehicleChanged;
			}

			UpdateDockedVehicles();

#if USE_AUTOSORT
			autosorters = subRoot.GetComponentsInChildren<AutosortLocker>().ToList();
#endif
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

			extractingItems = true;
			bool extractedAnything = false;
			Dictionary<string, int> extractionResults = new Dictionary<string, int>();

			List<Vehicle> localVehicles = vehicles.ToList();
			foreach (var vehicle in localVehicles)
			{
				var vName = vehicle.GetName();
				extractionResults[vName] = 0;
				var vContainers = vehicle.gameObject.GetComponentsInChildren<StorageContainer>().Select((x) => x.container).ToList();
				vContainers.AddRange(GetSeamothStorage(vehicle));
				foreach (var vContainer in vContainers)
				{
					foreach (var item in vContainer.ToList())
					{
						if (container.container.HasRoomFor(item.item))
						{
							container.container.AddItem(item.item);
							vContainer.RemoveItem(item.item.GetTechType());
							extractionResults[vName]++;
							extractedAnything = true;
						}
						yield return null;
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
						if (container != null)
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

		private void UpdateText()
		{
			var dockingBayCount = dockingBays.Count;
			text.text = dockingBayCount > 0 ? ("Docking Bays: " + dockingBayCount) : "No Docking Bays";
			text.text += "\nVehicles:";
			foreach (var vehicle in vehicles)
			{
				text.text += "\n" + vehicle.GetName();
			}
			if (vehicles.Count == 0)
			{
				text.text += "\n(None)";
			}
			if (extractingItems)
			{
				text.text += "\n\n<EXTRACTING...>";
			}

#if USE_AUTOSORT
			text.text += "\nAutosorters: " + autosorters.Count;
#endif
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

			if (SaveLoadManager.main != null && SaveLoadManager.main.isSaving && !Mod.IsSaving())
			{
				Mod.Save();
			}

			UpdateText();
		}

		private bool ShouldEnableContainer()
		{
#if USE_AUTOSORT
			return !autosortCheckbox.pointerOver;
#else
			return true;
#endif
		}

		private void Initialize()
		{
			background.gameObject.SetActive(true);
			icon.gameObject.SetActive(true);
			text.gameObject.SetActive(true);

			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("LockerScreen.png"));
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Receptacle.png"));

#if USE_AUTOSORT
			autosortCheckbox.toggled = true;
			autosortCheckbox.Initialize();
#endif

			UpdateDockedVehicles();
			UpdateText();

			initialized = true;
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

			var autosortTarget = prefab.AddComponent<VehicleStorageAccess>();

			autosortTarget.textPrefab = GameObject.Instantiate(prefab.GetComponentInChildren<Text>());
			var label = prefab.FindChild("Label");
			DestroyImmediate(label);

			var color = new Color32(66, 134, 244, 255);

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
			autosortTarget.background = LockerPrefabShared.CreateBackground(canvas.transform);
			autosortTarget.icon = LockerPrefabShared.CreateIcon(autosortTarget.background.transform, color, 80);
			autosortTarget.text = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, color, -10, 12, "");

#if USE_AUTOSORT
			autosortTarget.autosortCheckbox = CreateAutosortCheckbox(autosortTarget.background.transform, color, autosortTarget.textPrefab, autosortTarget);
#endif

			autosortTarget.background.gameObject.SetActive(false);
			autosortTarget.icon.gameObject.SetActive(false);
			autosortTarget.text.gameObject.SetActive(false);

			return prefab;
		}

#if USE_AUTOSORT
		private static CheckboxButton CreateAutosortCheckbox(Transform parent, Color color, Text textPrefab, VehicleStorageAccess target)
		{
			var w = 100;
			var checkboxButton = new GameObject("AutosortCheckbox", typeof(RectTransform));
			var rt = checkboxButton.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, w, 20);
			rt.anchoredPosition = new Vector2(0, -104);

			var iconWidth = 20;
			var checkbox = LockerPrefabShared.CreateIcon(rt, color, 0);
			RectTransformExtensions.SetSize(checkbox.rectTransform, iconWidth, iconWidth);
			checkbox.rectTransform.anchoredPosition = new Vector2(-w / 2 + iconWidth / 2, 0);

			var spacing = 5;
			var text = LockerPrefabShared.CreateText(rt, textPrefab, color, 0, 10, "Autosort");
			RectTransformExtensions.SetSize(text.rectTransform, w - iconWidth - spacing, iconWidth);
			text.rectTransform.anchoredPosition = new Vector2(iconWidth / 2 + spacing, 0);
			text.alignment = TextAnchor.MiddleLeft;

			checkboxButton.AddComponent<BoxCollider2D>();

			var button = checkboxButton.AddComponent<CheckboxButton>();
			button.target = target;
			button.image = checkbox;
			button.text = text;

			return button;
		}
#endif
	}
}
