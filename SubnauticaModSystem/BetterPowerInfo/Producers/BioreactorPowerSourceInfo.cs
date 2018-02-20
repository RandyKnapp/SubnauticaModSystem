using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BetterPowerInfo.Producers
{
	public class BioreactorPowerSourceInfo : PowerSourceInfo
	{
		protected static MethodInfo BaseBioreactor_get_container;
		protected static FieldInfo BaseBioreactor_charge;

		private BaseBioReactor reactor;

		public BioreactorPowerSourceInfo(PowerSource source) : base(source, TechType.BaseBioReactor)
		{
			reactor = source.GetComponent<BaseBioReactor>();
		}

		protected override string GetPowerSourceCustomText()
		{
			bool inactive = !reactor.producingPower && (Source.GetPower() < Source.GetMaxPower());
			float availableCharge = GetBioreactorAvailableCharge(reactor);
			return string.Format("Bioreactor <color={0}</color>",
				(inactive ? "red>[Inactive]" : "silver>[" + Mathf.RoundToInt(availableCharge) + "]")
			);
		}

		protected override float GetPowerProductionPerMinute()
		{
			return (reactor.producingPower ? 0.8333333f : 0) * 60;
		}

		protected float GetBioreactorAvailableCharge(BaseBioReactor reactor)
		{
			if (BaseBioreactor_charge == null)
			{
				BaseBioreactor_charge = typeof(BaseBioReactor).GetField("charge", BindingFlags.NonPublic | BindingFlags.Static);
			}

			Dictionary<TechType, float> chargeValues = (Dictionary<TechType, float>)BaseBioreactor_charge.GetValue(null);
			ItemsContainer items = GetBioreactorItems(reactor);

			float sum = 0;
			foreach (InventoryItem inventoryItem in items)
			{
				Pickupable item = inventoryItem.item;
				TechType techType = item.GetTechType();
				float itemChargeValue = 0;
				if (chargeValues.TryGetValue(techType, out itemChargeValue))
				{
					sum += itemChargeValue;
				}
			}

			return sum - reactor._toConsume;
		}

		protected ItemsContainer GetBioreactorItems(BaseBioReactor reactor)
		{
			if (BaseBioreactor_get_container == null)
			{
				BaseBioreactor_get_container = typeof(BaseBioReactor).GetMethod("get_container", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return (ItemsContainer)BaseBioreactor_get_container.Invoke(reactor, new object[] { });
		}
	}
}
