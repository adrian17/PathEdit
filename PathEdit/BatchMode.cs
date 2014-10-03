using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PathEdit
{
	static class BatchMode
	{
		private const string TempFileName = "tempPathFile.txt";

		public static void SavePathToTempFile(IEnumerable<PathEntry> items)
		{
			var path = PathReader.ItemsToPathString(items);
			var file = new StreamWriter(TempFileName);
			file.Write(path);
			file.Close();
		}

		public static void UseBatchModeAsAdmin(IEnumerable<PathEntry> items)
		{
			BatchMode.SavePathToTempFile(items);
			var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
			var startInfo = new ProcessStartInfo(exeName);
			startInfo.Verb = "runas";
			startInfo.Arguments = "saveSystemPath";
			try
			{
				Process.Start(startInfo);
			}
			catch
			{
				File.Delete(TempFileName);
			}
		}

		public static void SavePathFromTempFile()
		{
			try
			{
				var file = new StreamReader(TempFileName);
				var path = file.ReadToEnd();
				PathReader.SavePathToRegistry(PathType.System, path);
				file.Close();
				File.Delete(TempFileName);
			}
			catch
			{
			}
		}
	}
}
