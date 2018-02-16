using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterPowerInfo.Producers
{
	public class RegenPowerSourceInfo : PowerSourceInfoBase
	{
		private RegeneratePowerSource regen;

		public RegenPowerSourceInfo(PowerSource source) : base(source)
		{
			regen = source.gameObject.GetComponent<RegeneratePowerSource>();
		}

		protected override float GetPowerProductionPerMinute()
		{
			return regen.regenerationAmount / (regen.regenerationInterval / 60);
		}
	}
}
