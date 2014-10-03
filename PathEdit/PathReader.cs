using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace PathEdit
{
	public enum PathType
	{
		User,
		System
	}

	internal static class PathReader
	{
		public static IEnumerable<PathEntry> ReadPath(PathType type)
		{
			return GetPathFromRegistry(type)
				.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => new PathEntry(x));
		}

		public static string ItemsToPathString(IEnumerable<PathEntry> items)
		{
			var path = String.Join(";", items.Where(x => x.Enabled).Select(x => x.Path)) + ";";
			return path;
		}

		private const string UserPathKey = @"Environment";

		private const string SystemPathKey =
			@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

		private static string GetPathFromRegistry(PathType type)
		{
			var mainKey = type == PathType.User ? Registry.CurrentUser : Registry.LocalMachine;

			var subKey = mainKey.OpenSubKey(type == PathType.User ? UserPathKey : SystemPathKey);
			if (subKey == null)
				throw new Exception();
			var path = subKey
				.GetValue("Path", null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
			if (path == null)
				throw new Exception();
			return path;
		}

		public static void SavePathToRegistry(PathType type, string path)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine(path);
#else
			Environment.SetEnvironmentVariable("Path", path,
				type == PathType.User ? EnvironmentVariableTarget.User : EnvironmentVariableTarget.Machine);
#endif
		}
	}
}