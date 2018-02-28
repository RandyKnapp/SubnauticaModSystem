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
		private List<AutosortTarget> targets = new List<AutosortTarget>();
		private List<AutosortTarget> anyTargets = new List<AutosortTarget>();

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
			targets.Clear();
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
			int itemsToSort = GetSortableItemCount();
			int unsortableItems = GetUnsortableItemCount();
			if (isSorting)
			{
				output += "Sorting (" + (itemsToSort + 1) + ")";
			}
			if (unsortableItems > 0)
			{
				output += (isSorting ? "\n" : "") + "Unsorted Items: " + unsortableItems;
			}
			if (!isSorting && unsortableItems == 0 && itemsToSort == 0)
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
				yield return new WaitForSeconds(Mod.config.SortInterval);
				
				isSorting = Sort();
			}
		}

		private void AccumulateTargets()
		{
			targets.Clear();
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
						targets.Add(target);
					}
				}
			}
		}

		private bool Sort()
		{
			if (!initialized || container.IsEmpty())
			{
				return false;
			}

			AccumulateTargets();
			if (targets.Count <= 0 && anyTargets.Count <= 0)
			{
				return false;
			}

			foreach (InventoryItem item in container.container)
			{
				Pickupable pickup = item.item;
				AutosortTarget target = FindTarget(pickup);
				if (target != null)
				{
					container.container.RemoveItem(pickup, true);
					target.AddItem(pickup);

					StartCoroutine(PulseIcon());
					return true;
				}
			}

			return false;
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

		private int GetSortableItemCount()
		{
			int count = 0;
			foreach (InventoryItem item in container.container)
			{
				Pickupable pickup = item.item;
				AutosortTarget target = FindTarget(pickup);
				if (target != null)
				{
					count++;
				}
			}

			return count;
		}

		private int GetUnsortableItemCount()
		{
			return container.container.count - GetSortableItemCount();
		}

		private AutosortTarget FindTarget(Pickupable item)
		{
			foreach (AutosortTarget target in targets)
			{
				if (target.CanAddItem(item))
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
