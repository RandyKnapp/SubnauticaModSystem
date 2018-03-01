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
		}
		
		private void Initialize()
		{
			initialized = true;
		}

		public void Save(SaveData saveData)
		{
			var prefabIdentifier = GetComponent<PrefabIdentifier>();
			var id = prefabIdentifier.Id;

			var entry = new SaveDataEntry() { Id = id };
			saveData.Entries.Add(entry);
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
			GameObject originalPrefab = CraftData.GetPrefabForTechType(TechType.BaseReinforcement, false);
			GameObject prefab = GameObject.Instantiate(originalPrefab);

			prefab.name = "HabitatControlPanel";
			ModUtils.PrintObject(prefab);
			
			var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material.color = new Color(0.5f, 0.5f, 1);
			}
			
			return prefab;
		}
	}
}
