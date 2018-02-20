using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BetterPowerInfo.Producers
{
	public class NuclearReactorPowerSourceInfo : PowerSourceInfo
	{
		protected static MethodInfo BaseNuclearReactor_get_equipment;
		protected static FieldInfo BaseNuclearReactor_charge;

		private BaseNuclearReactor reactor;

		public NuclearReactorPowerSourceInfo(PowerSource source) : base(source, TechType.BaseNuclearReactor)
		{
			reactor = source.GetComponent<BaseNuclearReactor>();
		}

		protected override string GetPowerSourceCustomText()
		{
			bool inactive = !reactor.producingPower && (Source.GetPower() < Source.GetMaxPower());
			float availableCharge = GetNuclearReactorAvailableCharge(reactor);
			return string.Format("Nuclear Reactor <color={0}</color>",
				(inactive ? "red>[Inactive]" : "silver>[" + Mathf.RoundToInt(availableCharge) + "]")
			);
		}

		protected override float GetPowerProductionPerMinute()
		{
			return (reactor.producingPower ? 4.16666651f : 0) * 60;
		}

		protected float GetNuclearReactorAvailableCharge(BaseNuclearReactor reactor)
		{
			if (BaseNuclearReactor_charge == null)
			{
				BaseNuclearReactor_charge = typeof(BaseNuclearReactor).GetField("charge", BindingFlags.NonPublic | BindingFlags.Static);
			}

			Dictionary<TechType, float> chargeValues = (Dictionary<TechType, float>)BaseNuclearReactor_charge.GetValue(null);
			Equipment items = GetNuclearReactorItems(reactor);

			var e = items.GetEquipment();
			float sum = 0;
			while (e.MoveNext())
			{
				if (e.Current.Value == null)
					continue;

				Pickupable item = e.Current.Value.item;
				TechType techType = item.GetTechType();
				float itemChargeValue = 0;
				if (chargeValues.TryGetValue(techType, out itemChargeValue))
				{
					sum += itemChargeValue;
				}
			}

			return sum - reactor._toConsume;
		}

		protected Equipment GetNuclearReactorItems(BaseNuclearReactor reactor)
		{
			if (BaseNuclearReactor_get_equipment == null)
			{
				BaseNuclearReactor_get_equipment = typeof(BaseNuclearReactor).GetMethod("get_equipment", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return (Equipment)BaseNuclearReactor_get_equipment.Invoke(reactor, new object[] { });
		}
	}
}
