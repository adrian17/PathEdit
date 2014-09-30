using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PathEdit
{
	static class DisabledItems
	{
		private const string DisabledUserItemsFileName = "DisabledItems_User.dat";
		private const string DisabledSystemItemsFileName = "DisabledItems_System.dat";
		private static readonly string _appDataDirPath;

		static DisabledItems()
		{
			_appDataDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PathEdit");
			if (!Directory.Exists(_appDataDirPath))
				Directory.CreateDirectory(_appDataDirPath);
		}

		public static void SaveDisabledItems(PathType type, IEnumerable<PathEntry> items)
		{
			var name = type == PathType.User ? DisabledUserItemsFileName : DisabledSystemItemsFileName;
			var fullPath = Path.Combine(_appDataDirPath, name);

			var disabledItems = items.Where(x => x.Enabled == false).Select(x => x.Path);

			File.WriteAllLines(fullPath, disabledItems);
		}

		public static IEnumerable<PathEntry> ReadDisabledItems(PathType type)
		{
			var name = type == PathType.User ? DisabledUserItemsFileName : DisabledSystemItemsFileName;
			var fullPath = Path.Combine(_appDataDirPath, name);
			if (!File.Exists(fullPath))
				return new List<PathEntry>();
			return File.ReadAllLines(fullPath).Select(x => new PathEntry(x) { Enabled = false });
		}
	}
}
