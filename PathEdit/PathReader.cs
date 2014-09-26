using System;
using System.IO;
using System.Security.Principal;
using Microsoft.Win32;

namespace PathEdit
{
	public enum PathType
	{
		User,
		System
	}

	public class PathEntry : IEquatable<PathEntry>
	{
		private string _path;

		public PathEntry(string path)
		{
			Path = path;
			Exists = DirExists();
			Enabled = true;
		}

		public bool Exists { get; set; }
		public bool Enabled { get; set; }

		public string Path
		{
			get { return _path; }
			set
			{
				_path = value;
				Exists = DirExists();
			}
		}

		public bool DirExists()
		{
			return Directory.Exists(Path);
		}

		public bool Equals(PathEntry other)
		{
			if (other == null) return false;
			return this.Path.Equals(other.Path);
		}
		public override int GetHashCode()
		{
			return Path.GetHashCode();
		}
	}

	internal static class PathReader
	{
		private const string UserPathKey = @"HKEY_CURRENT_USER\Environment";

		private const string SystemPathKey =
			@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

		public static string GetPathFromRegistry(PathType type)
		{
			var key = type == PathType.User ? UserPathKey : SystemPathKey;
			var path = Registry.GetValue(key, "Path", null) as string;
			if (path == null)
				throw new Exception();
			return path;
		}

		public static void SavePathToRegistry(PathType type, string path)
		{
			var key = type == PathType.User ? UserPathKey : SystemPathKey;

			//todo: only for debugging, remove later
			System.Diagnostics.Debug.WriteLine(path);

			//todo: remove on release, only when 100% sure it's safe
			//Registry.SetValue(key, "Path", path);
		}
	}
}