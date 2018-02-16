using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterPowerInfo.Producers
{
	public class GenericPowerSourceInfo : PowerSourceInfoBase
	{
		private IPowerInterface iSource;
		private string displayOverride;

		public GenericPowerSourceInfo(IPowerInterface source, string displayOverride = null) : base(null)
		{
			iSource = source;
			this.displayOverride = displayOverride;
		}

		public override int CurrentPower { get { return Mathf.RoundToInt(iSource.GetPower()); } }
		public override int MaxPower { get { return Mathf.RoundToInt(iSource.GetMaxPower()); } }

		protected override string GetPowerSourceDisplayText()
		{
			if ((iSource as Component) != null)
			{
				string name = (iSource as Component).name.Replace("(Clone)", "").Replace("Base", "").Replace("Module", "");
				name = System.Text.RegularExpressions.Regex.Replace(name, "[A-Z]", " $0").Trim();
				if (Input.GetKey(KeyCode.X))
				{
					foreach (var c in (iSource as Component).gameObject.GetComponents<Component>())
					{
						name += "," + c.GetType();
					}
				}
				return name;
			}

			return iSource.ToString();
		}
	}
}
