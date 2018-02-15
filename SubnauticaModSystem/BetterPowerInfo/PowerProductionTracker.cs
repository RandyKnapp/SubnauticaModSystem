using System.Collections;
using UnityEngine;

namespace BetterPowerInfo
{
	class PowerProductionTracker : MonoBehaviour
	{
		private const float updateInterval = 1.0f;
		private const int windowSize = 20;

		private float[] productionWindow = new float[windowSize];
		private float previousPower = 0;
		private int currentSlot = 0;

		private IEnumerator Start()
		{
			for (;;)
			{
				yield return new WaitForSeconds(updateInterval);

				PowerSource source = gameObject.GetComponent<PowerSource>();
				if (source != null)
				{
					float powerProduced = source.GetPower() - previousPower;

					int slot = (currentSlot++) % windowSize;
					productionWindow[slot] = powerProduced;

					previousPower = source.GetPower();
				}
			}
		}

		public float GetPowerProductionPerMinute()
		{
			float sum = 0;
			foreach (float f in productionWindow)
			{
				sum += f;
			}
			return (sum / windowSize) * (60 / updateInterval);
		}
	}
}
