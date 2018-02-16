using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BetterPowerInfo.Consumers
{
	class ChargerPowerConsumerInfo : PowerConsumerInfoBase
	{
		private static FieldInfo Charger_batteries;

		private Charger charger;

		public ChargerPowerConsumerInfo(Charger charger) : base(charger.name)
		{
			this.charger = charger;
		}

		protected override string GetDisplayText()
		{
			string n = base.GetDisplayText() + " [ ";
			var batteries = GetChargerBatteries(charger);
			List<string> batteryStrings = new List<string>();
			
			foreach (KeyValuePair<string, IBattery> keyValuePair in batteries)
			{
				IBattery value = keyValuePair.Value;
				if (value != null)
				{
					float charge = value.charge;
					float capacity = value.capacity;
					float x = charge / capacity;
					Color color = (x >= 0.5f) ? Color.Lerp(charger.colorHalf, charger.colorFull, 2f * x - 1f) : Color.Lerp(charger.colorEmpty, charger.colorHalf, 2f * x);
					batteryStrings.Add(string.Format("<color=#{1}>{0}%</color> ", Mathf.RoundToInt(x * 100), ColorUtility.ToHtmlStringRGBA(color)));
				}
				else
				{
					batteryStrings.Add("   -   ");
				}
			}
			n += string.Join(" | ", batteryStrings.ToArray()) + " ]";
			return n;
		}

		protected override float GetPowerConsumedPerMinute()
		{
			float total = 0f;
			var batteries = GetChargerBatteries(charger);
			foreach (KeyValuePair<string, IBattery> keyValuePair in batteries)
			{
				IBattery value = keyValuePair.Value;
				if (value != null)
				{
					float charge = value.charge;
					float capacity = value.capacity;
					if (charge < capacity)
					{
						float chargeAmount = DayNightCycle.main.dayNightSpeed * charger.chargeSpeed * capacity;
						total += chargeAmount;
					}
				}
			}

			return total;
		}

		private Dictionary<string, IBattery> GetChargerBatteries(Charger charger)
		{
			if (Charger_batteries == null)
			{
				Charger_batteries = typeof(Charger).GetField("batteries", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return (Dictionary<string, IBattery>)Charger_batteries.GetValue(charger);
		}
	}
}
