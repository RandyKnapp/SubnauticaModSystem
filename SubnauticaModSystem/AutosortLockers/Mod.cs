using Common.Mod;
using Common.Utility;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutosortLockers
{
	public enum CustomTechType
	{
		AutosortLocker = 11110,
		AutosortTarget
	}

	static class Mod
	{
		public static Config config;

		private static string modDirectory;

		public static void Patch(string modDirectory = null)
		{
			Mod.modDirectory = modDirectory ?? "Subnautica_Data\\Managed";
			LoadConfig();

			DevConsole.disableConsole = false;

			AddBuildables();

			HarmonyInstance harmony = HarmonyInstance.Create("com.AutosortLockers.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			Logger.Log("Patched");
		}

		public static void AddBuildables()
		{
			Logger.Log("Add Buildable: " + CustomTechType.AutosortLocker);
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				techType = GetTechType(CustomTechType.AutosortLocker),
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/AutosortLocker",
				displayString = "Autosorter",
				tooltip = "Small, wall-mounted smart-locker that automatically transfers items into linked Autosort Receptacles.",
				techTypeKey = CustomTechType.AutosortLocker.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(GetAssetPath("AutosortLocker.png"))),
				recipe = config.EasyBuild
				? new List<CustomIngredient> {
					new CustomIngredient() {
						techType = TechType.Titanium,
						amount = 2
					}
				}
				: new List <CustomIngredient>
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

			Logger.Log("Add Buildable: " + CustomTechType.AutosortTarget);
			BuilderUtils.AddBuildable(new CustomTechInfo() {
				techType = GetTechType(CustomTechType.AutosortTarget),
				techGroup = TechGroup.InteriorModules,
				techCategory = TechCategory.InteriorModule,
				knownAtStart = true,
				assetPath = "Submarine/Build/AutosortTarget",
				displayString = "Autosort Receptacle",
				tooltip = "Wall locker linked to an Autosorter that stores the items.",
				techTypeKey = CustomTechType.AutosortTarget.ToString(),
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(GetAssetPath("AutosortTarget.png"))),
				recipe = config.EasyBuild
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

		public static string GetModPath()
		{
			return Environment.CurrentDirectory + "\\" + modDirectory;
		}

		public static string GetAssetPath(string filename)
		{
			return GetModPath() + @"\Assets\" + filename;
		}

		private static string GetModInfoPath()
		{
			return GetModPath() + "\\mod.json";
		}

		private static void LoadConfig()
		{
			config = ModUtils.LoadConfig<Config>(GetModInfoPath());
			ValidateConfig();
		}

		private static void ValidateConfig()
		{
			Config defaultConfig = new Config();
			if (config == null)
			{
				config = defaultConfig;
				return;
			}
		}

		private static TechType GetTechType(CustomTechType customTechType)
		{
			return (TechType)customTechType;
		}
	}
}