using Common.Mod;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BaseTeleporters
{
	class BaseTeleporter : MonoBehaviour
	{
		///////////////////////////////////////////////////////////////////////////////////////////
		public static void AddBuildable()
		{
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				getPrefab = BaseTeleporter.GetPrefab,
				techType = (TechType)CustomTechType.BaseTeleporter,
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/BaseTeleporter",
				displayString = "Teleporter",
				tooltip = "TODO TOOLTIP",
				techTypeKey = CustomTechType.BaseTeleporter.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(Mod.GetAssetPath("BaseTeleporter.png"))),
				recipe = new List<CustomIngredient> {
					new CustomIngredient() {
						techType = TechType.PrecursorIonCrystal,
						amount = 1
					}
				}
			});
		}

		public static GameObject GetPrefab()
		{
			var alienTeleporterPrefab = Resources.Load<GameObject>("WorldEntities/Environment/Precursor/MountainIsland/Precursor_Mountain_Teleporter_ToFloatingIsland");
			var baseTeleporterPrefab = new GameObject("BaseTeleporter");
			var plantPotPrefab = Resources.Load<GameObject>("Submarine/Build/PlanterPot");
			var aquariumPrefab = Resources.Load<GameObject>("Submarine/Build/Aquarium");

			const float padHeight = 0.1f;
			const float fieldHeight = 1.5f;

			var warpCollider = ModUtils.GetChildByName(alienTeleporterPrefab, "WarpCollider");
			Console.WriteLine("BoxCollider--");
			ModUtils.PrintObjectFields(warpCollider.GetComponent<BoxCollider>());
			Console.WriteLine("PrecursorTeleporterCollider--");
			ModUtils.PrintObjectFields(warpCollider.GetComponent<PrecursorTeleporterCollider>());

			var constructable = ModUtils.CopyComponent(plantPotPrefab.GetComponent<Constructable>(), baseTeleporterPrefab);
			constructable.techType = (TechType)CustomTechType.BaseTeleporter;

			var techTag = ModUtils.CopyComponent(plantPotPrefab.GetComponent<TechTag>(), baseTeleporterPrefab);
			techTag.type = (TechType)CustomTechType.BaseTeleporter;

			var prefabIdentifier = ModUtils.CopyComponent(plantPotPrefab.GetComponent<PrefabIdentifier>(), baseTeleporterPrefab);
			prefabIdentifier.ClassId = "Submarine/Build/BaseTeleporter";

			var constructBounds = ModUtils.CopyComponent(plantPotPrefab.GetComponent<ConstructableBounds>(), baseTeleporterPrefab);
			constructBounds.bounds.extents = new Vector3(0.4f, padHeight, 0.4f);
			constructBounds.bounds.position = new Vector3(0, padHeight, 0);

			var teleporterPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
			teleporterPad.name = "Pad";
			teleporterPad.transform.SetParent(baseTeleporterPrefab.transform);
			teleporterPad.transform.localScale = new Vector3(1.5f, padHeight, 1.5f);
			teleporterPad.transform.localPosition = new Vector3(0, padHeight / 2, 0);
			teleporterPad.GetComponent<MeshRenderer>().material = alienTeleporterPrefab.GetComponentInChildren<MeshRenderer>().material;

			constructable.model = teleporterPad;

			var teleporterField = GameObject.CreatePrimitive(PrimitiveType.Cube);
			teleporterField.name = "TeleportField";
			teleporterField.transform.SetParent(baseTeleporterPrefab.transform);
			teleporterField.transform.localScale = new Vector3(1, fieldHeight, 1);
			teleporterField.transform.localPosition = new Vector3(0, padHeight + fieldHeight / 2, 0);
			var fieldRenderer = teleporterField.GetComponent<MeshRenderer>();
			fieldRenderer.material = alienTeleporterPrefab.GetComponentInChildren<MeshRenderer>().material;
			fieldRenderer.material.color = new Color(0, 1, 0, 0.5f);

			teleporterField.AddComponent<PrecursorTeleporterCollider>();

			var teleporterCollider = teleporterField.GetComponent<BoxCollider>();
			teleporterCollider.isTrigger = true;

			baseTeleporterPrefab.SetActive(false);
			var sky = baseTeleporterPrefab.AddComponent<SkyApplier>();
			sky.dynamic = true;
			sky.renderers = baseTeleporterPrefab.GetAllComponentsInChildren<MeshRenderer>();
			sky.anchorSky = Skies.BaseInterior;
			baseTeleporterPrefab.SetActive(true);

			return baseTeleporterPrefab;
		}
	}
}
