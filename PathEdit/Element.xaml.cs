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
		private const string DisabledUserItemsFileName = "DisabledItems_User.dat";
		private const string DisabledSystemItemsFileName = "DisabledItems_System.dat";
		private readonly string _appDataDirPath;

		public PathType PathType { get; set; }

		private ObservableCollection<PathEntry> Items { get; set; }

		public Element()
		{
			InitializeComponent();
			PathBox.DataContext = this;

			_appDataDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PathEdit");
			if (!Directory.Exists(_appDataDirPath))
				Directory.CreateDirectory(_appDataDirPath);

			Items = new ObservableCollection<PathEntry>();
			PathBox.ItemsSource = Items;

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
				.Select(x => new PathEntry(x));
			var disabledItemList = ReadDisabledItems(PathType)
				.Select(x => new PathEntry(x) {Enabled = false});

			var items = itemList.Concat(disabledItemList)
				.Distinct(new PathEqualityComparer());

			Items.Clear();
			foreach (var item in items)
			{
				Items.Add(item);
			}
		}

		private void EnableButton_Click(object sender, RoutedEventArgs e)
		{
			var clickedElement = ((FrameworkElement) sender).DataContext as PathEntry;
			if (clickedElement == null)
				throw new Exception();

			clickedElement.Enabled = !clickedElement.Enabled;

			SaveDisabledItems(PathType, Items.Where(x => x.Enabled == false).Select(x => x.Path).ToList());
		}

		private void SaveDisabledItems(PathType type, IEnumerable<string> items)
		{
			var name = type == PathType.User ? DisabledUserItemsFileName : DisabledSystemItemsFileName;
			var fullPath = Path.Combine(_appDataDirPath, name);
			File.WriteAllLines(fullPath, items);
		}

		private IEnumerable<string> ReadDisabledItems(PathType type)
		{
			var name = type == PathType.User ? DisabledUserItemsFileName : DisabledSystemItemsFileName;
			var fullPath = Path.Combine(_appDataDirPath, name);
			if (!File.Exists(fullPath))
				return new List<string>();
			return File.ReadAllLines(fullPath);
		}

		private void OpenDirButton_Click(object sender, RoutedEventArgs e)
		{
			var clickedElement = ((FrameworkElement) sender).DataContext as PathEntry;
			if (clickedElement == null)
				throw new Exception();
			if (!clickedElement.Exists)
				return;
			Process.Start("explorer.exe", @"/root," + clickedElement.PathExpanded);
		}

		private void AddEmptyButton_Click(object sender, RoutedEventArgs e)
		{
			Items.Add(new PathEntry(""));
			SetItemFocus(Items.Count - 1);
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
			PathBox.Focus();
		}

		private void RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			var index = PathBox.SelectedIndex;
			if (index == -1)
				return;
			Items.RemoveAt(index);
			if (Items.Count != 0)
				SetItemFocus(Items.Count > index ? index : index - 1);
		}

		private void UpButton_Click(object sender, RoutedEventArgs e)
		{
			var index = PathBox.SelectedIndex;
			if (index == -1 || index == 0)
				return;
			SwapItems(index, index - 1);
			SetItemFocus(index - 1);
		}

		private void DownButton_Click(object sender, RoutedEventArgs e)
		{
			var index = PathBox.SelectedIndex;
			if (index == -1 || index == Items.Count - 1)
				return;
			SwapItems(index, index + 1);
			SetItemFocus(index + 1);
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
			var path = String.Join(";", Items.Where(x => x.Enabled).Select(x => x.Path)) + ";";
			PathReader.SavePathToRegistry(PathType, path);
			PathBox.Focus();
		}

		private void PathBox_DragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop, true))
			{
				e.Effects = DragDropEffects.None;
				e.Handled = true;
				return;
			}
			var paths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
			if (paths == null)
				return;
			if (paths.Any(path => !Directory.Exists(path)))
			{
				e.Effects = DragDropEffects.None;
				e.Handled = true;
			}
		}

		private void PathBox_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop, true))
				return;
			var paths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
			if (paths == null)
				return;
			if (paths.Any(path => !Directory.Exists(path)))
				return;
			foreach (var path in paths)
				Items.Add(new PathEntry(path));
			SetItemFocus(Items.Count - 1);
		}

		// workaround for selected item being indistinguishable from others when the DataGrid goes out of focus
		private void SetItemFocus(int index)
		{
			PathBox.SelectedIndex = index;
			PathBox.ScrollIntoView(Items[index]);
			PathBox.Focus();
		}
	}
}