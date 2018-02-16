using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterPowerInfo.Consumers
{
	public class FiltrationMachinePowerConsumerInfo : PowerConsumerInfoBase
	{
		private FiltrationMachine filter;

		public FiltrationMachinePowerConsumerInfo(FiltrationMachine filter) : base(filter.name)
		{
			this.filter = filter;
		}

		protected override string GetDisplayText()
		{
			string n = base.GetDisplayText();

			string salt = filter.storageContainer.container.GetCount(TechType.Salt).ToString();
			string water = filter.storageContainer.container.GetCount(TechType.BigFilteredWater).ToString();
			if (filter.timeRemainingSalt >= 0)
			{
				float saltProgress = 1f - Mathf.Clamp01(filter.timeRemainingSalt / 420f);
				salt = string.Format("{0} ({1}%)", salt, Mathf.RoundToInt(saltProgress * 100));
			}
			if (filter.timeRemainingWater >= 0)
			{
				float waterProgress = 1f - Mathf.Clamp01(filter.timeRemainingWater / 840f);
				water = string.Format("{0} ({1}%)", water, Mathf.RoundToInt(waterProgress * 100));
			}
			n += string.Format(" [ <color=cyan>H₂0: {1}</color>  <color=lightblue>NaCl: {0}</color> ]", salt, water);
			return n;
		}

		protected override float GetPowerConsumedPerMinute()
		{
			PowerRelay relay = filter.gameObject.GetComponentInParent<PowerRelay>();
			bool powered = relay.GetPower() >= 0.85f;
			bool working = filter.timeRemainingWater > 0f || filter.timeRemainingSalt > 0f;
			float powerPerSecond = DayNightCycle.main.dayNightSpeed * 0.85f;
			return (working && powered) ? powerPerSecond * 60 : 0;
		}
	}
}
