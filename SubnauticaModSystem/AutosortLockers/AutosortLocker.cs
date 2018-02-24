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
		private bool initialized;
		private Constructable constructable;
		private StorageContainer container;
		private List<AutosortTarget> targets = new List<AutosortTarget>();

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

			sortingText.gameObject.SetActive(isSorting);
		}

		private void Initialize()
		{
			Logger.Log("Autosorter Initialize");

			background.gameObject.SetActive(true);
			icon.gameObject.SetActive(true);
			text.gameObject.SetActive(true);

			background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
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

			BaseRoot baseRoot = gameObject.GetComponentInParent<BaseRoot>();
			if (baseRoot == null)
			{
				return;
			}

			targets = baseRoot.GetComponentsInChildren<AutosortTarget>().ToList();
		}

		private bool Sort()
		{
			if (!initialized || container.IsEmpty())
			{
				return false;
			}

			AccumulateTargets();
			if (targets.Count <= 0)
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
					return true;
				}
			}

			return false;
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
			var color = new Color(1, 0.2f, 0.2f);

			var canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
			autoSorter.background = LockerPrefabShared.CreateBackground(canvas.transform);
			autoSorter.icon = LockerPrefabShared.CreateIcon(autoSorter.background.transform, color, 40);
			autoSorter.text = LockerPrefabShared.CreateText(autoSorter.background.transform, prefabText, color, 0, 14, "Autosorter");
			autoSorter.sortingText = LockerPrefabShared.CreateText(autoSorter.background.transform, prefabText, color, -20, 12, "Sorting...");

			autoSorter.background.gameObject.SetActive(false);
			autoSorter.icon.gameObject.SetActive(false);
			autoSorter.text.gameObject.SetActive(false);
			autoSorter.sortingText.gameObject.SetActive(false);

			return prefab;
		}
	}
}
