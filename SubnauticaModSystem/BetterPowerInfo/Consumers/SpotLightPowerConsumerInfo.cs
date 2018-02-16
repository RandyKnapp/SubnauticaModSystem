using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BetterPowerInfo.Consumers
{
	class SpotLightPowerConsumerInfo : PowerConsumerInfoBase
	{
		private static FieldInfo BaseSpotLight_powerPerSecond;
		private static FieldInfo BaseSpotLight__powered;

		private BaseSpotLight light;

		public SpotLightPowerConsumerInfo(BaseSpotLight light) : base(light.name)
		{
			this.light = light;
		}

		protected override string GetDisplayText()
		{
			string n = base.GetDisplayText();
			return n;
		}

		protected override float GetPowerConsumedPerMinute()
		{
			if (BaseSpotLight_powerPerSecond == null)
			{
				BaseSpotLight_powerPerSecond = typeof(BaseSpotLight).GetField("powerPerSecond", BindingFlags.NonPublic | BindingFlags.Static);
			}

			if (BaseSpotLight__powered == null)
			{
				BaseSpotLight__powered = typeof(BaseSpotLight).GetField("_powered", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			float powerPerSecond = (float)BaseSpotLight_powerPerSecond.GetValue(null);
			bool powered = (bool)BaseSpotLight__powered.GetValue(light);
			return powered ? powerPerSecond * 60 : 0;
		}
	}
}
