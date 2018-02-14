using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterPowerInfo
{
	public static class Logger
	{
		public static void Log(string message)
		{
			Console.WriteLine("[BetterPowerInfo] " + message);
		}

		public static void Log(string format, params object[] args)
		{
			Console.WriteLine("[BetterPowerInfo] " + string.Format(format, args));
		}
	}
}
