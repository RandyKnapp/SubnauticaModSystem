using Common.Mod;
using Common.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class AutosortLocker : MonoBehaviour
	{
		private bool initialized;
		private Constructable constructable;
		private StorageContainer container;
		private List<AutosortTarget> targets = new List<AutosortTarget>();
		
		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = GetComponent<StorageContainer>();
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
		}

		private void Initialize()
		{
			Logger.Log("Autosorter Initialize");
			var label = gameObject.FindChild("Label");
			var labelPos = label.transform.position;
			DestroyImmediate(label);

			initialized = true;
		}

		private IEnumerator Start()
		{
			while (true)
			{
				yield return new WaitForSeconds(Mod.config.SortInterval);
				
				Sort();
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

		private void Sort()
		{
			if (!initialized || container.IsEmpty())
			{
				return;
			}

			AccumulateTargets();
			if (targets.Count <= 0)
			{
				return;
			}

			Pickupable item = GetFirstItem();
			container.container.RemoveItem(item, true);

			AutosortTarget target = FindTarget(item);
			if (target != null)
			{
				target.AddItem(item);
			}
		}

		private Pickupable GetFirstItem()
		{
			foreach (var item in container.container)
			{
				return item.item;
			}

			return null;
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

			prefab.AddComponent<AutosortLocker>();

			ModUtils.PrintObject(prefab);

			return prefab;
		}
	}
}
