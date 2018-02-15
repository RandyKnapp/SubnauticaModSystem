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
			text.text = "PowerConsumerDisplay";
		}
	}
}
