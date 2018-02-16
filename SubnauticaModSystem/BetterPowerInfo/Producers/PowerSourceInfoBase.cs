using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BetterPowerInfo.Producers
{
	public class PowerSourceInfoBase
	{
		public virtual string DisplayText { get { return GetPowerSourceDisplayText(); } }
		public virtual int CurrentPower { get { return Mathf.RoundToInt(source.GetPower()); } }
		public virtual int MaxPower { get { return Mathf.RoundToInt(source.GetMaxPower()); } }
		public virtual float ProductionPerMinute { get { return GetPowerProductionPerMinute(); } }

		protected PowerSource source;

		public PowerSourceInfoBase(PowerSource source)
		{
			this.source = source;
		}

		protected virtual string GetPowerSourceDisplayText()
		{
			return GetDefaultPowerSourceDisplayText();
		}

		private string GetDefaultPowerSourceDisplayText()
		{
			return Mod.FormatName(source.name).Replace("Base", "").Replace("Module", "");
		}

		protected virtual float GetPowerProductionPerMinute()
		{
			return 0;
		}
	}
}
