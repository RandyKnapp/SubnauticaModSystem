using UnityEngine;

namespace BetterPowerInfo.Producers
{
	class ThermalPlantPowerSourceInfo : PowerSourceInfoBase
	{
		private ThermalPlant plant;

		public ThermalPlantPowerSourceInfo(PowerSource source) : base(source)
		{
			plant = source.gameObject.GetComponent<ThermalPlant>();
		}

		protected override float GetPowerProductionPerMinute()
		{
			float num = 2f * DayNightCycle.main.dayNightSpeed;
			float num2 = 1.6500001f * num * Mathf.Clamp01(Mathf.InverseLerp(25f, 100f, plant.temperature));
			return num2 * 30;
		}

		protected override string GetPowerSourceDisplayText()
		{
			Color color = plant.temperatureText.color;
			string name =  base.GetPowerSourceDisplayText();
			return string.Format("{0} <color=#{2}>({1})</color>", name, plant.temperatureText.text, ColorUtility.ToHtmlStringRGBA(color));
		}
	}
}
