using Common.Mod;
using Common.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace AutosortLockers
{
	public class AutosortLocker : MonoBehaviour
	{
		private static readonly Color MainColor = new Color(1, 0.2f, 0.2f);
		private static readonly Color PulseColor = Color.white;

		private bool initialized;
		private Constructable constructable;
		private StorageContainer container;
		private List<AutosortTarget> singleItemTargets = new List<AutosortTarget>();
		private List<AutosortTarget> categoryTargets = new List<AutosortTarget>();
		private List<AutosortTarget> anyTargets = new List<AutosortTarget>();

		private int sortableItems = 0;
		private int unsortableItems = 0;

		[SerializeField]
		private Image background;
		[SerializeField]
		private Image icon;
		[SerializeField]
		private Text text;
		[SerializeField]
		private Text sortingText;
		[SerializeField]
		private bool isSorting;
		
		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = GetComponent<StorageContainer>();
			container.hoverText = "Open autosorter";
			container.storageLabel = "Autosorter";
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

			UpdateText();
		}

		private void UpdateText()
		{
			string output = "";
			if (isSorting)
			{
				output = "Sorting...";
			}
			else if (unsortableItems > 0)
			{
				output = "Unsorted Items: " + unsortableItems;
				foreach (var item in container.container)
				{
					output += "\n" + item.item.GetTechType();
				}
			}
			else
			{
				output = "Ready to Sort";
			}

			sortingText.text = output;
		}

		private void Initialize()
		{
			background.gameObject.SetActive(true);
			icon.gameObject.SetActive(true);
			text.gameObject.SetActive(true);
			sortingText.gameObject.SetActive(true);

			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("LockerScreen.png"));
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Sorter.png"));

			initialized = true;
		}

		private IEnumerator Start()
		{
			while (true)
			{
				yield return new WaitForSeconds(Mathf.Max(0, Mod.config.SortInterval - (unsortableItems / 60.0f)));
				
				yield return Sort();
			}
		}

		private void AccumulateTargets()
		{
			singleItemTargets.Clear();
			categoryTargets.Clear();
			anyTargets.Clear();

			SubRoot subRoot = gameObject.GetComponentInParent<SubRoot>();
			if (subRoot == null)
			{
				return;
			}

			var allTargets = subRoot.GetComponentsInChildren<AutosortTarget>().ToList();
			foreach (var target in allTargets)
			{
				if (target.isActiveAndEnabled && target.CanAddItems())
				{
					if (target.CanTakeAnyItem())
					{
						anyTargets.Add(target);
					}
					else
					{
						if (target.HasItemFilters())
						{
							singleItemTargets.Add(target);
						}
						if (target.HasCategoryFilters())
						{
							categoryTargets.Add(target);
						}
					}
				}
			}
		}

		private IEnumerator Sort()
		{
			isSorting = false;
			sortableItems = 0;
			unsortableItems = container.container.count;

			if (!initialized || container.IsEmpty())
			{
				yield break;
			}

			AccumulateTargets();
			if (NoTargets())
			{
				yield break;
			}

			if (SortFilteredTargets(false))
			{
				isSorting = true;
			}
			else if (SortFilteredTargets(true))
			{
				isSorting = true;
			}
			else
			{
				foreach (AutosortTarget target in anyTargets)
				{
					foreach (var item in container.container.ToList())
					{
						if (target.CanAddItem(item.item))
						{
							SortItem(item.item, target);
							sortableItems++;
							unsortableItems--;
							isSorting = true;
							yield break;
						}
					}
					yield return null;
				}
			}

			yield break;
		}

		private bool NoTargets()
		{
			return singleItemTargets.Count <= 0 && categoryTargets.Count <= 0 && anyTargets.Count <= 0;
		}

		private bool SortFilteredTargets(bool byCategory)
		{
			foreach (AutosortTarget target in byCategory ? categoryTargets : singleItemTargets)
			{
				foreach (AutosorterFilter filter in target.GetCurrentFilters())
				{
					if (filter.IsCategory() == byCategory)
					{
						foreach (var techType in filter.Types)
						{
							var items = container.container.GetItems(techType);
							if (items != null && items.Count > 0 && target.CanAddItem(items[0].item))
							{
								sortableItems += items.Count;
								unsortableItems -= items.Count;
								SortItem(items[0].item, target);
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		private bool SortAnyTargets()
		{
			foreach (AutosortTarget target in anyTargets)
			{
				foreach (var item in container.container)
				{
					if (target.CanAddItem(item.item))
					{
						SortItem(item.item, target);
						sortableItems++;
						unsortableItems--;
						return true;
					}
				}
			}
			return false;
		}

		private void SortItem(Pickupable pickup, AutosortTarget target)
		{
			container.container.RemoveItem(pickup, true);
			target.AddItem(pickup);
			sortableItems++;

			StartCoroutine(PulseIcon());
		}

		public IEnumerator PulseIcon()
		{
			float t = 0;
			float rate = 0.5f;
			while (t < 1.0)
			{
				t += Time.deltaTime * rate;
				icon.color = Color.Lerp(PulseColor, MainColor, t);
				yield return null;
			}
		}

		private AutosortTarget FindTarget(Pickupable item)
		{
			foreach (AutosortTarget target in singleItemTargets)
			{
				if (target.CanAddItemByItemFilter(item))
				{
					return target;
				}
			}
			foreach (AutosortTarget target in categoryTargets)
			{
				if (target.CanAddItemByCategoryFilter(item))
				{
					return target;
				}
			}
			foreach (AutosortTarget target in anyTargets)
			{
				if (target.CanAddItem(item))
				{
					return target;
				}
			}
			return null;
		}



		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = AutosortLocker.GetPrefab,
				techType = Mod.GetTechType(CustomTechType.AutosortLocker),
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/AutosortLocker",
				displayString = "Autosorter",
				tooltip = "Small, wall-mounted smart-locker that automatically transfers items into linked Autosort Receptacles.",
				techTypeKey = CustomTechType.AutosortLocker.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(Mod.GetAssetPath("AutosortLocker.png"))),
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
						techType = TechType.ComputerChip,
						amount = 1
					},
					new CustomIngredient() {
						techType = TechType.AluminumOxide,
						amount = 2
					}
				}
			});
		}

		public static GameObject GetPrefab()
		{
			GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			prefab.name = "Autosorter";
			
			var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = new Color(1, 0, 0);
			}

			var prefabText = prefab.GetComponentInChildren<Text>();
			var label = prefab.FindChild("Label");
			DestroyImmediate(label);

			var autoSorter = prefab.AddComponent<AutosortLocker>();

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
			autoSorter.background = LockerPrefabShared.CreateBackground(canvas.transform);
			autoSorter.icon = LockerPrefabShared.CreateIcon(autoSorter.background.transform, MainColor, 40);
			autoSorter.text = LockerPrefabShared.CreateText(autoSorter.background.transform, prefabText, MainColor, 0, 14, "Autosorter");

			autoSorter.sortingText = LockerPrefabShared.CreateText(autoSorter.background.transform, prefabText, MainColor, -120, 12, "Sorting...");
			autoSorter.sortingText.alignment = TextAnchor.UpperCenter;

			autoSorter.background.gameObject.SetActive(false);
			autoSorter.icon.gameObject.SetActive(false);
			autoSorter.text.gameObject.SetActive(false);
			autoSorter.sortingText.gameObject.SetActive(false);

			return prefab;
		}
	}
}
