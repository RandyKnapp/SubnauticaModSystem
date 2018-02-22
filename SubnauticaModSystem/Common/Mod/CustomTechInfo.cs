using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Mod
{
	public class CustomIngredient : IIngredient
	{
		public TechType techType;
		public int amount;

		TechType IIngredient.techType => techType;
		int IIngredient.amount => amount;
	}

	public class CustomTechInfo : ITechData
	{
		public TechType techType;
		public TechGroup techGroup;
		public TechCategory techCategory;
		public string assetPath;
		public bool knownAtStart;
		public Atlas.Sprite sprite;
		public List<CustomIngredient> recipe = new List<CustomIngredient>();
		public List<TechType> linkedItems = new List<TechType>();

		public int craftAmount { get; set; }
		public int ingredientCount => recipe.Count;
		public int linkedItemCount => linkedItems.Count;

		public IIngredient GetIngredient(int index)
		{
			return recipe[index];
		}

		public TechType GetLinkedItem(int index)
		{
			return linkedItems[index];
		}
	}
}
