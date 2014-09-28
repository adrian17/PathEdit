using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;

namespace PathEdit
{
	public enum PathType
	{
		User,
		System
	}

	public class PathEntry : INotifyPropertyChanged
	{
		private string _path;
		private bool _exists;
		private bool _enabled;

		public PathEntry(string path)
		{
			Path = path;
			Exists = DirExists();
			Enabled = true;
		}

		public bool Exists
		{
			get { return _exists; }
			set
			{
				if (value == _exists)
					return;
				_exists = value;
				OnPropertyChanged("Exists");
			}
		}

		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (value == _enabled)
					return;
				_enabled = value;
				OnPropertyChanged("Enabled");
			}
		}

		public string Path
		{
			get { return _path; }
			set
			{
				if (value == _path)
					return;
				_path = value;
				Exists = DirExists();
				OnPropertyChanged("Path");
			}
		}

		public bool DirExists()
		{
			return Directory.Exists(Path);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	class PathEqualityComparer : IEqualityComparer<PathEntry>
	{
		public bool Equals(PathEntry x, PathEntry y)
		{
			return x.Path.Equals(y.Path);
		}
		public int GetHashCode(PathEntry x)
		{
			return x.Path.GetHashCode();
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