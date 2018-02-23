using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Common.Mod
{
	public delegate GameObject GetPrefabDelegate();

	public class CustomIngredient : IIngredient
	{
		public TechType techType;
		public int amount;

		TechType IIngredient.techType => techType;
		int IIngredient.amount => amount;
	}

	public class CustomTechInfo : ITechData
	{
		public GetPrefabDelegate getPrefab;

		public TechType techType;
		public string classID;
		public TechGroup techGroup;
		public TechCategory techCategory;
		public string assetPath;
		public bool knownAtStart;
		public Atlas.Sprite sprite;
		public string displayString;
		public string techTypeKey;
		public string tooltip;
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
