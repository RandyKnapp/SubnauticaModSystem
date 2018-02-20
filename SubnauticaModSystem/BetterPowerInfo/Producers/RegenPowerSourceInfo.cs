using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterPowerInfo.Producers
{
	public class RegenPowerSourceInfo : PowerSourceInfo
	{
		private RegeneratePowerSource regen;

		public RegenPowerSourceInfo(PowerSource source) : base(source, TechType.PowerCell)
		{
			regen = source.gameObject.GetComponent<RegeneratePowerSource>();
		}

		protected override float GetPowerProductionPerMinute()
		{
			return regen.regenerationAmount / (regen.regenerationInterval / 60);
		}
	}
}
