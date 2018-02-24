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

			var amount = Input.GetKey(KeyCode.LeftControl) ? 0.1f : 0.01f;
			var t = GetComponentInChildren<Canvas>().transform;
			if (Input.GetKeyDown(KeyCode.Keypad4))
			{
				t.localPosition += new Vector3(-amount, 0, 0);
				LogPos(t.localPosition);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				t.localPosition += new Vector3(amount, 0, 0);
				LogPos(t.localPosition);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad5))
			{
				t.localPosition += new Vector3(0, -amount, 0);
				LogPos(t.localPosition);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad8))
			{
				t.localPosition += new Vector3(0, amount, 0);
				LogPos(t.localPosition);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad9))
			{
				t.localPosition += new Vector3(0, 0, -amount);
				LogPos(t.localPosition);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad3))
			{
				t.localPosition += new Vector3(0, 0, amount);
				LogPos(t.localPosition);
			}
			/*else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				if (Input.GetKey(KeyCode.LeftShift))
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t.rect.height - amount);
				else
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t.rect.width - amount);
				Logger.Log("canvas rect=" + t.rect);
			}
			else if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				if (Input.GetKey(KeyCode.LeftShift))
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t.rect.height + amount);
				else
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t.rect.width + amount);
				Logger.Log("canvas rect=" + t.rect);
			}*/
		}

		private void LogPos(Vector3 pos)
		{
			Logger.Log("canvas pos=(" + pos.x + "," + pos.y + "," + pos.z + ")");
		}

		private void Initialize()
		{
			Logger.Log("Autosorter Initialize");

			background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Sorter.png"));

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

			var prefabText = prefab.GetComponentInChildren<Text>();
			var label = prefab.FindChild("Label");
			DestroyImmediate(label);

			var autoSorter = prefab.AddComponent<AutosortLocker>();
			var color = new Color(1, 0.17f, 0.17f);

			var canvas = CreateCanvas(prefab.transform);
			autoSorter.background = CreateBackground(canvas.transform);
			autoSorter.icon = CreateIcon(autoSorter.background.transform, color);
			autoSorter.text = CreateText(autoSorter.background.transform, prefabText, color);

			ModUtils.PrintObject(prefab);

			return prefab;
		}

		private static Canvas CreateCanvas(Transform parent)
		{
			var canvas = new GameObject("Canvas", typeof(RectTransform)).AddComponent<Canvas>();
			var t = canvas.transform;
			t.SetParent(parent, false);

			var rt = t as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
			RectTransformExtensions.SetSize(rt, 1, 2);

			t.localPosition = new Vector3(0, 0, 0.345f);
			t.localRotation = new Quaternion(0, 1, 0, 0);

			canvas.scaleFactor = 0.01f;
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.referencePixelsPerUnit = 100;

			var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
			scaler.dynamicPixelsPerUnit = 20;

			var image = canvas.gameObject.AddComponent<Image>();
			image.color = new Color(1, 0, 0, 0.3f);

			return canvas;
		}

		private static Image CreateBackground(Transform parent)
		{
			var background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
			var rt = background.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, 100, 100);
			background.color = new Color(0, 0, 0);

			background.transform.localScale = new Vector3(0.01f, 0.01f, 1);

			return background;
		}

		private static Image CreateIcon(Transform parent, Color color)
		{
			var icon = new GameObject("Text", typeof(RectTransform)).AddComponent<Image>();
			var rt = icon.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, 100, 100);

			icon.color = color;

			return icon;
		}

		private static Text CreateText(Transform parent, Text prefab, Color color)
		{
			var text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
			var rt = text.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(rt, 100, 100);

			text.font = prefab.font;
			text.fontSize = 16;
			text.color = color;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "AUTOSORTER";

			return text;
		}
	}
}
