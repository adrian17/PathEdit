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
			var fullPath = Path.Combine(AppDataPath.AppDataDirPath, TempFileName);
			var file = new StreamWriter(fullPath);
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
				System.Windows.MessageBox.Show("Running as administrator is required to use system PATH!");
			}
		}

		public static void SavePathFromTempFile()
		{
			try
			{
				var fullPath = Path.Combine(AppDataPath.AppDataDirPath, TempFileName);
				var file = new StreamReader(fullPath);
				var path = file.ReadToEnd();
				PathReader.SavePathToRegistry(PathType.System, path);
				file.Close();
			}
			catch
			{
				System.Windows.MessageBox.Show("An unidentified error happened while trying to save system PATH.");
			}
			finally
			{
				File.Delete(TempFileName);
			}
		}
	}
}
