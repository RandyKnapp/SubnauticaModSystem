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
				sprite = new Atlas.Sprite(ImageUtils.LoadTexture(GetAssetPath("AutosortLocker.png"))),
				recipe = new List<CustomIngredient>
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