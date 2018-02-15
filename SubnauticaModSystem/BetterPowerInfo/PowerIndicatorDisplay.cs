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
	struct PowerSourceEntry
	{
		public string Name;
		public int CurrentPower;
		public int MaxPower;
		public float ProductionPerMinute;
	}

	public class PowerIndicatorDisplay : MonoBehaviour
	{
		private const float TextUpdateInterval = 0.5f;

		private static MethodInfo SolarPanel_GetRechargeScalar;
		private static FieldInfo PowerSource_inboundPowerSources;

		private Text text;
		private List<PowerSourceEntry> powerSources = new List<PowerSourceEntry>();

		private void Awake()
		{
			text = GetComponent<Text>();
			text.supportRichText = true;
		}

		private IEnumerator Start()
		{
			for (;;)
			{
				yield return new WaitForSeconds(TextUpdateInterval);
				if (ShouldSkipUpdate())
				{
					continue;
				}

				UpdatePower();
			}
		}

		private void UpdatePower()
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
				text.text = "No Powered System";
			}
		}

		private void AccumulatePowerSources(PowerRelay power)
		{
			if (power.internalPowerSource != null)
			{
				AddPowerSourceEntry(power.internalPowerSource);
			}

			List<IPowerInterface> inboundPowerSources = GetInboundPowerSources(power);
			foreach (IPowerInterface iSource in inboundPowerSources)
			{
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

				string name = "?? " + (iSource as Component).name.Replace("(Clone)", "");
				if (Input.GetKey(KeyCode.X))
				{
					foreach (var c in (iSource as Component).gameObject.GetComponents<Component>())
					{
						name += "," + c.GetType();
					}
				}
				powerSources.Add(new PowerSourceEntry() {
					Name = name,
					CurrentPower = Mathf.RoundToInt(iSource.GetPower()),
					MaxPower = Mathf.RoundToInt(iSource.GetMaxPower()),
					ProductionPerMinute = 0
				});
			}
		}

		private void AddPowerSourceEntry(PowerSource source)
		{
			powerSources.Add(new PowerSourceEntry() {
				Name = source.name.Replace("(Clone)", ""),
				CurrentPower = Mathf.RoundToInt(source.GetPower()),
				MaxPower = Mathf.RoundToInt(source.GetMaxPower()),
				ProductionPerMinute = GetPowerProductionPerMinute(source)
			});
		}

		private void AddGenericPowerEntry(IPowerInterface source)
		{
			powerSources.Add(new PowerSourceEntry() {
				Name = (source as Component).name.Replace("(Clone)", ""),
				CurrentPower = Mathf.RoundToInt(source.GetPower()),
				MaxPower = Mathf.RoundToInt(source.GetMaxPower()),
				ProductionPerMinute = 0
			});
		}

		private float GetTotalProductionPerMinute()
		{
			float sum = 0;
			foreach (var entry in powerSources)
			{
				sum += entry.ProductionPerMinute;
			}
			return sum;
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
			if (source == null)
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
			if (source == null)
				source = (power as Component).gameObject.GetComponent<BatterySource>();
			return source;
		}

		private float GetPowerProductionPerMinute(PowerSource source)
		{
			float powerProduction = 0;
			if (GetPowerProductionPerMinute(source.gameObject.GetComponent<RegeneratePowerSource>(), out powerProduction))
			{
				return powerProduction;
			}
			else if (GetPowerProductionPerMinute(source.gameObject.GetComponent<SolarPanel>(), out powerProduction))
			{
				return powerProduction;
			}
			else if(GetPowerProductionPerMinute(source.gameObject.GetComponent<ThermalPlant>(), out powerProduction))
			{
				return powerProduction;
			}
			else if (GetPowerProductionPerMinute(source.gameObject.GetComponent<BaseBioReactor>(), out powerProduction))
			{
				return powerProduction;
			}
			else if (GetPowerProductionPerMinute(source.gameObject.GetComponent<BaseNuclearReactor>(), out powerProduction))
			{
				return powerProduction;
			}

			return 0;
		}

		private bool GetPowerProductionPerMinute(RegeneratePowerSource source, out float result)
		{
			result = source != null ? source.regenerationAmount / (source.regenerationInterval / 60) : 0;
			return source != null;
		}

		private bool GetPowerProductionPerMinute(SolarPanel source, out float result)
		{
			result = 0;
			if (source != null)
			{
				if (SolarPanel_GetRechargeScalar == null)
				{
					SolarPanel_GetRechargeScalar = typeof(SolarPanel).GetMethod("GetRechargeScalar", BindingFlags.NonPublic | BindingFlags.Instance);
				}

				float chargeScalar = (float)SolarPanel_GetRechargeScalar.Invoke(source, new object[] { });
				result = chargeScalar * 0.25f * 5.0f * 60;
			}
			return source != null;
		}

		private bool GetPowerProductionPerMinute(ThermalPlant source, out float result)
		{
			result = 0;
			if (source != null)
			{
				float num = 2f * DayNightCycle.main.dayNightSpeed;
				float num2 = 1.6500001f * num * Mathf.Clamp01(Mathf.InverseLerp(25f, 100f, source.temperature));
				result = num2 * 30;
			}
			return source != null;
		}

		private bool GetPowerProductionPerMinute(BaseBioReactor source, out float result)
		{
			result = 0;
			if (source != null)
			{
				result = (source.producingPower ? 0.8333333f : 0) * 60;
			}
			return source != null;
		}

		private bool GetPowerProductionPerMinute(BaseNuclearReactor source, out float result)
		{
			result = 0;
			if (source != null)
			{
				result = (source.producingPower ? 4.16666651f : 0) * 60;
			}
			return source != null;
		}

		private string UpdatePowerText(PowerRelay power)
		{
			string t = GetCurrentAndMaxPowerText(power);
			t += GetPowerStatusText(power);
			foreach (var entry in powerSources)
			{
				t += GetTextForPowerSource(entry);
			}
			return t;
		}

		private string GetCurrentAndMaxPowerText(PowerRelay power)
		{
			return string.Format("{0}\nPower: {1} / {2}", power.name.Replace("(Clone)", ""), Mathf.RoundToInt(power.GetPower()), Mathf.RoundToInt(power.GetMaxPower()));
		}

		private string GetTextForPowerSource(PowerSourceEntry source)
		{
			string productionString = string.Format(" <color=\"lime\">+{0:0.0}</color>", source.ProductionPerMinute);
			return string.Format("\n > {0}: {1}/{2}{3}", source.Name, source.CurrentPower, source.MaxPower, source.ProductionPerMinute == 0 ? "" : productionString);
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

		private bool ShouldSkipUpdate()
		{
			return MainMenuOrStillLoading() || IsWrongGameState() || IsInCameraMode() || Player.main == null;
		}

		private bool MainMenuOrStillLoading()
		{
			return !uGUI_SceneLoading.IsLoadingScreenFinished || uGUI.main == null || uGUI.main.loading.IsLoading;
		}

		private bool IsWrongGameState()
		{
			return uGUI.isIntro || LaunchRocket.isLaunching || !GameModeUtils.RequiresPower();
		}

		private bool IsInCameraMode()
		{
			uGUI_CameraDrone cameraDrone = uGUI_CameraDrone.main;
			return cameraDrone != null && cameraDrone.GetCamera() != null;
		}

		private PowerRelay GetCurrentPowerRelay()
		{
			Player player = Player.main;
			if (player.escapePod.value && player.currentEscapePod != null)
			{
				return player.currentEscapePod.GetComponent<PowerRelay>();
			}
			else if (player.currentSub != null && player.currentSub.isBase)
			{
				return player.currentSub.GetComponent<PowerRelay>();
			}
			else if (player.currentSub != null && player.currentSub.powerRelay != null)
			{
				return player.currentSub.powerRelay;
			}
			return null;
		}
	}
}
