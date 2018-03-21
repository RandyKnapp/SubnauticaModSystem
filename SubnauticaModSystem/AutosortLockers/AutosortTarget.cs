using Common.Mod;
using Common.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Newtonsoft.Json;
using System;

namespace AutosortLockers
{
	public class AutosortTarget : MonoBehaviour
	{
		public const int MaxTypes = 7;
		public const float MaxDistance = 3;

		private bool initialized;
		private Constructable constructable;
		private StorageContainer container;
		private AutosortTypePicker picker;
		private CustomizeScreen customizeScreen;
		private Coroutine plusCoroutine;
		private SaveDataEntry saveData;

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
		private ConfigureButton customizeButton;
		[SerializeField]
		private Image customizeButtonImage;
		[SerializeField]
		private Text text;
		[SerializeField]
		private Text label;
		[SerializeField]
		private Text plus;
		[SerializeField]
		private Text quantityText;
		[SerializeField]
		private List<AutosorterFilter> currentFilters = new List<AutosorterFilter>();

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = gameObject.GetComponent<StorageContainer>();
		}

		public void SetPicker(AutosortTypePicker picker)
		{
			this.picker = picker;
		}

		public List<AutosorterFilter> GetCurrentFilters()
		{
			return currentFilters;
		}

		public void AddFilter(AutosorterFilter filter)
		{
			if (currentFilters.Count >= AutosortTarget.MaxTypes)
			{
				return;
			}
			if (ContainsFilter(filter))
			{
				return;
			}

			currentFilters.Add(filter);
			UpdateText();
		}

		private bool ContainsFilter(AutosorterFilter filter)
		{
			foreach (var f in currentFilters)
			{
				if (f.IsSame(filter))
				{
					return true;
				}
			}

			return false;
		}

		public void RemoveFilter(AutosorterFilter filter)
		{
			foreach (var f in currentFilters)
			{
				if (f.IsSame(filter))
				{
					currentFilters.Remove(f);
					break;
				}
			}
			UpdateText();
		}

		private void UpdateText()
		{
			if (text != null)
			{
				if (currentFilters == null || currentFilters.Count == 0)
				{
					text.text = "[Any]";
				}
				else
				{
					string filtersText = string.Join("\n", currentFilters.Select((f) => f.IsCategory() ? "[" + f.GetString() + "]" : f.GetString()).ToArray());
					text.text = filtersText;
				}
			}
		}

		internal void AddItem(Pickupable item)
		{
			container.container.AddItem(item);

			if (plusCoroutine != null)
			{
				StopCoroutine(plusCoroutine);
			}
			plusCoroutine = StartCoroutine(ShowPlus());
		}

		internal bool CanAddItemByItemFilter(Pickupable item)
		{
			bool allowed = IsTypeAllowedByItemFilter(item.GetTechType());
			return allowed && container.container.HasRoomFor(item);
		}

		internal bool CanAddItemByCategoryFilter(Pickupable item)
		{
			bool allowed = IsTypeAllowedByCategoryFilter(item.GetTechType());
			return allowed && container.container.HasRoomFor(item);
		}

		internal bool CanAddItem(Pickupable item)
		{
			bool allowed = CanTakeAnyItem() || IsTypeAllowed(item.GetTechType());
			return allowed && container.container.HasRoomFor(item);
		}

		internal bool CanTakeAnyItem()
		{
			return currentFilters == null || currentFilters.Count == 0;
		}

		internal bool CanAddItems()
		{
			return constructable.constructed;
		}

		internal bool HasCategoryFilters()
		{
			foreach (var filter in currentFilters)
			{
				if (filter.IsCategory())
				{
					return true;
				}
			}
			return false;
		}

		internal bool HasItemFilters()
		{
			foreach (var filter in currentFilters)
			{
				if (!filter.IsCategory())
				{
					return true;
				}
			}
			return false;
		}

		private bool IsTypeAllowedByCategoryFilter(TechType techType)
		{
			foreach (var filter in currentFilters)
			{
				if (filter.IsCategory() && filter.IsTechTypeAllowed(techType))
				{
					return true;
				}
			}

			return false;
		}

		private bool IsTypeAllowedByItemFilter(TechType techType)
		{
			foreach (var filter in currentFilters)
			{
				if (!filter.IsCategory() && filter.IsTechTypeAllowed(techType))
				{
					return true;
				}
			}

			return false;
		}

		private bool IsTypeAllowed(TechType techType)
		{
			foreach (var filter in currentFilters)
			{
				if (filter.IsTechTypeAllowed(techType))
				{
					return true;
				}
			}

			return false;
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
				bool playerInRange = distSq <= (MaxDistance * MaxDistance);
				configureButton.enabled = playerInRange;
				customizeButton.enabled = playerInRange;

				if (picker != null && picker.isActiveAndEnabled && !playerInRange)
				{
					picker.gameObject.SetActive(false);
				}
				if (customizeScreen != null && customizeScreen.isActiveAndEnabled && !playerInRange)
				{
					customizeScreen.gameObject.SetActive(false);
				}
			}

			container.enabled = ShouldEnableContainer();

			if (SaveLoadManager.main != null && SaveLoadManager.main.isSaving && !Mod.IsSaving())
			{
				Mod.Save();
			}

			UpdateQuantityText();
		}

		private bool ShouldEnableContainer()
		{
			return (picker == null || !picker.isActiveAndEnabled)
				&& (customizeScreen == null || !customizeScreen.isActiveAndEnabled)
				&& (!configureButton.pointerOver || !configureButton.enabled)
				&& (!customizeButton.pointerOver || !customizeButton.enabled);
		}

		internal void ShowConfigureMenu()
		{
			foreach (var otherPicker in GameObject.FindObjectsOfType<AutosortTarget>())
			{
				otherPicker.HideAllMenus();
			}
			picker.gameObject.SetActive(true);
		}

		internal void ShowCustomizeMenu()
		{
			foreach (var otherPicker in GameObject.FindObjectsOfType<AutosortTarget>())
			{
				otherPicker.HideAllMenus();
			}
			customizeScreen.gameObject.SetActive(true);
		}

		internal void HideConfigureMenu()
		{
			if (picker != null)
			{
				picker.gameObject.SetActive(false);
			}
		}

		internal void HideCustomizeMenu()
		{
			if (customizeScreen != null)
			{
				customizeScreen.gameObject.SetActive(false);
			}
		}

		internal void HideAllMenus()
		{
			if (initialized)
			{
				HideConfigureMenu();
				HideCustomizeMenu();
			}
		}

		private void Initialize()
		{
			background.gameObject.SetActive(true);
			icon.gameObject.SetActive(true);
			text.gameObject.SetActive(true);

			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("LockerScreen.png"));
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Receptacle.png"));
			configureButtonImage.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Configure.png"));
			customizeButtonImage.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Edit.png"));

			configureButton.onClick = ShowConfigureMenu;
			customizeButton.onClick = ShowCustomizeMenu;

			saveData = GetSaveData();
			InitializeFromSaveData();

			InitializeFilters();

			UpdateText();

			CreatePicker();
			CreateCustomizeScreen();

			initialized = true;
		}

		private void InitializeFromSaveData()
		{
			label.text = saveData.Label;
			label.color = saveData.LabelColor.ToColor();
			icon.color = saveData.IconColor.ToColor();
			configureButtonImage.color = saveData.ButtonsColor.ToColor();
			customizeButtonImage.color = saveData.ButtonsColor.ToColor();
			text.color = saveData.OtherTextColor.ToColor();
			quantityText.color = saveData.OtherTextColor.ToColor();
			SetLockerColor(saveData.LockerColor.ToColor());
		}

		private void SetLockerColor(Color color)
		{
			var meshRenderers = GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = color;
			}
		}

		private SaveDataEntry GetSaveData()
		{
			var prefabIdentifier = GetComponent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			return Mod.GetSaveData(id);
		}

		private void InitializeFilters()
		{
			if (saveData == null)
			{
				currentFilters = new List<AutosorterFilter>();
				return;
			}

			currentFilters = saveData.FilterData.ShallowCopy();
		}

		private void CreatePicker()
		{
			SetPicker(AutosortTypePicker.Create(transform, textPrefab));
			picker.transform.localPosition = background.canvas.transform.localPosition + new Vector3(0, 0, 0.04f);
			picker.Initialize(this);
			picker.gameObject.SetActive(false);
		}

		private void CreateCustomizeScreen()
		{
			customizeScreen = CustomizeScreen.Create(background.transform, saveData);
			customizeScreen.onModified += InitializeFromSaveData;
			customizeScreen.Initialize(saveData);
			customizeScreen.gameObject.SetActive(false);
		}

		public void Save(SaveData saveDataList)
		{
			var prefabIdentifier = GetComponent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			if (saveData == null)
			{
				saveData = new SaveDataEntry() { Id = id, FilterData = currentFilters };
			}

			saveData.Label = label.text;
			saveData.LabelColor = label.color;
			saveData.IconColor = icon.color;
			saveData.OtherTextColor = text.color;
			saveData.ButtonsColor = configureButtonImage.color;

			var meshRenderer = GetComponentInChildren<MeshRenderer>();
			saveData.LockerColor = meshRenderer.material.color;

			saveDataList.Entries.Add(saveData);
		}

		public IEnumerator ShowPlus()
		{
			plus.color = new Color(plus.color.r, plus.color.g, plus.color.b, 1);
			float t = 0;
			float rate = 0.5f;
			while (t < 1.0)
			{
				t += Time.deltaTime * rate;
				plus.color = new Color(plus.color.r, plus.color.g, plus.color.b, Mathf.Lerp(1, 0, t));
				yield return null;
			}
		}
		
		private void UpdateQuantityText()
		{
			var count = container.container.count;
			quantityText.text = count == 0 ? "empty" : count.ToString();
		}




		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = AutosortTarget.GetSmallPrefab,
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
						techType = TechType.Magnetite,
						amount = 1
					}
				}
			});

			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = AutosortTarget.GetStandingPrefab,
				techType = Mod.GetTechType(CustomTechType.AutosortTargetStanding),
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/AutosortTargetStanding",
				displayString = "Standing Autosort Receptacle",
				tooltip = "Large locker linked to an Autosorter that receives sorted items.",
				techTypeKey = CustomTechType.AutosortTargetStanding.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(Mod.GetAssetPath("AutosortTargetStanding.png"))),
				recipe = Mod.config.EasyBuild
				? new List<CustomIngredient> {
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 2
					},
					new CustomIngredient() {
						techType = TechType.Quartz,
						amount = 1
					}
				}
				: new List<CustomIngredient>
				{
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 2
					},
					new CustomIngredient() {
						techType = TechType.Magnetite,
						amount = 1
					}
				}
			});
		}

		public static GameObject GetStandingPrefab()
		{
			var prefab = GetPrefab("StandingAutosortReceptacle", "Submarine/Build/Locker");
			var autosortTarget = prefab.GetComponent<AutosortTarget>();

			var canvas = prefab.GetComponentInChildren<Canvas>();
			var t = canvas.transform;
			t.localPosition = new Vector3(0, 1.1f, 0.25f);

			return prefab;
		}

		public static GameObject GetSmallPrefab()
		{
			return GetPrefab("AutosortReceptacle", "Submarine/Build/SmallLocker");
		}

		public static GameObject GetPrefab(string name, string basePrefab)
		{
			GameObject originalPrefab = Resources.Load<GameObject>(basePrefab);
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			prefab.name = name;

			var container = prefab.GetComponent<StorageContainer>();
			container.width = Mod.config.ReceptacleWidth;
			container.height = Mod.config.ReceptacleHeight;
			container.container.Resize(Mod.config.ReceptacleWidth, Mod.config.ReceptacleHeight);

			var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
			}

			var autosortTarget = prefab.AddComponent<AutosortTarget>();

			var smallLockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			autosortTarget.textPrefab = GameObject.Instantiate(smallLockerPrefab.GetComponentInChildren<Text>());
			var label = prefab.FindChild("Label");
			DestroyImmediate(label);

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
			autosortTarget.background = LockerPrefabShared.CreateBackground(canvas.transform);
			autosortTarget.icon = LockerPrefabShared.CreateIcon(autosortTarget.background.transform, autosortTarget.textPrefab.color, 70);
			autosortTarget.text = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, -20, 12, "Any");

			autosortTarget.label = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, 100, 12, "Locker");

			autosortTarget.background.gameObject.SetActive(false);
			autosortTarget.icon.gameObject.SetActive(false);
			autosortTarget.text.gameObject.SetActive(false);

			autosortTarget.plus = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, 0, 30, "+");
			autosortTarget.plus.color = new Color(autosortTarget.textPrefab.color.r, autosortTarget.textPrefab.color.g, autosortTarget.textPrefab.color.g, 0);
			autosortTarget.plus.rectTransform.anchoredPosition += new Vector2(30, 70);

			autosortTarget.quantityText = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, 0, 10, "XX");
			autosortTarget.quantityText.rectTransform.anchoredPosition += new Vector2(-35, -104);

			autosortTarget.configureButton = ConfigureButton.Create(autosortTarget.background.transform, autosortTarget.textPrefab.color, 40);
			autosortTarget.configureButtonImage = autosortTarget.configureButton.GetComponent<Image>();
			autosortTarget.customizeButton = ConfigureButton.Create(autosortTarget.background.transform, autosortTarget.textPrefab.color, 20);
			autosortTarget.customizeButtonImage = autosortTarget.customizeButton.GetComponent<Image>();

			return prefab;
		}
	}
}
