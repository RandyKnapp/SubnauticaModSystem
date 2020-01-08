using System;
using UnityEngine;

namespace CustomizedStorage
{
	[Serializable]
	class Size : IComparable
	{
		public int width;
		public int height;

		public Size(int w, int h)
		{
			width = w;
			height = h;
		}

		public int CompareTo(object otherObj)
		{
			Size other = otherObj as Size;
			if (width < other.width || height < other.height)
			{
				return -1;
			}
			else if (width > other.width || height > other.height)
			{
				return 1;
			}
			return 0;
		}

		public override string ToString()
		{
			return "(" + width + ", " + height + ")";
		}
	}

	[Serializable]
	class ExosuitConfig
	{
		public int width = 6;
		public int baseHeight = 4;
		public int heightPerStorageModule = 1;
	}

	[Serializable]
	class FiltrationMachineConfig
	{
		public int width = 2;
		public int height = 2;
		public int maxSalt = 2;
		public int maxWater = 2;
	}

	[Serializable]
	class Config
	{
		public Size Inventory = new Size(6, 8);
        public Size SmallLocker = new Size(5, 6);
        public Size Locker = new Size(6, 8);
		public ExosuitConfig Exosuit = new ExosuitConfig();
		public Size SeamothStorage = new Size(4, 4);
		public Size BioReactor = new Size(4, 4);
		public FiltrationMachineConfig FiltrationMachine = new FiltrationMachineConfig();
	}
}
