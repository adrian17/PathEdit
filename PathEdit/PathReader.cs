using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows;
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

		private const string UserPathKey =
			@"Environment";

		private const string SystemPathKey =
			@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

		private static string GetPathFromRegistry(PathType type)
		{
			var mainKey = type == PathType.User ? Registry.CurrentUser : Registry.LocalMachine;

			try
			{
				var subKey = mainKey.OpenSubKey(type == PathType.User ? UserPathKey : SystemPathKey);
				if (subKey == null)
					throw new Exception();
				var path = subKey
					.GetValue("Path", "", RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
				return path;
			}
			catch (SecurityException)
			{
				MessageBox.Show("An error has occured while trying to open registry.\n" +
								"Your account probably doesn't have necessary permissions.");
				Application.Current.Shutdown();
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("An error has occured while trying to open registry.\n" +
								"Your account probably doesn't have necessary permissions.");
				Application.Current.Shutdown();
			}
			catch (Exception e)
			{
				MessageBox.Show("An unknown error has occured while trying to open registry:\n" + e.Message);
				Application.Current.Shutdown();
			}
			return "";
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