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
		[SerializeField]
		private BoxCollider powerCellTrigger;

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

			PositionTrigger();
			DrawBoxCollider();
		}
		
		private void Initialize()
		{
			background.gameObject.SetActive(true);
			background.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Background.png"));

			Destroy(transform.Find("mesh").gameObject);

			var handTarget = powerCellTrigger.gameObject.AddComponent<GenericHandTarget>();
			handTarget.onHandHover = new HandTargetEvent();
			handTarget.onHandClick = new HandTargetEvent();
			handTarget.onHandHover.AddListener(OnPowerCellHandHover);
			handTarget.onHandClick.AddListener(OnPowerCellHandClick);

			initialized = true;
		}

		private void OnPowerCellHandHover(HandTargetEventData eventData)
		{
			HandReticle main = HandReticle.main;
			main.SetIcon(HandReticle.IconType.Hand);
			main.SetInteractTextRaw("SDOIJOSDIJF", "adij");
		}

		private void OnPowerCellHandClick(HandTargetEventData eventData)
		{
			Logger.Log("Power Cell Click");
		}

		public void Save(SaveData saveData)
		{
			var prefabIdentifier = GetComponent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			var entry = new SaveDataEntry() { Id = id };
			saveData.Entries.Add(entry);
		}

		public void PositionTrigger()
		{
			var t = powerCellTrigger.transform;
			var bc = powerCellTrigger;
			var amount = 0.01f;

			if (Input.GetKeyDown(KeyCode.Keypad8))
			{
				t.localPosition += new Vector3(0, amount, 0);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad5))
			{
				t.localPosition += new Vector3(0, -amount, 0);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				t.localPosition += new Vector3(amount, 0, 0);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad4))
			{
				t.localPosition += new Vector3(-amount, 0, 0);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad1))
			{
				t.localPosition += new Vector3(0, 0, amount);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad7))
			{
				t.localPosition += new Vector3(0, 0, -amount);
				PrintBoxCollider();
			}

			var scaleAmount = 0.01f;
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				bc.size += new Vector3(scaleAmount, scaleAmount, scaleAmount);
				PrintBoxCollider();
			}
			else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				bc.size -= new Vector3(scaleAmount, scaleAmount, scaleAmount);
				PrintBoxCollider();
			}
		}

		private void PrintBoxCollider()
		{
			var bc = powerCellTrigger;
			var t = powerCellTrigger.transform;
			Logger.Log("Trigger p=" + t.localPosition + " s=" + bc.bounds.size);
		}

		private void DrawBoxCollider()
		{
			var bc = powerCellTrigger;
			var t = bc.transform;
			var verts = new Vector3[8];
			var worldTransform = bc.transform.localToWorldMatrix;
			var storedRotation = bc.transform.rotation;
			bc.transform.rotation = Quaternion.identity;

			var extents = bc.bounds.extents;
			verts[0] = worldTransform.MultiplyPoint3x4(extents);
			verts[1] = worldTransform.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, extents.z));
			verts[2] = worldTransform.MultiplyPoint3x4(new Vector3(extents.x, extents.y, -extents.z));
			verts[3] = worldTransform.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, -extents.z));
			verts[4] = worldTransform.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, extents.z));
			verts[5] = worldTransform.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, extents.z));
			verts[6] = worldTransform.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, -extents.z));
			verts[7] = worldTransform.MultiplyPoint3x4(-extents);

			bc.transform.rotation = storedRotation;

			DrawLine(verts, 0, 1);
			DrawLine(verts, 1, 2);
			DrawLine(verts, 2, 3);
			DrawLine(verts, 3, 0);

			DrawLine(verts, 0, 4);
			DrawLine(verts, 1, 5);
			DrawLine(verts, 2, 6);
			DrawLine(verts, 3, 7);

			DrawLine(verts, 4, 5);
			DrawLine(verts, 5, 6);
			DrawLine(verts, 6, 7);
			DrawLine(verts, 7, 4);
		}

		private void DrawLine(Vector3[] verts, int a, int b)
		{
			Debug.DrawLine(verts[a], verts[b], Color.green, 0);
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

			GameObject.Destroy(prefab.GetComponent<PictureFrame>());
			var screen = prefab.transform.Find("Screen").gameObject;
			screen.transform.localEulerAngles = new Vector3(0, 0, 90);
			screen.SetActive(false);
			var mesh = prefab.transform.Find("mesh").gameObject;
			mesh.transform.localEulerAngles = new Vector3(0, 0, 90);
			var trigger = prefab.transform.Find("Trigger").gameObject;
			trigger.transform.localEulerAngles = new Vector3(0, 0, 90);
			GameObject.Destroy(trigger.GetComponent<GenericHandTarget>());

			GameObject powerCellSlotPrefab = GetPowerCellSlotModel();
			GameObject powerCellSlot = GameObject.Instantiate(powerCellSlotPrefab);
			Destroy(powerCellSlotPrefab);
			powerCellSlot.transform.SetParent(prefab.transform, false);
			powerCellSlot.transform.localPosition = new Vector3(0.44f, -0.7f, -0.12f);
			powerCellSlot.transform.localEulerAngles = new Vector3(61, 180, 0);

			powerCellSlot.SetActive(false);
			var sky = powerCellSlot.AddComponent<SkyApplier>();
			sky.dynamic = true;
			sky.renderers = sky.GetAllComponentsInChildren<MeshRenderer>();
			sky.anchorSky = Skies.BaseInterior;
			powerCellSlot.SetActive(true);

			//GameObject beaconPrefab = Resources.Load<GameObject>("WorldEntities/Tools/Beacon");
			//ModUtils.PrintObject(beaconPrefab);

			var controlPanel = prefab.AddComponent<HabitatControlPanel>();
			controlPanel.powerCellSlot = powerCellSlot;
			controlPanel.background = CreateScreen(prefab.transform);

			var slotGeo = powerCellSlot.transform.GetChild(0).gameObject;
			var collider = slotGeo.AddComponent<BoxCollider>();
			controlPanel.powerCellTrigger = collider;

			ModUtils.PrintObject(prefab);

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
			background.gameObject.SetActive(false);

			return background;
		}

		private static GameObject GetPowerCellSlotModel()
		{
			GameObject seamothPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth");

			GameObject powerCellSlot = new GameObject("PowerCellSlot");
			var offset = new Vector3(0, 0, 1.72f);

			/*GameObject batteryInput = GameObject.Instantiate(seamothPrefab.transform.Find("BatteryInput").gameObject);
			batteryInput.transform.localPosition += offset;
			batteryInput.transform.SetParent(powerCellSlot.transform, false);*/

			/*GameObject batterySlot = GameObject.Instantiate(seamothPrefab.transform.Find("BatterySlot").gameObject);
			batterySlot.transform.localPosition += offset;
			batterySlot.transform.SetParent(powerCellSlot.transform, false);*/

			var model = seamothPrefab.transform.Find("Model");
			var Submersible_SeaMoth = model.Find("Submersible_SeaMoth");
			var Submersible_seaMoth_geo = Submersible_SeaMoth.Find("Submersible_seaMoth_geo");
			var seamoth_power_cell_slot_geo = Submersible_seaMoth_geo.Find("seamoth_power_cell_slot_geo");
			var engine_power_cell_01 = Submersible_seaMoth_geo.Find("engine_power_cell_01");
			var engine_power_cell_ion = Submersible_seaMoth_geo.Find("engine_power_cell_ion");

			GameObject slotGeo = GameObject.Instantiate(seamoth_power_cell_slot_geo.gameObject);
			slotGeo.transform.localPosition += offset;
			slotGeo.transform.SetParent(powerCellSlot.transform, false);
			slotGeo.name = "SlotGeo";

			GameObject powerCell = GameObject.Instantiate(engine_power_cell_01.gameObject);
			powerCell.transform.localPosition += offset;
			powerCell.transform.SetParent(powerCellSlot.transform, false);
			powerCell.name = "PowerCellGeo";
			powerCell.SetActive(false);

			GameObject ionPowerCell = GameObject.Instantiate(engine_power_cell_ion.gameObject);
			ionPowerCell.transform.localPosition += offset;
			ionPowerCell.transform.SetParent(powerCellSlot.transform, false);
			ionPowerCell.name = "IonPowerCellGeo";
			ionPowerCell.SetActive(false);

			return powerCellSlot;
		}
	}
}
