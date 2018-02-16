using BetterPowerInfo.Producers;
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
	public class PowerProductionDisplay : PowerDisplayBase
	{
		private static FieldInfo PowerSource_inboundPowerSources;

		private List<PowerSourceInfoBase> powerSources = new List<PowerSourceInfoBase>();

		protected override void UpdatePower()
		{
			PowerRelay power = GetCurrentPowerRelay();
			if (power != null)
			{
				powerSources.Clear();
				AccumulatePowerSources(power);

				text.text = UpdatePowerText(power);
			}
			else
			{
				text.text = "";
			}
		}

		private void AccumulatePowerSources(PowerRelay power)
		{
			List<IPowerInterface> inboundPowerSources = GetInboundPowerSources(power);
			foreach (IPowerInterface iSource in inboundPowerSources)
			{
				if (iSource == null)
				{
					continue;
				}

				PowerSource source = TryGetPowerSource(iSource);
				PowerRelay relay = TryGetPowerRelay(iSource);
				BatterySource battery = TryGetBatterySource(iSource);
				if (source != null)
				{
					AddPowerSourceEntry(source);
					continue;
				}
				else if (relay != null)
				{
					AccumulatePowerSources(relay);
					continue;
				}
				else if (battery != null)
				{
					AddGenericPowerEntry(battery);
					continue;
				}
			}
		}

		private void AddPowerSourceEntry(PowerSource source)
		{
			if (source.gameObject.GetComponent<RegeneratePowerSource>())
			{
				powerSources.Add(new RegenPowerSourceInfo(source));
			}
			else if (source.gameObject.GetComponent<SolarPanel>())
			{
				powerSources.Add(new SolarPanelPowerSourceInfo(source));
			}
			else if (source.gameObject.GetComponent<ThermalPlant>())
			{
				powerSources.Add(new ThermalPlantPowerSourceInfo(source));
			}
			else if (source.gameObject.GetComponent<BaseBioReactor>())
			{
				powerSources.Add(new BioreactorPowerSourceInfo(source));
			}
			else if (source.gameObject.GetComponent<BaseNuclearReactor>())
			{
				powerSources.Add(new NuclearReactorPowerSourceInfo(source));
			}
			else
			{
				powerSources.Add(new PowerSourceInfoBase(source));
			}
		}

		private void AddGenericPowerEntry(IPowerInterface source)
		{
			powerSources.Add(new GenericPowerSourceInfo(source));
		}

		public float GetTotalProductionPerMinute()
		{
			float sum = 0;
			foreach (var entry in powerSources)
			{
				if (entry.CurrentPower < entry.MaxPower)
				{
					sum += entry.ProductionPerMinute;
				}
			}
			return sum;
		}

		public List<PowerSourceInfoBase> GetPowerSources()
		{
			return powerSources;
		}

		private List<IPowerInterface> GetInboundPowerSources(PowerRelay power)
		{
			if (PowerSource_inboundPowerSources == null)
			{
				PowerSource_inboundPowerSources = typeof(PowerRelay).GetField("inboundPowerSources", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			return (List<IPowerInterface>)PowerSource_inboundPowerSources.GetValue(power);
		}

		private PowerSource TryGetPowerSource(IPowerInterface power)
		{
			PowerSource source = power as PowerSource;
			if (source == null && (power as Component) != null)
				source = (power as Component).gameObject.GetComponent<PowerSource>();
			return source;
		}

		private PowerRelay TryGetPowerRelay(IPowerInterface power)
		{
			PowerRelay relay = power as PowerRelay;
			return relay;
		}

		private BatterySource TryGetBatterySource(IPowerInterface power)
		{
			BatterySource source = power as BatterySource;
			if (source == null && (power as Component) != null)
				source = (power as Component).gameObject.GetComponent<BatterySource>();
			return source;
		}

		private string UpdatePowerText(PowerRelay power)
		{
			if (Mode == DisplayMode.Minimal)
			{
				return GetCurrentAndMaxPowerTextMinimal(power);
			}
			else
			{
				string t = GetCurrentAndMaxPowerTextVerbose(power);
				powerSources.Sort((p1, p2) => p1.DisplayText.Substring(0, 5).CompareTo(p2.DisplayText.Substring(0, 5)));
				foreach (var entry in powerSources)
				{
					t += GetTextForPowerSource(entry);
				}
				return t;
			}
		}

		private string GetCurrentAndMaxPowerTextMinimal(PowerRelay power)
		{
			float totalProduction = Mathf.RoundToInt(GetTotalProductionPerMinute());
			return string.Format("<color={1}><b>+{0}</b></color>", totalProduction, totalProduction > 0 ? "lime" : "silver");
		}

		private string GetCurrentAndMaxPowerTextVerbose(PowerRelay power)
		{
			float totalProduction = Mathf.RoundToInt(GetTotalProductionPerMinute());
			string name = Mod.FormatName(power.name).Replace("Base", "Habitat").Replace("Module", "").Replace("-MainPrefab", "");
			string firstLine = string.Format("{0} <color={2}><b>+{1}</b></color>", name, totalProduction, totalProduction > 0 ? "lime" : "silver");
			return string.Format("{0}\n<b><color=lightblue>Power Sources</color></b>", 
				firstLine
			);
		}

		private string GetTextForPowerSource(PowerSourceInfoBase source)
		{
			string productionColor = (source.CurrentPower == source.MaxPower ? "silver" : (source.ProductionPerMinute == 0 ? "grey" : "lime"));
			string productionString = string.Format(" <color={1}>+{0:0.0}</color>", source.ProductionPerMinute, productionColor);
			return string.Format("\n{0} <color={4}>{1}/{2}</color>{3} >",
				source.DisplayText,
				source.CurrentPower,
				source.MaxPower,
				productionString,
				(source.CurrentPower == source.MaxPower ? "lime" : (source.CurrentPower == 0 ? "red" : "cyan"))
			);
		}

		private string GetPowerStatusText(PowerRelay power)
		{
			string statusColor = GetPowerStatusColor(power.GetPowerStatus());
			string statusText = GetPowerStatusText(power.GetPowerStatus());
			return string.Format(" <color={0}>{1}</color>", statusColor, statusText);
		}

		private string GetPowerStatusColor(PowerSystem.Status status)
		{
			switch (status)
			{
				default:
				case PowerSystem.Status.Normal:		return "cyan";
				case PowerSystem.Status.Offline:	return "red";
				case PowerSystem.Status.Emergency:	return "yellow";
			}
		}

		private string GetPowerStatusText(PowerSystem.Status status)
		{
			return status.ToString();
		}
	}
}
