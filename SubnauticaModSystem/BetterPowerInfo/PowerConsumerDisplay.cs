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
		List<PowerConsumerInfoBase> consumers = new List<PowerConsumerInfoBase>();

		protected override void UpdatePower()
		{
			PowerRelay power = GetCurrentPowerRelay();
			if (power == null || Mode == DisplayMode.Off)
			{
				text.text = "";
			}
			else
			{
				AccumulateConsumers();

				if (Mode == DisplayMode.Minimal)
				{
					text.text = GetTotalConsumedText();
				}
				else if (Mode == DisplayMode.Verbose)
				{
					string t = GetTotalConsumedText();
					t += "\n<b><color=lightblue>Power Consumers</color></b>";
					t += GetTextForConsumers();
					text.text = t;
				}
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
			AccumulateBaseObjects<Charger, ChargerPowerConsumerInfo>(root.gameObject);
			AccumulateBaseObjects<FiltrationMachine, FiltrationMachinePowerConsumerInfo>(root.gameObject);
			AccumulateBaseObjects<MapRoomFunctionality, ScannerRoomPowerConsumerInfo>(root.gameObject);
			AccumulateBaseObjects<BaseSpotLight, SpotLightPowerConsumerInfo>(root.gameObject);
		}

		private List<ObjectT> AccumulateBaseObjects<ObjectT, ConsumerInfoT>(GameObject root)
			where ObjectT : Component
			where ConsumerInfoT : PowerConsumerInfoBase
		{
			List<ObjectT> objs = root.GetAllComponentsInChildren<ObjectT>().Distinct().ToList();
			foreach (var obj in objs)
			{
				consumers.Add((ConsumerInfoT)Activator.CreateInstance(typeof(ConsumerInfoT), new object[] { obj }));
			}
			return objs;
		}

		private void AccumulateCyclopsConsumers(SubRoot root)
		{

		}
	}
}