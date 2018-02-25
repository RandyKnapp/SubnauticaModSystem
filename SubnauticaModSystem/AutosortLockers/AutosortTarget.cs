using Common.Mod;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class AutosortTarget : MonoBehaviour
	{
		public const int MaxTypes = 7;

		private bool initialized;
		private Constructable constructable;
		private StorageContainer container;
		private AutosortTypePicker picker;

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
		private HashSet<TechType> allowedTypes;

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = gameObject.GetComponent<StorageContainer>();
		}

		public HashSet<TechType> GetTechTypes()
		{
			return allowedTypes;
		}

		public void SetTechTypes(HashSet<TechType> types)
		{
			allowedTypes = types;
			UpdateText();
		}

		private void UpdateText()
		{
			if (text != null)
			{
				if (allowedTypes.Count == 0)
				{
					text.text = "Any";
				}
				else
				{
					string typesText = string.Join("\n", allowedTypes.Select((t) => Language.main.Get(t)).ToArray());
					text.text = typesText;
				}
			}
		}

		internal void AddItem(Pickupable item)
		{
			container.container.AddItem(item);
		}

		internal bool CanAddItem(Pickupable item)
		{
			bool allowed = allowedTypes.Count == 0 || allowedTypes.Contains(item.GetTechType());
			return allowed && container.container.HasRoomFor(item);
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

			if (Player.main != null)
			{
				float distSq = (Player.main.transform.position - transform.position).sqrMagnitude;
				configureButton.enabled = distSq <= 3 * 3;
			}

			container.enabled = ShouldEnableContainer();
		}

		private bool ShouldEnableContainer()
		{
			return !picker.isActiveAndEnabled && (!configureButton.pointerOver || !configureButton.enabled);
		}

		internal void ShowConfigureMenu()
		{
			picker.gameObject.SetActive(true);
		}

		internal void HideConfigureMenu()
		{
			picker.gameObject.SetActive(false);
		}

		private void Initialize()
		{
			background.gameObject.SetActive(true);
			icon.gameObject.SetActive(true);
			text.gameObject.SetActive(true);

			background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Receptacle.png"));
			configureButtonImage.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Configure.png"));

			picker = AutosortTypePicker.Create(transform, textPrefab);
			picker.Initialize(this);
			picker.gameObject.SetActive(false);

			SetTechTypes(new HashSet<TechType>() {
				TechType.Titanium,
				TechType.Glass,
				TechType.Gold,
				TechType.AcidMushroom,
				TechType.ComputerChip,
				TechType.JeweledDiskPiece,
				TechType.AluminumOxide
			});

			initialized = true;
		}



		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = AutosortTarget.GetPrefab,
				techType = Mod.GetTechType(CustomTechType.AutosortTarget),
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/AutosortTarget",
				displayString = "Autosort Receptacle",
				tooltip = "Wall locker linked to an Autosorter that receives sorted items.",
				techTypeKey = CustomTechType.AutosortTarget.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(Mod.GetAssetPath("AutosortTarget.png"))),
				recipe = Mod.config.EasyBuild
				? new List<CustomIngredient> {
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 2
					}
				}
				: new List<CustomIngredient>
				{
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 2
					},
					new CustomIngredient() {
						techType = TechType.UraniniteCrystal,
						amount = 1
					},
					new CustomIngredient() {
						techType = TechType.Magnetite,
						amount = 1
					}
				}
			});
		}

		public static GameObject GetPrefab()
		{
			GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			prefab.name = "AutosortReceptacle";

			var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
			}

			var autosortTarget = prefab.AddComponent<AutosortTarget>();

			autosortTarget.textPrefab = GameObject.Instantiate(prefab.GetComponentInChildren<Text>());
			var label = prefab.FindChild("Label");
			DestroyImmediate(label);

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
			autosortTarget.background = LockerPrefabShared.CreateBackground(canvas.transform);
			autosortTarget.icon = LockerPrefabShared.CreateIcon(autosortTarget.background.transform, autosortTarget.textPrefab.color, 80);
			autosortTarget.text = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, -20, 12, "Any");
			autosortTarget.configureButton = CreateConfigureButton(autosortTarget.background.transform, autosortTarget.textPrefab.color, autosortTarget);
			autosortTarget.configureButtonImage = autosortTarget.configureButton.GetComponent<Image>();

			autosortTarget.background.gameObject.SetActive(false);
			autosortTarget.icon.gameObject.SetActive(false);
			autosortTarget.text.gameObject.SetActive(false);

			return prefab;
		}

		private static ConfigureButton CreateConfigureButton(Transform parent, Color color, AutosortTarget target)
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
