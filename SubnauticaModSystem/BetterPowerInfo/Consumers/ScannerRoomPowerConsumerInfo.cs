using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BetterPowerInfo.Consumers
{
	public class ScannerRoomPowerConsumerInfo : PowerConsumerInfoBase
	{
		private static FieldInfo MapRoomFunctionality_scanActive;
		private MapRoomFunctionality scanner;

		public ScannerRoomPowerConsumerInfo(MapRoomFunctionality scanner) : base(scanner.name)
		{
			this.scanner = scanner;
		}

		protected override string GetDisplayText()
		{
			bool isScanning = IsScannerActive();
			float range = scanner.GetScanRange();
			float speed = scanner.GetScanInterval();
			return string.Format("Scanner Room {0}(<color=lightblue>{1}m</color> | <color=lightblue>{2}s</color>)",
				isScanning ? "<color=lime>[Scanning...]</color> " : "",
				Mathf.RoundToInt(range),
				Mathf.RoundToInt(speed)
			);
		}

		protected override float GetPowerConsumedPerMinute()
		{
			bool isScanning = IsScannerActive();
			return isScanning ? 0.5f * 60 : 0;
		}

		private bool IsScannerActive()
		{
			if (MapRoomFunctionality_scanActive == null)
			{
				MapRoomFunctionality_scanActive = typeof(MapRoomFunctionality).GetField("scanActive", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return (bool)MapRoomFunctionality_scanActive.GetValue(scanner);
		}
	}
}
