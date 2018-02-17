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
			Console.WriteLine("[BlueprintTracker] " + string.Format(format, args));
		}
	}
}
