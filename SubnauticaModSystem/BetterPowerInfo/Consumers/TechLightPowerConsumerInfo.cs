using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterPowerInfo.Consumers
{
	public class TechLightPowerConsumerInfo : PowerConsumerInfoBase
	{
		private TechLight light;

		public TechLightPowerConsumerInfo(TechLight light) : base(light.name)
		{
			this.light = light;
		}

		protected override string GetDisplayText()
		{
			return base.GetDisplayText();
		}

		protected override float GetPowerConsumedPerMinute()
		{
			return base.GetPowerConsumedPerMinute();
		}
	}
}
