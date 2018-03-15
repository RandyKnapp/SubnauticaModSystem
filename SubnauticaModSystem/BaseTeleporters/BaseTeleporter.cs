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
				techGroup = TechGroup.InteriorPieces,
				techCategory = TechCategory.InteriorPiece,
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
			var teleporterPrefab = Resources.Load<GameObject>("WorldEntities/Environment/Precursor/MountainIsland/Precursor_Mountain_Teleporter_ToFloatingIsland");
			var nuclearReactorPrefab = Resources.Load<GameObject>("Submarine/Build/BaseNuclearReactorModule");
			var nuclearReactorGhost = Resources.Load<GameObject>("Base/Ghosts/BaseNuclearReactor");

			ModUtils.PrintObject(nuclearReactorPrefab);
			ModUtils.PrintObject(nuclearReactorGhost);

			var constructable = nuclearReactorGhost.GetComponent<ConstructableBase>();
			Logger.Log("ConstructableBase");
			ModUtils.PrintObjectFields(constructable);
			var addModuleGhost = nuclearReactorGhost.GetComponentInChildren<BaseAddModuleGhost>();
			Logger.Log("BaseAddModuleGhost");
			ModUtils.PrintObjectFields(addModuleGhost);
			addModuleGhost.modulePrefab = teleporterPrefab;

			foreach (var meshRenderer in addModuleGhost.modulePrefab.GetComponentsInChildren<MeshRenderer>())
			{
				meshRenderer.material.color = new Color(0, 1, 0);
			}

			return nuclearReactorGhost;
		}
	}
}
