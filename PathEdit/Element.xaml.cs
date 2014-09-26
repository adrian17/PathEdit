using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PathEdit
{
	public partial class Element : UserControl
	{
		private const string DisableUserItemsFilePath = "PathEdit_disableditems_User.dat";
		private const string DisableSystemItemsFilePath = "PathEdit_disableditems_System.dat";

		public PathType PathType { get; set; }

		//todo: make it react to rebindings and item changes
		private ObservableCollection<PathEntry> Items { get; set; }

		public Element()
		{
			InitializeComponent();
			PathBox.DataContext = this;

			this.Loaded += Element_Loaded;
		}

		private void Element_Loaded(object sender, RoutedEventArgs e)
		{
			Reload();
		}

		private void Reload()
		{
			var itemList = PathReader.GetPathFromRegistry(PathType)
				.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => new PathEntry(x))
				.ToList();
			var disabledItemList = ReadDisabledItems(PathType).Select(x => new PathEntry(x) { Enabled = false });


			//todo: if there are two entries with the same path, one enabled and one disabled, keep the enabled one
			var items = itemList.Concat(disabledItemList).Distinct().ToList();

			Items = new ObservableCollection<PathEntry>(items);

			//todo: remove this, there is already binding in XAML but doesn't react to new
			PathBox.ItemsSource = Items;
		}

		private void EnableButton_Click(object sender, RoutedEventArgs e)
		{
			var clickedElement = ((FrameworkElement) sender).DataContext as PathEntry;
			if (clickedElement == null)
				throw new Exception();

			clickedElement.Enabled = !clickedElement.Enabled;

			SaveDisabledItems(PathType, Items.Where(x => x.Enabled == false).Select(x => x.Path).ToList());
		}

		private static void SaveDisabledItems(PathType type, IEnumerable<string> items)
		{
			string path = type == PathType.User ?  DisableUserItemsFilePath : DisableSystemItemsFilePath;
			File.WriteAllLines(path, items);
		}

		private static IEnumerable<string> ReadDisabledItems(PathType type)
		{
			string path = type == PathType.User ? DisableUserItemsFilePath : DisableSystemItemsFilePath;
			if (!File.Exists(path))
				return new List<string>();
			return File.ReadAllLines(path).ToList();
		}

		private void OpenDirButton_Click(object sender, RoutedEventArgs e)
		{
			var clickedElement = ((FrameworkElement) sender).DataContext as PathEntry;
			if (clickedElement == null)
				throw new Exception();
			Process.Start("explorer.exe", @"/root," + clickedElement.Path);
		}

		private void AddEmptyButton_Click(object sender, RoutedEventArgs e)
		{
			Items.Add(new PathEntry(""));
			PathBox.SelectedIndex = Items.Count - 1;
			//todo: give focus to the textbox
		}

		private void SetDirButton_Click(object sender, RoutedEventArgs e)
		{
			var index = PathBox.SelectedIndex;
			if (index == -1)
				return;

			var fbd = new System.Windows.Forms.FolderBrowserDialog();
			var result = fbd.ShowDialog();
			if (result != System.Windows.Forms.DialogResult.OK)
				return;
			string dir = fbd.SelectedPath;
			if (!Directory.Exists(dir))
				return;

			Items[index].Path = dir;
		}

		private void RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			if (PathBox.SelectedIndex == -1)
				return;
			Items.RemoveAt(PathBox.SelectedIndex);
		}

		private void UpButton_Click(object sender, RoutedEventArgs e)
		{
			var index = PathBox.SelectedIndex;
			if (index == -1 || index == 0)
				return;
			SwapItems(index, index - 1);
			PathBox.SelectedIndex = index - 1;
		}

		private void DownButton_Click(object sender, RoutedEventArgs e)
		{
			var index = PathBox.SelectedIndex;
			if (index == -1 || index == Items.Count - 1)
				return;
			SwapItems(index, index + 1);
			PathBox.SelectedIndex = index + 1;
		}

		private void SwapItems(int index1, int index2)
		{
			var tmp = Items[index1];
			Items[index1] = Items[index2];
			Items[index2] = tmp;
		}

		private void Update_Click(object sender, RoutedEventArgs e)
		{
			Reload();
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			var path = String.Join(";", Items.Where(x => x.Enabled == true).Select(x => x.Path)) + ";";
			PathReader.SavePathToRegistry(PathType, path);
		}
	}
}