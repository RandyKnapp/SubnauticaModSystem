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
		}
		
		private void UpdateText()
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
			container.width = Mod.config.ReceptacleWidth;
			container.height = Mod.config.ReceptacleHeight;
			container.container.Resize(Mod.config.ReceptacleWidth, Mod.config.ReceptacleHeight);

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
