using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Mod;
using Common.Utility;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;

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

        //private int sortableItems = 0;
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
        [SerializeField]
        private bool sortedItem;

        public bool IsSorting => isSorting;

        private void Awake()
        {
            constructable = GetComponent<Constructable>();
            container = GetComponent<StorageContainer>();
            container.hoverText = "Open autosorter";
            container.storageLabel = "Autosorter";
        }

        private void Update()
        {
            if (!initialized && constructable._constructed && this.transform.parent != null)
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

            SubRoot subRoot = this.gameObject.GetComponentInParent<SubRoot>();
            if (subRoot == null)
            {
                return;
            }

            var allTargets = subRoot.GetComponentsInChildren<AutosortTarget>().ToList();
            foreach (AutosortTarget target in allTargets)
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
            sortedItem = false;
            //sortableItems = 0;
            unsortableItems = container.container.count;

            if (!initialized || container.IsEmpty())
            {
                isSorting = false;
                yield break;
            }

            AccumulateTargets();
            if (NoTargets())
            {
                isSorting = false;
                yield break;
            }

            isSorting = true;
            yield return SortFilteredTargets(false);
            if (sortedItem)
            {
                yield break;
            }

            yield return SortFilteredTargets(true);
            if (sortedItem)
            {
                yield break;
            }

            yield return SortAnyTargets();
            if (sortedItem)
            {
                yield break;
            }

            isSorting = false;
        }

        private bool NoTargets()
        {
            return singleItemTargets.Count <= 0 && categoryTargets.Count <= 0 && anyTargets.Count <= 0;
        }

        private IEnumerator SortFilteredTargets(bool byCategory)
        {
            int callsToCanAddItem = 0;
            const int CanAddItemCallThreshold = 10;

            foreach (AutosortTarget target in byCategory ? categoryTargets : singleItemTargets)
            {
                foreach (AutosorterFilter filter in target.GetCurrentFilters())
                {
                    if (filter.IsCategory() == byCategory)
                    {
                        foreach (TechType techType in filter.Types)
                        {
                            callsToCanAddItem++;
                            IList<InventoryItem> items = container.container.GetItems(techType);
                            if (items != null && items.Count > 0 && target.CanAddItem(items[0].item))
                            {
                                //sortableItems += items.Count;
                                unsortableItems -= items.Count;
                                SortItem(items[0].item, target);
                                sortedItem = true;
                                yield break;
                            }
                            else if (callsToCanAddItem > CanAddItemCallThreshold)
                            {
                                callsToCanAddItem = 0;
                                yield return null;
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator SortAnyTargets()
        {
            int callsToCanAddItem = 0;
            const int CanAddItemCallThreshold = 10;
            foreach (InventoryItem item in container.container.ToList())
            {
                foreach (AutosortTarget target in anyTargets)
                {
                    callsToCanAddItem++;
                    if (target.CanAddItem(item.item))
                    {
                        SortItem(item.item, target);
                        //sortableItems++;
                        unsortableItems--;
                        sortedItem = true;
                        yield break;
                    }
                    else if (callsToCanAddItem > CanAddItemCallThreshold)
                    {
                        callsToCanAddItem = 0;
                        yield return null;
                    }
                }
            }
        }

        private void SortItem(Pickupable pickup, AutosortTarget target)
        {
            container.container.RemoveItem(pickup, true);
            target.AddItem(pickup);
            //sortableItems++;

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

        internal class AutosortLockerBuildable : Buildable
        {
            public AutosortLockerBuildable()
                : base("Autosorter",
                      "Autosort Locker",
                      "Small, wall-mounted smart-locker that automatically transfers items into linked Autosort Receptacles.")
            {
            }

            public override TechGroup GroupForPDA => TechGroup.InteriorModules;

            public override TechCategory CategoryForPDA => TechCategory.InteriorModule;

            public override GameObject GetGameObject()
            {
                GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
                var prefab = GameObject.Instantiate(originalPrefab);

                prefab.name = "Autosorter";

                StorageContainer container = prefab.GetComponent<StorageContainer>();
                container.width = Mod.config.AutosorterWidth;
                container.height = Mod.config.AutosorterHeight;
                container.container.Resize(Mod.config.AutosorterWidth, Mod.config.AutosorterHeight);

                MeshRenderer[] meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    meshRenderer.material.color = new Color(1, 0, 0);
                }

                Text prefabText = prefab.GetComponentInChildren<Text>();
                GameObject label = prefab.FindChild("Label");
                DestroyImmediate(label);

                AutosortLocker autoSorter = prefab.AddComponent<AutosortLocker>();

                Canvas canvas = LockerPrefabShared.CreateCanvas(prefab.transform);
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

            protected override TechData GetBlueprintRecipe()
            {
                return new TechData
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
                        new Ingredient(TechType.ComputerChip, 1),
                        new Ingredient(TechType.AluminumOxide, 2)
                    }
                };
            }

            protected override Atlas.Sprite GetItemSprite()
            {
                return SMLHelper.V2.Utility.ImageUtils.LoadSpriteFromFile(Mod.GetAssetPath("AutosortLocker.png"));
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        public static void AddBuildable()
        {
            var autosorter = new AutosortLockerBuildable();
            autosorter.Patch();
        }
    }


}
