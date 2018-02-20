using System;
using UnityEngine;

namespace LongLockerNames
{
	[Serializable]
	class Config
	{
		public int SmallLockerTextLimit = 60;
		public int SignTextLimit = 100;
		public bool ColorPickerOnLockers = true;
		public bool ExtraColorsOnLockers = true;
		public bool ColorPickerOnSigns = true;
		public bool ExtraColorsOnSigns = true;
	}
}
