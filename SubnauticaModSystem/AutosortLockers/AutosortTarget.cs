using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Mod;
using Common.Utility;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UWE;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.RecipeData;
#elif BELOWZERO
using TMPro;
#endif

namespace AutosortLockers
{
	public class AutosortTarget : MonoBehaviour
	{
		public const int MaxTypes = 10;
		public const float MaxDistance = 3;

		private bool initialized;
		private Constructable constructable;
		private StorageContainer container;
		private AutosortTypePicker picker;
		private CustomizeScreen customizeScreen;
		private Coroutine plusCoroutine;
		private SaveDataEntry saveData;

#if SUBNAUTICA
		[SerializeField]
		private Text textPrefab;
		[SerializeField]
		private Text text;
		[SerializeField]
		private Text label;
		[SerializeField]
		private Text plus;
		[SerializeField]
		private Text quantityText;
#elif BELOWZERO
		[SerializeField]
		private TextMeshProUGUI textPrefab;
		[SerializeField]
		private TextMeshProUGUI text;
		[SerializeField]
		private TextMeshProUGUI label;
		[SerializeField]
		private TextMeshProUGUI plus;
		[SerializeField]
		private TextMeshProUGUI quantityText;
#endif
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
		private List<AutosorterFilter> currentFilters = new List<AutosorterFilter>();

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = gameObject.GetComponent<StorageContainer>();

			Mod.OnDataLoaded += OnDataLoaded;
		}

		private void OnDataLoaded(SaveData allSaveData)
		{
			Logger.Log("OnDataLoaded");
			saveData = GetSaveData();
			InitializeFromSaveData();
			InitializeFilters();
			UpdateText();
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
			if (AnAutosorterIsSorting())
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
			if (AnAutosorterIsSorting())
			{
				return;
			}
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
			int strLen = 17;
			string lockerType = container.name;
			if (lockerType == "AutosortTarget(Clone)")
			{
				strLen = 17; // Trim filter lables so they don't wrap on the lockers
			}
			else
			{
				strLen = 22; // Trim filter lables so they don't wrap on the lockers
			}

			if (text != null)
			{
				if (currentFilters == null || currentFilters.Count == 0)
				{
					text.text = "[Any]";
#if SUBNAUTICA
					text.alignment = TextAnchor.MiddleCenter;
#elif BELOWZERO
					text.alignment = TextAlignmentOptions.Center;
#endif
				}
				else
				{
					string filtersText = string.Join("\n", currentFilters.Select((f) => f.IsCategory() ? "[" + (f.GetString().Length > strLen ? f.GetString().Substring(0, strLen) : f.GetString()) + "]" : f.GetString().Length > strLen ? f.GetString().Substring(0, strLen) : f.GetString()).ToArray());
					// Filter text displayed on the lockers
					text.text = filtersText;

					if (currentFilters.Count == 1)
					{
#if SUBNAUTICA
						text.alignment = TextAnchor.MiddleCenter;
#elif BELOWZERO
						text.alignment = TextAlignmentOptions.Center;
#endif
					}
					else
					{
#if SUBNAUTICA
						text.alignment = TextAnchor.MiddleLeft;
#elif BELOWZERO
						text.alignment = TextAlignmentOptions.Left;
#endif
					}
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

		private bool AnAutosorterIsSorting()
		{
			var root = GetComponentInParent<SubRoot>();
			if (root != null && root.isBase)
			{
				var autosorters = root.GetComponentsInChildren<AutosortLocker>();
				foreach (var autosorter in autosorters)
				{
					if (autosorter.IsSorting)
					{
						return true;
					}
				}
			}
			return false;
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

			StartCoroutine("FinalSetup");
			initialized = true;
		}

		internal bool bPrefabsLoaded = false;
		internal GameObject lockerPrefab;

		private IEnumerator FinalSetup()
		{
			IPrefabRequest request = PrefabDatabase.GetPrefabForFilenameAsync("Submarine/Build/SmallLocker.prefab");
			yield return request;
			request.TryGetPrefab(out GameObject prefab);
			lockerPrefab = prefab;
			bPrefabsLoaded = true;

			CreatePicker();
			CreateCustomizeScreen();

			yield break;
		}

		private void InitializeFromSaveData()
		{
			Logger.Log("Object Initialize from Save Data");
			label.text = saveData.Label;
			label.color = saveData.LabelColor.ToColor();
			DestroyImmediate(label);
			icon.color = saveData.IconColor.ToColor();
			configureButtonImage.color = saveData.ButtonsColor.ToColor();
			customizeButtonImage.color = saveData.ButtonsColor.ToColor();
			text.color = saveData.OtherTextColor.ToColor();
			quantityText.color = saveData.ButtonsColor.ToColor();
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
			var id = prefabIdentifier?.Id ?? string.Empty;

			return Mod.GetSaveData(id);
		}

		private void InitializeFilters()
		{
			if (saveData == null)
			{
				currentFilters = new List<AutosorterFilter>();
				return;
			}

			currentFilters = GetNewVersion(saveData.FilterData);
		}

		private List<AutosorterFilter> GetNewVersion(List<AutosorterFilter> filterData)
		{
			Dictionary<TechType, AutosorterFilter> validItems = new Dictionary<TechType, AutosorterFilter>();
			Dictionary<string, AutosorterFilter> validCategories = new Dictionary<string, AutosorterFilter>();

			var filterList = AutosorterList.GetFilters();

			foreach (var filter in filterList)
			{
				if (filter.IsCategory())
				{
					validCategories[filter.Category] = filter;
				}
				else
				{
					validItems[filter.Types[0]] = filter;
				}
			}

			var newData = new List<AutosorterFilter>();
			foreach (var filter in filterData)
			{
				if (validCategories.ContainsKey(filter.Category) || filter.Category == "")
				{
					newData.Add(filter);
					continue;
				}

				if (filter.Category == "0")
				{
					filter.Category = "";
					newData.Add(filter);
					continue;
				}
			}
			return newData;
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
			customizeScreen = CustomizeScreen.Create(background.transform, saveData, lockerPrefab);
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
				saveData = new SaveDataEntry() { Id = id };
			}

			saveData.Id = id;
			saveData.FilterData = currentFilters;
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

			quantityText.text = count == 0 ? "Empty" : count.ToString();
		}

		internal class AutosortTargetBuildable : Buildable
		{
			public AutosortTargetBuildable()
				: base("AutosortTarget",
						"Autosort Receptacle",
						"Wall locker linked to an Autosorter that receives sorted items.")
			{
			}

			public override TechGroup GroupForPDA => TechGroup.InteriorModules;

			public override TechCategory CategoryForPDA => TechCategory.InteriorModule;

			public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
			{
				TaskResult<GameObject> result = new TaskResult<GameObject>();
				yield return GetPrefabAsync(TechType.SmallLocker, result);

				GameObject basePrefab = result.Get();
				GameObject prefab = GameObject.Instantiate(basePrefab);

				StorageContainer container = prefab.GetComponent<StorageContainer>();
				container.width = Mod.config.ReceptacleWidth;
				container.height = Mod.config.ReceptacleHeight;
				container.Resize(Mod.config.ReceptacleWidth, Mod.config.ReceptacleHeight);

				gameObject.Set(prefab);
				yield break;
			}

			protected override RecipeData GetBlueprintRecipe()
			{
				return new RecipeData
				{
					craftAmount = 1,
					Ingredients = Mod.config.EasyBuild
					? new List<Ingredient>
					{
						new Ingredient(TechType.Titanium, 2)
					}
					: new List<Ingredient>
					{
						new Ingredient(TechType.Titanium, 2),
						new Ingredient(TechType.Magnetite, 1)
					}
				};
			}

			protected override Sprite GetItemSprite()
			{
				return SMLHelper.V2.Utility.ImageUtils.LoadSpriteFromFile(Mod.GetAssetPath("AutosortTarget.png"));
			}
		}

		internal class AutosortStandingTargetBuildable : Buildable
		{
			public AutosortStandingTargetBuildable()
				: base("AutosortTargetStanding",
						"Standing Autosort Receptacle",
						"Large locker linked to an Autosorter that receives sorted items.")
			{
			}

			public override TechGroup GroupForPDA => TechGroup.InteriorModules;

			public override TechCategory CategoryForPDA => TechCategory.InteriorModule;

			public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
			{
				TaskResult<GameObject> result = new TaskResult<GameObject>();
				yield return GetPrefabAsync(TechType.Locker, result);

				GameObject basePrefab = result.Get();
				GameObject prefab = GameObject.Instantiate(basePrefab);

				StorageContainer container = prefab.GetComponent<StorageContainer>();
				container.width = Mod.config.StandingReceptacleWidth;
				container.height = Mod.config.StandingReceptacleHeight;
				container.Resize(Mod.config.StandingReceptacleWidth, Mod.config.StandingReceptacleHeight);
				gameObject.Set(prefab);
				yield break;
			}

			protected override RecipeData GetBlueprintRecipe()
			{
				return new RecipeData
				{
					craftAmount = 1,
					Ingredients = Mod.config.EasyBuild
					? new List<Ingredient>
					{
						new Ingredient(TechType.Titanium, 2),
						new Ingredient(TechType.Quartz, 1)
					}
					: new List<Ingredient>
					{
						new Ingredient(TechType.Titanium, 2),
						new Ingredient(TechType.Quartz, 1),
						new Ingredient(TechType.Magnetite, 1)
					}
				};
			}

			protected override Sprite GetItemSprite()
			{
				return SMLHelper.V2.Utility.ImageUtils.LoadSpriteFromFile(Mod.GetAssetPath("AutosortTargetStanding.png"));
			}
		}

		/*__________________________________________________________________________________________________________*/

		public static void AddBuildable()
		{
			var sorterTarget = new AutosortTargetBuildable();
			sorterTarget.Patch();

			var sorterStandingTarget = new AutosortStandingTargetBuildable();
			sorterStandingTarget.Patch();
		}

		public static IEnumerator GetPrefabAsync(TechType basePrefab, IOut<GameObject> gameObject)
		{
			CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(basePrefab);
			yield return task;

			GameObject originalPrefab = task.GetResult();
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
			}

			var autosortTarget = prefab.AddComponent<AutosortTarget>();

			task = CraftData.GetPrefabForTechTypeAsync(TechType.SmallLocker);
			yield return task;
			var smallLockerPrefab = GameObject.Instantiate(task.GetResult());

#if SUBNAUTICA
			autosortTarget.textPrefab = GameObject.Instantiate(smallLockerPrefab.GetComponentInChildren<Text>());
#elif BELOWZERO
			autosortTarget.textPrefab = GameObject.Instantiate(smallLockerPrefab.GetComponentInChildren<TextMeshProUGUI>());
#endif
			// Destroys the lable on the small locker
			var label = prefab.FindChild("Label");
			DestroyImmediate(label);
			label = prefab.FindChild("Locker");
			DestroyImmediate(label);

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
			if (basePrefab == TechType.Locker)
			{
				// Positions the rectangle on the standing locker horz, vert, depth
				canvas.transform.localPosition = new Vector3(0.04f, 1.0f, 0.25f);
			}

			autosortTarget.background = LockerPrefabShared.CreateBackground(canvas.transform, prefab.name);

			int iconPos = 75; // The vertical pos of the icon at the top of the container
			int textPos = 110; // The vertical pos of the "Locker" text at the top of the container
			int buttonPos = -104; // The vertical positions of the color and customize buttons
			int labelFont = 12; // The font for the label on the lockers
			if (Mod.config.ShowLabel)
			{                // This is a cludge until I can find the placeholder
				labelFont = 0; // Set to zero and the Locker text does not display
			}
			else
			{
				labelFont = 12;
			}

			// Change the positions for the Standing Locker
			if (prefab.name == "Locker(Clone)")
			{
				iconPos = 93;
				textPos = 120;
				buttonPos = -120;
			}

			// Position the locker icon
			autosortTarget.icon = LockerPrefabShared.CreateIcon(autosortTarget.background.transform, autosortTarget.textPrefab.color, iconPos);
			// Position the Filter lables, the first number is the horizontal position, the second number is the font size.
			autosortTarget.text = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, 0, 12, "[Any] - Doesn't display", prefab.name);
			// Position the "Locker" text
			autosortTarget.label = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, textPos, labelFont, "Locker - Doesn't display", prefab.name);

			autosortTarget.background.gameObject.SetActive(false);
			autosortTarget.icon.gameObject.SetActive(false);
			autosortTarget.text.gameObject.SetActive(false);
			// The container filters ??
			autosortTarget.plus = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, 0, 12, "+ - Doesn't display", prefab.name);

			// Pos of the color picker
			autosortTarget.plus.color = new Color(autosortTarget.textPrefab.color.r, autosortTarget.textPrefab.color.g, autosortTarget.textPrefab.color.g, 0);
			// Pos of the item count on the locker
			autosortTarget.quantityText = LockerPrefabShared.CreateText(autosortTarget.background.transform, autosortTarget.textPrefab, autosortTarget.textPrefab.color, 0, 12, "XX - Doesn't display", prefab.name);
			// Pos the quantity text on the locker
			if (prefab.name == "Locker(Clone)")
			{
				autosortTarget.quantityText.rectTransform.anchoredPosition += new Vector2(14, -210);
			}
			else
			{
				autosortTarget.quantityText.rectTransform.anchoredPosition += new Vector2(4, -198);
			}
			// Pos of the configure button on the locker
			autosortTarget.configureButton = ConfigureButton.Create(autosortTarget.background.transform, autosortTarget.textPrefab.color, 45, buttonPos);
			autosortTarget.configureButtonImage = autosortTarget.configureButton.GetComponent<Image>();
			// Pos of the customize button on the locker
			autosortTarget.customizeButton = ConfigureButton.Create(autosortTarget.background.transform, autosortTarget.textPrefab.color, 20, buttonPos);
			autosortTarget.customizeButtonImage = autosortTarget.customizeButton.GetComponent<Image>();

			gameObject.Set(prefab);
		}
	}
}