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

		[SerializeField]
		private GameObject background;
		[SerializeField]
		private Text text;
		[SerializeField]
		private HashSet<TechType> allowedTypes;

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = gameObject.GetComponent<StorageContainer>();
		}

		public void SetTechTypes(HashSet<TechType> types)
		{
			allowedTypes = types;
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

		internal void AddItem(Pickupable item)
		{
			container.container.AddItem(item);
		}

		internal bool CanAddItem(Pickupable item)
		{
			return allowedTypes.Contains(item.GetTechType()) && container.container.HasRoomFor(item);
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

			container.enabled = true;

			/*var amount = Input.GetKey(KeyCode.LeftControl) ? 10 : 1;
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
			}*/
		}

		private void Initialize()
		{
			Logger.Log("Autosort Target Initialize");
			Logger.Log("Text=" + text + ", Background=" + background);
			SetTechTypes(new HashSet<TechType>() {
				TechType.Titanium,
				TechType.Glass,
				TechType.Gold,
				TechType.AcidMushroom,
				TechType.ComputerChip,
				TechType.JeweledDiskPiece,
				TechType.AluminumOxide
			});
			initialized = true;
			ModUtils.PrintObject(gameObject);
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

			var autosortTarget = prefab.AddComponent<AutosortTarget>();

			var prefabText = prefab.GetComponentInChildren<Text>();
			var label = prefab.FindChild("Label");
			//label.SetActive(false);
			DestroyImmediate(label);

			var mapRoomPrefab = Resources.Load<GameObject>("Submarine/Build/MapRoomFunctionality");
			var mapRoomScreenPrefab = mapRoomPrefab.GetComponentInChildren<uGUI_MapRoomScanner>();
			var mapRoomScreen = GameObject.Instantiate(mapRoomScreenPrefab);
			var screen = mapRoomScreen.gameObject;
			DestroyImmediate(screen.GetComponent<uGUI_MapRoomScanner>());

			var canvasScalar = prefab.AddComponent<CanvasScaler>();
			canvasScalar.dynamicPixelsPerUnit = 30;

			screen.name = "LockerScreen";
			screen.transform.SetParent(prefab.transform, false);
			var t = screen.transform;
			t.localPosition = new Vector3(0, 0, 0.375f);
			t.localRotation = new Quaternion(0, 1, 0, 0);

			var canvas = screen.GetComponent<Canvas>();
			Logger.Log("Canvas " + canvas.scaleFactor + ", " + canvas.renderMode + ", " + canvas.referencePixelsPerUnit);

			DestroyImmediate(screen.FindChild("scanning"));
			DestroyImmediate(screen.FindChild("foreground"));

			autosortTarget.background = screen.FindChild("background");
			var rt = autosortTarget.background.transform as RectTransform;
			rt.localScale = new Vector3(0.3f, 0.3f, 0);
			rt.anchoredPosition = new Vector2(0, 2);
			RectTransformExtensions.SetSize(rt, 188, 391);
			var image = autosortTarget.background.GetComponent<Image>();
			image.color = new Color(0, 0, 0, 1);
			var sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
			image.sprite = sprite;
			image.type = Image.Type.Sliced;

			var icon = new GameObject("Icon", typeof(RectTransform)).AddComponent<Image>();
			icon.transform.SetParent(autosortTarget.background.transform, false);
			icon.rectTransform.localPosition = new Vector3(0, 120, 0);
			icon.color = prefabText.color;
			icon.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Receptacle.png"));
			RectTransformExtensions.SetSize(icon.rectTransform, 62, 62);

			autosortTarget.text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
			rt = autosortTarget.text.rectTransform;
			rt.localScale = new Vector3(10, 10, 10);
			rt.localPosition = new Vector3(0, 0, 0);
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), autosortTarget.background.transform);
			RectTransformExtensions.SetSize(rt, 189, 351);
			autosortTarget.text.color = prefabText.color;
			autosortTarget.text.font = prefabText.font;
			autosortTarget.text.fontSize = 30;
			autosortTarget.text.alignment = TextAnchor.MiddleCenter;

			var list = screen.FindChild("list");
			list.transform.SetAsLastSibling();
			list.SetActive(false);
			//DestroyImmediate(list);
			

			ModUtils.PrintObject(prefab);

			return prefab;
		}
	}
}
