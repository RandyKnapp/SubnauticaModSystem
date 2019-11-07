using System;

namespace MoreQuickSlots
{
	public static class Logger
	{
		public static void Log(string message)
		{
			Console.WriteLine("[MoreQuickSlots] " + message);
		}

		public static void Log(string format, params object[] args)
		{
			Console.WriteLine("[MoreQuickSlots] " + string.Format(format, args));
		}
	}
}
