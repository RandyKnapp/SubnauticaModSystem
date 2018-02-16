using BetterPowerInfo.Consumers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BetterPowerInfo
{
	class PowerConsumerDisplay : PowerDisplayBase
	{
		private static FieldInfo Charger_batteries;

		List<PowerConsumerInfoBase> consumers = new List<PowerConsumerInfoBase>();

		protected override void UpdatePower()
		{
			PowerRelay power = GetCurrentPowerRelay();
			if (power != null)
			{
				AccumulateConsumers();

				// TODO: REMOVE
				if (false && Mode == DisplayMode.Minimal)
				{
					text.text = GetTotalConsumedText();
				}
				else
				{
					string t = GetTotalConsumedText();
					t += "\n<b><color=lightblue>Power Consumers</color></b>";
					t += GetTextForConsumers();
					text.text = t;
				}
			}
			else
			{
				text.text = "";
			}
		}

		private string GetTotalConsumedText()
		{
			float total = Mathf.RoundToInt(GetTotalPowerConsumedPerMinute());
			return string.Format("<color={1}>-{0}</color>", total, total == 0 ? "silver" : "red");
		}

		private float GetTotalPowerConsumedPerMinute()
		{
			float sum = 0;
			foreach (var c in consumers)
			{
				sum += c.PowerConsumedPerMinute;
			}
			return sum;
		}

		private string GetTextForConsumers()
		{
			string t = "";
			foreach (var c in consumers)
			{
				float consumed = c.PowerConsumedPerMinute;
				t += string.Format("\n> <color={2}>-{1}</color> {0}", c.DisplayText, Math.Round(consumed, 1), consumed == 0 ? "silver" : "red");
			}

			if (consumers.Count == 0)
			{
				t = "\n> <color=silver>None</color>";
			}
			return t;
		}

		private void AccumulateConsumers()
		{
			consumers.Clear();

			// NOTE: Escape Pod has no consumers

			Player player = Player.main;
			if (player.currentSub != null && player.currentSub.isBase)
			{
				AccumulateBaseConsumers(player.currentSub as BaseRoot);
			}
			else if (player.currentSub != null && player.currentSub.powerRelay != null)
			{
				AccumulateCyclopsConsumers(player.currentSub);
			}
		}

		private void AddGenericConsumer(string name, float consumption)
		{
			consumers.Add(new PowerConsumerInfoBase(name, consumption));
		}

		private void AccumulateBaseConsumers(BaseRoot root)
		{
			// Chargers
			AccumulateBaseChargers(root.gameObject.GetComponent<Base>());
		}

		private void AccumulateBaseChargers(Base b)
		{
			Charger[] chargers = b.gameObject.GetAllComponentsInChildren<Charger>();
			List<Charger> done = new List<Charger>();
			foreach (var charger in chargers)
			{
				if (done.Contains(charger))
				{
					continue;
				}
				done.Add(charger);

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

				AddGenericConsumer(charger.name, total * 12f);
			}
		}

		private Dictionary<string, IBattery> GetChargerBatteries(Charger charger)
		{
			if (Charger_batteries == null)
			{
				Charger_batteries = typeof(Charger).GetField("batteries", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return (Dictionary<string, IBattery>)Charger_batteries.GetValue(charger);
		}

		private void AccumulateCyclopsConsumers(SubRoot root)
		{

		}
	}
}
