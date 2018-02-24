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
		}

		private void Initialize()
		{
			Logger.Log("Autosorter Initialize");
			var prefabText = gameObject.GetComponentInChildren<Text>();
			var label = gameObject.FindChild("Label");
			if (label != null)
			{
				DestroyImmediate(label);
			}

			var mapRoomPrefab = Resources.Load<GameObject>("Submarine/Build/MapRoomFunctionality");
			var mapRoomScreenPrefab = mapRoomPrefab.GetComponentInChildren<uGUI_MapRoomScanner>();
			var mapRoomScreen = GameObject.Instantiate(mapRoomScreenPrefab);
			var screen = mapRoomScreen.gameObject;
			mapRoomPrefab = null;
			mapRoomScreenPrefab = null;
			DestroyImmediate(screen.GetComponent<uGUI_MapRoomScanner>());

			var canvasScalar = gameObject.AddComponent<CanvasScaler>();
			canvasScalar.dynamicPixelsPerUnit = 20;

			screen.transform.SetParent(transform, false);
			var t = screen.transform;
			t.localPosition = new Vector3(0, 0, 0.375f);
			t.localRotation = new Quaternion(0, 1, 0, 0);

			DestroyImmediate(screen.FindChild("scanning"));
			DestroyImmediate(screen.FindChild("foreground"));

			var background = screen.FindChild("background");
			var rt = background.transform as RectTransform;
			rt.localScale = new Vector3(0.3f, 0.3f, 0);
			rt.anchoredPosition = new Vector2(0, 2);
			RectTransformExtensions.SetSize(rt, 188, 391);
			var image = background.GetComponent<Image>();
			image.color = new Color(0, 0, 0, 1);
			var sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
			image.sprite = sprite;
			image.type = Image.Type.Sliced;

			var icon = new GameObject("icon", typeof(RectTransform)).AddComponent<Image>();
			icon.transform.SetParent(background.transform, false);
			icon.rectTransform.localPosition = new Vector3(0, 50, 0);
			icon.color = new Color(1, 0.17f, 0.17f);
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Sorter.png"));
			RectTransformExtensions.SetSize(icon.rectTransform, 62, 62);

			var text = new GameObject("text", typeof(RectTransform)).AddComponent<Text>();
			rt = text.rectTransform;
			rt.localScale = new Vector3(10, 10, 10);
			rt.localPosition = new Vector3(0, -10, 0);
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), background.transform);
			RectTransformExtensions.SetSize(rt, 189, 50);
			text.color = icon.color;
			text.font = prefabText.font;
			text.fontSize = 30;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "AUTOSORTER";
			text.horizontalOverflow = HorizontalWrapMode.Overflow;

			var list = screen.FindChild("list");
			DestroyImmediate(list);

			ModUtils.PrintObject(screen);

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

			return prefab;
		}
	}
}
