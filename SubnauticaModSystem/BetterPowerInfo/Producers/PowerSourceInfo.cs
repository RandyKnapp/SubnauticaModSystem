using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BetterPowerInfo.Producers
{
	public class PowerSourceInfo
	{
		public virtual string Name { get { return GetPowerSourceNameText(); } }
		public virtual string CustomText { get { return GetPowerSourceCustomText(); } }
		public virtual int CurrentPower { get { return Mathf.RoundToInt(Source.GetPower()); } }
		public virtual int MaxPower { get { return Mathf.RoundToInt(Source.GetMaxPower()); } }
		public virtual float ProductionPerMinute { get { return GetPowerProductionPerMinute(); } }

		public PowerSource Source { get; private set; }
		public TechType TechType { get; private set; }

		public PowerSourceInfo(PowerSource source, TechType techType)
		{
			this.TechType = techType;
			this.Source = source;
		}

		protected virtual string GetPowerSourceNameText()
		{
			return GetDefaultPowerSourceNameText();
		}

		protected virtual string GetPowerSourceCustomText()
		{
			return "";
		}

		private string GetDefaultPowerSourceNameText()
		{
			return Mod.FormatName(Source.name).Replace("Base", "").Replace("Module", "");
		}

		protected virtual float GetPowerProductionPerMinute()
		{
			return 0;
		}
	}
}
