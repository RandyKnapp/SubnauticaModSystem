using System;

namespace BlueprintTracker
{
	public static class Logger
	{
		public static void Log(string message)
		{
			Console.WriteLine("[BlueprintTracker] " + message);
		}

		public static void Log(string format, params object[] args)
		{
			Log(string.Format(format, args));
		}

		public static void Error(string message)
		{
			Console.WriteLine("[BlueprintTracker:ERROR] " + message);
		}

		public static void Error(string format, params object[] args)
		{
			Error(string.Format(format, args));
		}
	}
}
