using System.Collections.Generic;
using System.Linq;
using System.IO;
#if SN
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Linq;
#elif BZ
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace AutosortLockers
{
	class GetLists
	{
		// Class to get valid lists of Category and TechTypes

		//Vince
		//Logger.Log("Vince: " + text.text);
		//QModManager.Utility.Logger.Log(QModManager.Utility.Logger.Level.Debug, $"Vince: " + text.text);

		private static string FilterFromJson;

		public static string GetFiltersFromJson(char listType, char gameVersion)
		{
			// Load categories.json
			JObject catObj = JObject.Load(new JsonTextReader(File.OpenText(Mod.GetModPath() + "/categories.json")));
			// Load techtypes.json
			JObject ttObj = JObject.Load(new JsonTextReader(File.OpenText(Mod.GetModPath() + "/techtypes.json")));

			foreach (var categoriesJson in catObj)
			{
				// Filter variables
				var gameVersions = new HashSet<char> { 'A', gameVersion };
				var categoryIDs = new HashSet<string> { };
				var useInMod = new HashSet<bool> { true };

				// Right outer join on catObj.  Select all Items[*] array items
				var query = from c in catObj.SelectTokens("Categories[*]").OfType<JObject>()
											// Join catObj with ttObj on CategoryID
										join t in ttObj.SelectTokens("TechTypes[*]") on (string)c["CategoryID"] equals (string)t["CategoryID"]
										// Process the filters
										where categoryIDs.Count() > 0 ?
										useInMod.Contains((bool)c["UseInMod"])
										&& gameVersions.Contains((char)c["GameVersion"])
										&& gameVersions.Contains((char)t["GameVersion"])
										&& categoryIDs.Contains((string)c["CategoryID"]) :
										useInMod.Contains((bool)c["UseInMod"])
										&& gameVersions.Contains((char)c["GameVersion"])
										&& gameVersions.Contains((char)t["GameVersion"])
										select new
										{
											CategoryDescription = c["CategoryDescription"],
											CategoryID = c["CategoryID"],
											TechName = t["TechName"],
											TechType = t["TechType"],
											TechID = t["TechID"],
											GameVersion = t["GameVersion"]
										};
				// Convert the query into a formatted list
				if (listType == 'T')
				{
					FilterFromJson = JsonConvert.SerializeObject(query.ToArray(), Formatting.Indented);
				}
				else
				{
					var results = query.GroupBy(
					i => i.CategoryDescription,
					t => t.TechID,
					(key, g) => new { Category = key, Types = g.ToList() });
					FilterFromJson = JsonConvert.SerializeObject(results, Formatting.Indented);
				}
			}
			return FilterFromJson;
		}
	}
}