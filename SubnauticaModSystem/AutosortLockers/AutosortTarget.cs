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

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
			container = gameObject.GetComponent<StorageContainer>();
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

			container.enabled = false;

			var amount = Input.GetKey(KeyCode.LeftControl) ? 10 : 1;
			var t = background.transform as RectTransform;
			if (Input.GetKeyDown(KeyCode.J))
			{
				t.anchoredPosition += new Vector2(-amount, 0);
				Logger.Log("background pos=" + t.anchoredPosition);
			}
			else if (Input.GetKeyDown(KeyCode.L))
			{
				t.anchoredPosition += new Vector2(amount, 0);
				Logger.Log("background pos=" + t.anchoredPosition);
			}
			else if (Input.GetKeyDown(KeyCode.K))
			{
				t.anchoredPosition += new Vector2(0, -amount);
				Logger.Log("background pos=" + t.anchoredPosition);
			}
			else if (Input.GetKeyDown(KeyCode.I))
			{
				t.anchoredPosition += new Vector2(0, amount);
				Logger.Log("background pos=" + t.anchoredPosition);
			}
			else if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				if (Input.GetKey(KeyCode.LeftShift))
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, t.rect.height - amount);
				else
					t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t.rect.width - amount);
				Logger.Log("background rect=" + t.rect);
			}
			else if (Input.GetKeyDown(KeyCode.KeypadMinus))
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
			var label = gameObject.FindChild("Label");
			var labelPos = label.transform.position;
			DestroyImmediate(label);

			var mapRoomPrefab = Resources.Load<GameObject>("Submarine/Build/MapRoomFunctionality");
			var mapRoomScreenPrefab = mapRoomPrefab.GetComponentInChildren<uGUI_MapRoomScanner>();
			var mapRoomScreen = GameObject.Instantiate(mapRoomScreenPrefab);
			var screen = mapRoomScreen.gameObject;
			DestroyImmediate(mapRoomScreenPrefab);

			screen.transform.SetParent(transform, false);
			var t = screen.transform;
			t.localPosition = new Vector3(0, 0, 0.375f);
			t.localRotation = new Quaternion(0, 1, 0, 0);

			DestroyImmediate(screen.FindChild("scanning"));
			DestroyImmediate(screen.FindChild("foreground"));

			background = screen.FindChild("background");
			var rt = background.transform as RectTransform;
			rt.localScale = new Vector3(0.3f, 0.3f, 0);
			rt.anchoredPosition = new Vector2(-2, 2);
			RectTransformExtensions.SetSize(rt, 210, 391);
			var image = background.GetComponent<Image>();
			image.color = new Color(0, 0, 0, 1);
			var sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
			image.sprite = sprite;
			image.type = Image.Type.Sliced;

			ModUtils.PrintObject(screen);

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
