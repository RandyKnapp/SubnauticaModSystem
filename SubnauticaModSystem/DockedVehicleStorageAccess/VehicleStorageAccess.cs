using Common.Mod;
using Common.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Newtonsoft.Json;
using System;

namespace DockedVehicleStorageAccess
{
	public class VehicleStorageAccess : MonoBehaviour
	{
		private bool initialized;
		private Constructable constructable;
		private StorageContainer container;
		private SubRoot subRoot;
		private List<VehicleDockingBay> dockingBays = new List<VehicleDockingBay>();
		private List<Vehicle> vehicles = new List<Vehicle>();

		[SerializeField]
		private Text textPrefab;
		[SerializeField]
		private Image background;
		[SerializeField]
		private Image icon;
		[SerializeField]
		private ConfigureButton configureButton;
		[SerializeField]
		private Image configureButtonImage;
		[SerializeField]
		private Text text;
		[SerializeField]
		private Text plus;
		[SerializeField]
		private Text quantityText;

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
				}
				yield return new WaitForSeconds(0.5f);
			}
		}

		private void GetDockingBays()
		{
			foreach (var dockingBay in dockingBays)
			{
				dockingBay.onDockedChanged -= UpdateDockedVehicles;
			}
			dockingBays = subRoot.GetComponentsInChildren<VehicleDockingBay>().ToList();
			foreach (var dockingBay in dockingBays)
			{
				dockingBay.onDockedChanged += UpdateDockedVehicles;
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

		private void UpdateText()
		{
			var dockingBayCount = dockingBays.Count;
			text.text = dockingBayCount > 0 ? ("Docking Bays: " + dockingBayCount) : "No Docking Bays";
			text.text += "\nVehicles:";
			foreach (var vehicle in vehicles)
			{
				text.text += "\n-" + vehicle.GetName();
			}
			if (vehicles.Count == 0)
			{
				text.text += "(None)";
			}
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

			container.enabled = ShouldEnableContainer();

			if (SaveLoadManager.main != null && SaveLoadManager.main.isSaving && !Mod.IsSaving())
			{
				Mod.Save();
			}

			UpdateText();
		}

		private bool ShouldEnableContainer()
		{
			return true;
		}
		private void Initialize()
		{
			background.gameObject.SetActive(true);
			icon.gameObject.SetActive(true);
			text.gameObject.SetActive(true);

			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("LockerScreen.png"));
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Receptacle.png"));
			configureButtonImage.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Configure.png"));

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

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
			autosortTarget.background = LockerPrefabShared.CreateBackground(canvas.transform);
			autosortTarget.icon = LockerPrefabShared.CreateIcon(autosortTarget.background.transform, autosortTarget.textPrefab.color, 80);
			autosortTarget.text = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, -10, 12, "Any");
			autosortTarget.configureButton = CreateConfigureButton(autosortTarget.background.transform, autosortTarget.textPrefab.color, autosortTarget);
			autosortTarget.configureButtonImage = autosortTarget.configureButton.GetComponent<Image>();

			autosortTarget.plus = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, 0, 30, "+");
			autosortTarget.plus.color = new Color(autosortTarget.textPrefab.color.r, autosortTarget.textPrefab.color.g, autosortTarget.textPrefab.color.g, 0);
			autosortTarget.plus.rectTransform.anchoredPosition += new Vector2(30, 80);

			autosortTarget.quantityText = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, 0, 10, "XX");
			autosortTarget.quantityText.rectTransform.anchoredPosition += new Vector2(-35, -104);

			autosortTarget.background.gameObject.SetActive(false);
			autosortTarget.icon.gameObject.SetActive(false);
			autosortTarget.text.gameObject.SetActive(false);

			return prefab;
		}

		private static ConfigureButton CreateConfigureButton(Transform parent, Color color, VehicleStorageAccess target)
		{
			var config = LockerPrefabShared.CreateIcon(parent, color, 0);
			RectTransformExtensions.SetSize(config.rectTransform, 20, 20);
			config.rectTransform.anchoredPosition = new Vector2(40, -104);

			config.gameObject.AddComponent<BoxCollider2D>();
			var button = config.gameObject.AddComponent<ConfigureButton>();
			button.target = target;

			return button;
		}
	}
}
