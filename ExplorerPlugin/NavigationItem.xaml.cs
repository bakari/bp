using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ApplicationCore;
using EditorPlugin;

namespace ExplorerPlugin
{
	/// <summary>
	/// Content of root node in treeview
	/// </summary>
	public partial class NavigationItem : UserControl
	{
		public NavigationItem()
		{
			InitializeComponent();
		}
		
		void AddItemToNavigation(string header, string path)
		{
			var t = new TextBlock();
			t.Text = header;
			t.Tag = path;
			t.Style = (Style) FindResource("navigationStyle");
			t.MouseDown += new MouseButtonEventHandler(Navigation_MouseDown);
			t.MouseEnter += new MouseEventHandler(Navigation_MouseEnter);
			t.MouseLeave += new MouseEventHandler(Navigation_MouseLeave);
			navigation.Children.Add(t);
			
			var backslash = new TextBlock();
			if (!header.Contains("\\")) backslash.Text = "\\";
			backslash.Style = (Style) FindResource("navigationStyle");
			navigation.Children.Add(backslash);
		}
		
		#region Event Handlers
		void dataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var item = DataContext as ItemViewModel;
			if (item == null) return;
			var imgConv = new ImageSourceConverter();
			var imgPath = "pack://application:,,,/Images/diskdrive.png";
			img.Source = (ImageSource) imgConv.ConvertFromString(imgPath);
			var directories = item.Path.Split('\\');
			foreach (var directory in directories)
			{
				if (directory.Length > 0) 
				{
					var path = item.Path.Substring(0, item.Path.IndexOf(directory) + directory.Length);
					var header = directory;
					if (item.Path.IndexOf(directory)==0)
					{
						path = path +"\\";
						header = header +"\\";
					}
					AddItemToNavigation(header, path);	
				}
			}
		}
		
		void Navigation_MouseEnter(object sender, MouseEventArgs e)
		{
			var position = navigation.Children.IndexOf(sender as TextBlock)+2;
			
			for (int i = 0; i < position; i++)
			{
				var child = navigation.Children[i] as TextBlock;
				if (child != null) child.TextDecorations.Add(TextDecorations.Underline);
			}
		}
		
		void Navigation_MouseLeave(object sender, MouseEventArgs e)
		{
			foreach (object child in navigation.Children)
			{
				var item = child as TextBlock;
				if (item != null) item.TextDecorations.Clear();
			}
		}
		
		void Navigation_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var position = navigation.Children.IndexOf(sender as TextBlock); 
			var t = (navigation.Children[position] as TextBlock);
			
			var treeView = App.Current.MainWindow.FindName("treeView") as DirectoryTree;
			if (treeView != null)
				treeView.Model.SetNewRoot(t.Text, t.Tag.ToString());
			var count = navigation.Children.Count;
			for (int i = position+2; i < count; i++) {
				navigation.Children.RemoveAt(position+2);
			}
		}
		#endregion
	}
}