using Common.Mod;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class AutosortTarget : MonoBehaviour
	{
		private bool initialized;
		private Constructable constructable;
		private StorageContainer container;
		private GameObject background;
		private Text text;

		private List<TechType> allowedTypes = new List<TechType>() {
			TechType.Titanium,
			TechType.Glass,
			TechType.Gold,
			TechType.AcidMushroom,
			TechType.ComputerChip,
			TechType.JeweledDiskPiece,
			TechType.AluminumOxide
		};

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = gameObject.GetComponent<StorageContainer>();
		}

		public bool CanSetTechTypes()
		{
			return IsEmpty();
		}

		public void SetTechTypes(List<TechType> types)
		{
			if (!CanSetTechTypes())
			{
				return;
			}
			allowedTypes = types;
			container.container.SetAllowedTechTypes(allowedTypes.ToArray());
			UpdateText();
		}

		private void UpdateText()
		{
			if (text != null)
			{
				string typesText = string.Join("\n", allowedTypes.Select((t) => Language.main.Get(t)).ToArray());
				Logger.Log("UpdateText: " + typesText);
				text.text = typesText;
			}
		}

		public bool IsEmpty()
		{
			return container.container.count == 0;
		}

		internal void AddItem(Pickupable item)
		{
			container.container.AddItem(item);
		}

		internal bool CanAddItem(Pickupable item)
		{
			return container.container.HasRoomFor(item);
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

			if (Input.GetKeyDown(KeyCode.X))
			{
				var x = GameObject.FindObjectOfType<MapRoomFunctionality>();
				ModUtils.PrintObject(x.gameObject);
			}

			container.enabled = true;

			var amount = Input.GetKey(KeyCode.LeftControl) ? 10 : 1;
			var t = background.transform as RectTransform;
			if (Input.GetKeyDown(KeyCode.Keypad4))
			{
				t.anchoredPosition += new Vector2(-amount, 0);
				Logger.Log("background pos=" + t.anchoredPosition);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				t.anchoredPosition += new Vector2(amount, 0);
				Logger.Log("background pos=" + t.anchoredPosition);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad5))
			{
				t.anchoredPosition += new Vector2(0, -amount);
				Logger.Log("background pos=" + t.anchoredPosition);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad8))
			{
				t.anchoredPosition += new Vector2(0, amount);
				Logger.Log("background pos=" + t.anchoredPosition);
			}
			else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				if (Input.GetKey(KeyCode.LeftShift))
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t.rect.height - amount);
				else
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t.rect.width - amount);
				Logger.Log("background rect=" + t.rect);
			}
			else if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				if (Input.GetKey(KeyCode.LeftShift))
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t.rect.height + amount);
				else
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t.rect.width + amount);
				Logger.Log("background rect=" + t.rect);
			}
		}

		private void Initialize()
		{
			Logger.Log("Autosort Target Initialize");
			var prefabText = gameObject.GetComponentInChildren<Text>();
			var label = gameObject.FindChild("Label");
			var labelPos = label.transform.position;
			DestroyImmediate(label);

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

			background = screen.FindChild("background");
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
			icon.rectTransform.localPosition = new Vector3(0, 120, 0);
			icon.color = prefabText.color;
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Receptacle.png"));
			RectTransformExtensions.SetSize(icon.rectTransform, 62, 62);

			text = new GameObject("text", typeof(RectTransform)).AddComponent<Text>();
			rt = text.rectTransform;
			rt.localScale = new Vector3(10, 10, 10);
			rt.localPosition = new Vector3(0, 0, 0);
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), background.transform);
			RectTransformExtensions.SetSize(rt, 189, 351);
			text.color = prefabText.color;
			text.font = prefabText.font;
			text.fontSize = 30;
			text.alignment = TextAnchor.MiddleCenter;

			var list = screen.FindChild("list");
			list.transform.SetAsLastSibling();
			list.SetActive(false);

			ModUtils.PrintObject(screen);

			UpdateText();
			initialized = true;
		}



		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = AutosortTarget.GetPrefab,
				techType = Mod.GetTechType(CustomTechType.AutosortTarget),
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/AutosortTarget",
				displayString = "Autosort Receptacle",
				tooltip = "Wall locker linked to an Autosorter that stores the items.",
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
						techType = TechType.UraniniteCrystal,
						amount = 1
					},
					new CustomIngredient() {
						techType = TechType.Magnetite,
						amount = 1
					}
				}
			});
		}

		public static GameObject GetPrefab()
		{
			GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			prefab.name = "AutosortReceptacle";

			var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
			}

			prefab.AddComponent<AutosortTarget>();

			return prefab;
		}
	}
}
