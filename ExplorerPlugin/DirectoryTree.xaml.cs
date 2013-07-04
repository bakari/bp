using System;
using System.Collections.Generic;
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
	/// Interaction logic for DirectoryTree.xaml
	/// </summary>
	public partial class DirectoryTree : UserControl
	{
		DirectoryTreeViewModel model;
		public DirectoryTreeViewModel Model 
		{ 
			get 
			{
				return model;
			}
		}
		
		Editor editorPlugin;
		
		public DirectoryTree()
		{
			InitializeComponent();
		}
		
		public DirectoryTree(Editor editorPlugin)
		{
			if (editorPlugin != null)
				this.editorPlugin = editorPlugin;
			InitializeComponent();
		}
		
		/// <summary>
		/// (Re)loads model
		/// </summary>
		public void LoadTree()
		{
			model = new DirectoryTreeViewModel(this);
			base.DataContext = model;
		}
		
		public void SetSearchAreaVisible(bool b)
		{
			SearchArea.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
		}
		
		#region Event Handlers
		
		#region SearchTextBox
		void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && model.SearchCommand.CanExecute(this))
			{
				var item = ChangeViewComboBox.SelectedItem as ComboBoxItem;
				model.SearchCommand.Execute(item.Tag.ToString());
			}
		}
				
		void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var item = ChangeViewComboBox.SelectedItem as ComboBoxItem;
			if (model != null && model.SearchCommand.CanExecute(this)) 
			{
				model.SearchCommand.Execute(item.Tag.ToString());
			}	
			if (SearchTextBox.Text.Length > 0) 
			{
				SearchImage.Visibility = Visibility.Collapsed;
				StopImage.Visibility = Visibility.Visible;
			}
			else
			{
				SearchImage.Visibility = Visibility.Visible;
				StopImage.Visibility = Visibility.Collapsed;
			}
		}
		
		void SearchImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SearchTextBox_TextChanged(null, null);
		}
		
		void StopImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SearchTextBox.Clear();
		}
		#endregion
		
		#region TreeView	
		void Tree_KeyDown(object sender, KeyEventArgs e)
		{
			List<ItemViewModel> list = model.GetExpandedItems().FindAll
				(delegate(ItemViewModel item) { return item.Header.ToUpper().
						StartsWith(e.Key.ToString().ToUpper());});
			if (list.Count == 0) return;
			if (list.Contains(Tree.SelectedItem as ItemViewModel))
			{
				int i = list.IndexOf(Tree.SelectedItem as ItemViewModel);
				list[(i+1)%list.Count].IsSelected = true;
			} 
			else
			{
				list[0].IsSelected = true;
			}
		}
		#endregion
		
		#region TreeViewItem
		void TreeViewItem_MouseRightDown(object sender, MouseEventArgs e)
		{
			var item = sender as TreeViewItem;
			item.IsSelected = true;
			e.Handled = true;
		}
		
		void TreeViewItem_MouseDoubleClicked(object sender, MouseButtonEventArgs e)
		{
			if ((sender as TreeViewItem).IsSelected == false) return;
			
			var item = Tree.SelectedItem as ItemViewModel;
			if (item == null || item.Children.Count != 0 || editorPlugin == null) return;
			editorPlugin.CreateNewTab(item.Header, item.Path);
		}
		
		void TreeViewItem_MouseLeftDown(object sender, MouseButtonEventArgs e)
		{
			// only works for "Info item" -> click for more results
			var item = (sender as TreeViewItem).Header;
			if (item == null || !(item is ItemViewModel) || (item as ItemViewModel).Type != DirectoryTreeItem.TYPE.Info) return;
			
			// remove info item, increase threshold, resume filtering
			if (model.Worker is SearchWorker)
			{
				model.FirstGeneration.Remove((sender as TreeViewItem).Header as ItemViewModel);
				SearchWorker.Threshold *= 10;
				SearchWorker.WaitHandle.Set();
			}
		}
		
		void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var item = (sender as TreeViewItem);
				item.IsExpanded = !item.IsExpanded;
				TreeViewItem_MouseDoubleClicked(sender, null);
			}
			Tree.BringIntoView();
			e.Handled = true;
		}
		#endregion
		
		#region ContextMenu
		void MenuItem_SetAsRoot_Click(object sender, RoutedEventArgs e)
		{
			var item = Tree.SelectedItem as ItemViewModel;
			item.IsSelected = false;
			model.SetNewRoot(item);
		}
		
		void MenuItem_Open_Click(object sender, RoutedEventArgs e)
		{
			var item = Tree.SelectedItem as ItemViewModel;
			if (editorPlugin != null)
				editorPlugin.CreateNewTab(item.Header, item.Path);
		}
		#endregion
		
		#region ComboBox	
		void ChangeViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SearchTextBox.Text.Length > 0) SearchTextBox_TextChanged(null, null);
			if (Model != null) Model.ViewNumber = ChangeViewComboBox.SelectedIndex;
		}
		#endregion
		
		#endregion		
		
		#region Navigation
		public void AddItemToNavigation(ItemViewModel item)
		{
			if (item != null && Tree.HasItems)
			{
				var rootItem = (TreeViewItem) Tree.ItemContainerGenerator.ContainerFromItem(item);
				if (rootItem != null) 
				{
					rootItem.HeaderTemplate = (DataTemplate) FindResource("rootItem");	
				}
			}
		}
		
		void Tree_Loaded(object sender, RoutedEventArgs e)
		{
			if (Tree.HasItems)
			{
				foreach (ItemViewModel item in Tree.Items)
				{
					if (item.Type == DirectoryTreeItem.TYPE.Folder || item.Type == DirectoryTreeItem.TYPE.Drive)
					{
						AddItemToNavigation(item);	
					}
				}
			}
		}
		#endregion
	}
}