using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace PathEdit
{
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

		public string PathExpanded
		{
			get { return Environment.ExpandEnvironmentVariables(Path); }
		}

		public bool DirExists()
		{
			return Directory.Exists(PathExpanded);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public class PathEqualityComparer : IEqualityComparer<PathEntry>
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
	}
}