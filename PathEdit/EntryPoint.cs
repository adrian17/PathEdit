using System;

namespace PathEdit
{
	public static class EntryPoint
	{
		[STAThread]
		public static void Main(string[] args)
		{
			if (args != null && args.Length > 0 && args[0] == "saveSystemPath")
			{
				BatchMode.SavePathFromTempFile();
			}
			else
			{
				var app = new App();
				app.Run(new MainWindow());
			}
		}
	}
}