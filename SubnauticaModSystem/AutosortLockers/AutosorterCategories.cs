#if SUBNAUTICA
using Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AutosortLockers
{
	[Serializable]
	public class AutosorterFilter
	{
		public string Category;
		public List<TechType> Types = new List<TechType>();
		public bool IsCategory() => !string.IsNullOrEmpty(Category);

		public string GetString()
		{
			if (IsCategory())
			{
				return Category;
			}
			else
			{
				var textInfo = (new CultureInfo("en-US", false)).TextInfo;
				return textInfo.ToTitleCase(Language.main.Get(Types[0]));
			}
		}

		public bool IsTechTypeAllowed(TechType techType)
		{
			return Types.Contains(techType);
		}

		public bool IsSame(AutosorterFilter other)
		{
			return Category == other.Category && Types.Count > 0 && Types.Count == other.Types.Count && Types[0] == other.Types[0];
		}
	}

	[Serializable]
	public static class AutosorterList
	{
		public static List<AutosorterFilter> Filters;

		public static List<AutosorterFilter> GetFilters()
		{
			if (Filters == null)
			{
				InitializeFilters();
			}
			return Filters;
		}

		[Serializable]
		private class TypeReference
		{
			public string TechName = "";
			public TechType TechID = TechType.None;
		}

		private static void InitializeFilters()
		{
			string getFilters;
			// Gets the list of Cateories that are UsedInMod = true
			getFilters = GetLists.GetFiltersFromJson('C', Mod.config.GameVersion);
			var categoryList = JsonConvert.DeserializeObject<List<AutosorterFilter>>(getFilters);
			Filters = categoryList.Where((f) => f.IsCategory()).ToList();

			// Gets the list of TechTypes for the Game Version SN or BZ
			getFilters = GetLists.GetFiltersFromJson('T', Mod.config.GameVersion);
			List<TypeReference> techTypeList = JsonConvert.DeserializeObject<List<TypeReference>>(getFilters);

			techTypeList.Sort((TypeReference a, TypeReference b) =>
			{
				string aName = Language.main.Get(a.TechID);
				string bName = Language.main.Get(b.TechID);

				//Returns the list of items from techtypes.json
				return aName.CompareTo(bName);
			});

			foreach (var typeRef in techTypeList)
			{
				Filters.Add(new AutosorterFilter() { Category = "", Types = new List<TechType> { typeRef.TechID } });
			}
			return;
		}
	}
}