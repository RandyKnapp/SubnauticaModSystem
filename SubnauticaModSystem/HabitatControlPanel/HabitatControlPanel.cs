using Common.Mod;
using Common.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace HabitatControlPanel
{
	public class HabitatControlPanel : MonoBehaviour
	{
		private bool initialized;
		private Constructable constructable;

		[SerializeField]
		private Image background;
		[SerializeField]
		private GameObject powerCellSlot;

		private void Awake()
		{
			constructable = GetComponent<Constructable>();
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

			if (SaveLoadManager.main != null && SaveLoadManager.main.isSaving && !Mod.IsSaving() && !Mod.NeedsSaving())
			{
				Mod.SetNeedsSaving();
			}
			if (SaveLoadManager.main != null && !SaveLoadManager.main.isSaving && !Mod.IsSaving() && Mod.NeedsSaving())
			{
				Mod.Save();
			}

			PositionPowerCell();
			PositionScreen();
		}
		
		private void Initialize()
		{
			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Background.png"));

			Destroy(transform.Find("mesh").gameObject);

			initialized = true;
		}

		public void Save(SaveData saveData)
		{
			var prefabIdentifier = GetComponent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			var entry = new SaveDataEntry() { Id = id };
			saveData.Entries.Add(entry);
		}

		public void PositionPowerCell()
		{
			var t = powerCellSlot.transform;
			var amount = 0.01f;

			if (Input.GetKeyDown(KeyCode.Keypad8))
			{
				t.localPosition += new Vector3(0, amount, 0);
				PrintPowerCellPosition();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad5))
			{
				t.localPosition += new Vector3(0, -amount, 0);
				PrintPowerCellPosition();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				t.localPosition += new Vector3(amount, 0, 0);
				PrintPowerCellPosition();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad4))
			{
				t.localPosition += new Vector3(-amount, 0, 0);
				PrintPowerCellPosition();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad1))
			{
				t.localPosition += new Vector3(0, 0, amount);
				PrintPowerCellPosition();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad7))
			{
				t.localPosition += new Vector3(0, 0, -amount);
				PrintPowerCellPosition();
			}

			var rotAmount = 1.0f;
			if (Input.GetKeyDown(KeyCode.Keypad9))
			{
				t.localEulerAngles += new Vector3(-rotAmount, 0, 0);
				PrintPowerCellRotation();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad3))
			{
				t.localEulerAngles += new Vector3(rotAmount, 0, 0);
				PrintPowerCellRotation();
			}

			var scaleAmount = 0.1f;
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				t.localScale += new Vector3(scaleAmount, scaleAmount, scaleAmount);
				PrintPowerCellScale();
			}
			else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				t.localScale += new Vector3(-scaleAmount, -scaleAmount, -scaleAmount);
				PrintPowerCellScale();
			}
		}

		private void PrintPowerCellPosition()
		{
			var t = powerCellSlot.transform;
			var p = t.localPosition;
			var x = Math.Round(p.x, 2);
			var y = Math.Round(p.y, 2);
			var z = Math.Round(p.z, 2);
			Logger.Log("PowerCell pos=(" + x + "," + y + "," + z + ")");
		}

		private void PrintPowerCellRotation()
		{
			var t = powerCellSlot.transform;
			var r = t.localEulerAngles;
			Logger.Log("PowerCell rot=(" + r.x + "," + r.y + "," + r.z + ")");
		}

		private void PrintPowerCellScale()
		{
			var t = powerCellSlot.transform;
			var s = t.localScale;
			Logger.Log("PowerCell scale=(" + s.x + "," + s.y + "," + s.z + ")");
		}

		private void PositionScreen()
		{
			var t = background.transform as RectTransform;
			var amount = 1f;
			var w = t.rect.width;
			var h = t.rect.height;

			if (Input.GetKeyDown(KeyCode.L))
			{
				RectTransformExtensions.SetSize(t, w + amount, h);
				PrintScreenSize();
			}
			else if (Input.GetKeyDown(KeyCode.J))
			{
				RectTransformExtensions.SetSize(t, w - amount, h);
				PrintScreenSize();
			}
			else if (Input.GetKeyDown(KeyCode.I))
			{
				RectTransformExtensions.SetSize(t, w, h + amount);
				PrintScreenSize();
			}
			else if (Input.GetKeyDown(KeyCode.K))
			{
				RectTransformExtensions.SetSize(t, w, h - amount);
				PrintScreenSize();
			}

			var c = background.canvas.transform;
			if (Input.GetKeyDown(KeyCode.U))
			{
				c.localPosition += new Vector3(0, 0, -0.01f);
				Logger.Log("Screen depth=" + c.localPosition.z);
			}
			else if (Input.GetKeyDown(KeyCode.M))
			{
				c.localPosition += new Vector3(0, 0, 0.01f);
				Logger.Log("Screen depth=" + c.localPosition.z);
			}

			if (Input.GetKeyDown(KeyCode.X))
			{
				var active = background.gameObject.activeSelf;
				background.gameObject.SetActive(!active);
			}
		}
		
		private void PrintScreenSize()
		{
			var t = background.transform as RectTransform;
			var w = t.rect.width;
			var h = t.rect.height;
			Logger.Log("Screen size=(" + w + "," + h + ")");
		}




		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = HabitatControlPanel.GetPrefab,
				techType = (TechType)CustomTechType.HabitatControlPanel,
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/HabitatControlPanel",
				displayString = "Habitat Control Panel",
				tooltip = "TODO TOOLTIP",
				techTypeKey = CustomTechType.HabitatControlPanel.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(Mod.GetAssetPath("BlueprintIcon.png"))),
				recipe = new List<CustomIngredient>
				{
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 1
					},
					new CustomIngredient() {
						techType = TechType.ComputerChip,
						amount = 1
					},
					new CustomIngredient() {
						techType = TechType.WiringKit,
						amount = 1
					}
				}
			});
		}

		public static GameObject GetPrefab()
		{
			Logger.Log("GetPrefab for HabitatControlPanel");
			GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/PictureFrame");
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			prefab.name = "HabitatControlPanel";
			ModUtils.PrintObject(prefab);

			GameObject.Destroy(prefab.GetComponent<PictureFrame>());
			var screen = prefab.transform.Find("Screen").gameObject;
			screen.transform.localEulerAngles = new Vector3(0, 0, 90);
			screen.SetActive(false);
			var mesh = prefab.transform.Find("mesh").gameObject;
			mesh.transform.localEulerAngles = new Vector3(0, 0, 90);
			var trigger = prefab.transform.Find("Trigger").gameObject;
			trigger.transform.localEulerAngles = new Vector3(0, 0, 90);

			GameObject seamothPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth");
			ModUtils.PrintObject(seamothPrefab);

			GameObject powerCellSlotPrefab = GetPowerCellSlotModel();
			GameObject powerCellSlot = GameObject.Instantiate(powerCellSlotPrefab);
			powerCellSlot.transform.SetParent(prefab.transform, false);
			powerCellSlot.transform.localPosition = new Vector3(0.44f, -0.7f, -0.13f);
			powerCellSlot.transform.localEulerAngles = new Vector3(61, 180, 0);

			//GameObject beaconPrefab = Resources.Load<GameObject>("WorldEntities/Tools/Beacon");
			//ModUtils.PrintObject(beaconPrefab);

			var controlPanel = prefab.AddComponent<HabitatControlPanel>();
			controlPanel.powerCellSlot = powerCellSlot;
			controlPanel.background = CreateScreen(prefab.transform);

			return prefab;
		}

		private static Image CreateScreen(Transform parent)
		{
			var canvas = LockerPrefabShared.CreateCanvas(parent);
			canvas.transform.localPosition = new Vector3(0, 0, 0.02f);

			var background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
			var rt = background.rectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), canvas.transform);
			RectTransformExtensions.SetSize(rt, 178, 298);
			background.transform.localScale = new Vector3(0.01f, 0.01f, 1);

			background.type = Image.Type.Simple;
			background.color = new Color(1, 1, 1);
			//background.gameObject.SetActive(false);

			return background;
		}

		private static GameObject GetPowerCellSlotModel()
		{
			GameObject seamothPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth");

			GameObject powerCellSlot = new GameObject("PowerCellSlot");
			var offset = new Vector3(0, 0, 1.72f);

			GameObject batteryInput = GameObject.Instantiate(seamothPrefab.transform.Find("BatteryInput").gameObject);
			batteryInput.transform.localPosition += offset;
			batteryInput.transform.SetParent(powerCellSlot.transform);

			GameObject batterySlot = GameObject.Instantiate(seamothPrefab.transform.Find("BatterySlot").gameObject);
			batterySlot.transform.localPosition += offset;
			batterySlot.transform.SetParent(powerCellSlot.transform);

			var model = seamothPrefab.transform.Find("Model");
			var Submersible_SeaMoth = model.Find("Submersible_SeaMoth");
			var Submersible_seaMoth_geo = Submersible_SeaMoth.Find("Submersible_seaMoth_geo");
			var seamoth_power_cell_slot_geo = Submersible_seaMoth_geo.Find("seamoth_power_cell_slot_geo");
			var engine_power_cell_01 = Submersible_seaMoth_geo.Find("engine_power_cell_01");
			var engine_power_cell_ion = Submersible_seaMoth_geo.Find("engine_power_cell_ion");

			GameObject slotGeo = GameObject.Instantiate(seamoth_power_cell_slot_geo.gameObject);
			slotGeo.transform.localPosition += offset;
			slotGeo.transform.SetParent(powerCellSlot.transform);
			slotGeo.name = "SlotGeo";

			GameObject powerCell = GameObject.Instantiate(engine_power_cell_01.gameObject);
			powerCell.transform.localPosition += offset;
			powerCell.transform.SetParent(powerCellSlot.transform);
			powerCell.name = "PowerCellGeo";

			GameObject ionPowerCell = GameObject.Instantiate(engine_power_cell_ion.gameObject);
			ionPowerCell.transform.localPosition += offset;
			ionPowerCell.transform.SetParent(powerCellSlot.transform);
			ionPowerCell.name = "IonPowerCellGeo";
			ionPowerCell.SetActive(false);

			return powerCellSlot;
		}
	}
}
