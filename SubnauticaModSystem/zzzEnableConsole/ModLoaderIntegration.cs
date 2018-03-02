namespace zzzEnableConsole
{
	public static class QPatch
	{
		public static void Patch()
		{
			DevConsole.disableConsole = false;
		}
	}
}