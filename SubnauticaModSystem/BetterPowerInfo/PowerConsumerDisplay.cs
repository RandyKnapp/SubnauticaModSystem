using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	class PowerConsumerDisplay : PowerDisplayBase
	{
		protected override void UpdatePower()
		{
			PowerRelay power = GetCurrentPowerRelay();
			if (power != null)
			{
				text.text = "PowerConsumerDisplay";
			}
			else
			{
				text.text = "";
			}
		}
	}
}
