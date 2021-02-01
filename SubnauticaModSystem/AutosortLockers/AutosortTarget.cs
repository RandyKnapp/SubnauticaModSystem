using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Mod;
using Common.Utility;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;
using UWE;
#if SUBNAUTICA
    using RecipeData = SMLHelper.V2.Crafting.TechData;
    using Sprite = Atlas.Sprite;
#elif BELOWZERO
using TMPro;
#endif

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

			// Moved to FinalSetup
			//CreatePicker();
			//CreateCustomizeScreen();
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

				var newTypes = AutosorterList.GetOldFilter(filter.Category, out bool success, out string newCategory);
				if (success)
				{
					newData.Add(new AutosorterFilter() { Category = newCategory, Types = newTypes });
					continue;
				}

				newData.Add(filter);
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
			quantityText.text = count == 0 ? "empty" : count.ToString();
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
				//Logger.Log("AutosortTargetBuildable.GetGameObjectAsync: 1");

				TaskResult<GameObject> result = new TaskResult<GameObject>();
				yield return GetPrefabAsync(TechType.SmallLocker, result);

				//Logger.Log("AutosortTargetBuildable.GetGameObjectAsync: 2");
				GameObject basePrefab = result.Get();
				GameObject prefab = GameObject.Instantiate(basePrefab);


				//Logger.Log("AutosortTargetBuildable.GetGameObjectAsync: 3");
				StorageContainer container = prefab.GetComponent<StorageContainer>();
				//Logger.Log("AutosortTargetBuildable.GetGameObjectAsync: 3.1");
				container.width = Mod.config.ReceptacleWidth;
				//Logger.Log("AutosortTargetBuildable.GetGameObjectAsync: 3.2");
				container.height = Mod.config.ReceptacleHeight;
				//Logger.Log("AutosortTargetBuildable.GetGameObjectAsync: 3.3, container.container " + (container.container == null ? "is" : "is not") + " null");
				container.Resize(Mod.config.ReceptacleWidth, Mod.config.ReceptacleHeight);

				//Logger.Log("AutosortTargetBuildable.GetGameObjectAsync: 4");

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
				//Logger.Log("AutosortStandingTargetBuildable.GetGameObjectAsync: 1");

				TaskResult<GameObject> result = new TaskResult<GameObject>();
				yield return GetPrefabAsync(TechType.Locker, result);
				//Logger.Log("AutosortStandingTargetBuildable.GetGameObjectAsync: 2");

				GameObject basePrefab = result.Get();
				GameObject prefab = GameObject.Instantiate(basePrefab);

				//Logger.Log("AutosortStandingTargetBuildable.GetGameObjectAsync: 3");
				StorageContainer container = prefab.GetComponent<StorageContainer>();
				//Logger.Log("AutosortStandingTargetBuildable.GetGameObjectAsync: 3.1");
				container.width = Mod.config.StandingReceptacleWidth;
				//Logger.Log("AutosortStandingTargetBuildable.GetGameObjectAsync: 3.2");
				container.height = Mod.config.StandingReceptacleHeight;
				//Logger.Log("AutosortStandingTargetBuildable.GetGameObjectAsync: 3.3, container.container " + (container.container == null ? "is" : "is not") + " null");
				container.Resize(Mod.config.StandingReceptacleWidth, Mod.config.StandingReceptacleHeight);

				//Logger.Log("AutosortStandingTargetBuildable.GetGameObjectAsync: 4");
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

		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			var sorterTarget = new AutosortTargetBuildable();
			sorterTarget.Patch();

			var sorterStandingTarget = new AutosortStandingTargetBuildable();
			sorterStandingTarget.Patch();
		}

		public static IEnumerator GetPrefabAsync(TechType basePrefab, IOut<GameObject> gameObject)
		{
			Logger.Log($"GetPrefabAsync() executing for basePrefab TechType.{basePrefab.AsString()}");
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
			//var smallLockerPrefab = CraftData.GetPrefabForTechType(TechType.SmallLocker);

			//Logger.Log($"GetPrefabAsync() attempting to instantiate smallLockerPrefab");
			var smallLockerPrefab = GameObject.Instantiate(task.GetResult());
			//Logger.Log($"GetPrefabAsync() attempting to instantiate autosortTarget; smallLockerPrefab " + (smallLockerPrefab == null ? "is" : "is not") + " null");
#if SUBNAUTICA
			autosortTarget.textPrefab = GameObject.Instantiate(smallLockerPrefab.GetComponentInChildren<Text>());
#elif BELOWZERO
			autosortTarget.textPrefab = GameObject.Instantiate(smallLockerPrefab.GetComponentInChildren<TextMeshProUGUI>());
#endif
			var label = prefab.FindChild("Label");
			DestroyImmediate(label);

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
			if (basePrefab == TechType.Locker)
			{
				canvas.transform.localPosition = new Vector3(0, 1.1f, 0.25f);
			}

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

			//return prefab;
			//originalPrefab.SetActive(false);
			gameObject.Set(prefab);
		}
	}
}
