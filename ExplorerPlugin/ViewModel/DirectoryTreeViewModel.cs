using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ExplorerPlugin
{
	/// <summary>
	/// Model for directory tree view
	/// </summary>
	public class DirectoryTreeViewModel
	{
		ItemViewModel rootItem;
		readonly ICommand searchCommand;
		ObservableCollection<ItemViewModel> firstGeneration;
		DirectoryTree treeView;
		
		#region Constructors
		public DirectoryTreeViewModel(DirectoryTree treeView)
		{
			firstGeneration = InitializeTreeContext();
			RootItem = firstGeneration[0];
			searchCommand = new SearchDirectoryTreeCommand(this);
			this.treeView = treeView;
		}
		
		public DirectoryTreeViewModel(DirectoryTree treeView, ItemViewModel root)
		{
			RootItem = root;
			RootItem.Expanded += new RoutedEventHandler(treeViewItemExpanded);
			RootItem.IsExpanded = true;
			firstGeneration = new ObservableCollection<ItemViewModel>
				(new ItemViewModel[]{rootItem});
			searchCommand = new SearchDirectoryTreeCommand(this);
			this.treeView = treeView;
		}
		#endregion
		
		#region Properties
		public ObservableCollection<ItemViewModel> FirstGeneration 
		{
			get { return firstGeneration; }
		}
		
		public ItemViewModel RootItem 
		{
			get { return rootItem; } 
			set { rootItem = value; }
		}
		
		public ICommand SearchCommand 
		{
			get { return searchCommand; }
		}
		
		public string SearchText { get; set; }
		
		public int ViewNumber { get; set; }
		
		public DirectoryTree TreeView { get { return treeView; } }
		
		public Thread SearchThread { get; set; }
		
		public ISearchWorker Worker { get; set; }
		#endregion
		
		#region SearchCommand
		private class SearchDirectoryTreeCommand : ICommand
		{
			readonly DirectoryTreeViewModel model;
			
			public SearchDirectoryTreeCommand(DirectoryTreeViewModel m)
			{
				model = m;
			}
			
			public bool CanExecute(object parameter)
			{
				return model != null;
			}
			
			event EventHandler ICommand.CanExecuteChanged
			{
				add {}
				remove {}
			}
			
			public void Execute(object parameter)
			{
				if (model.SearchThread != null)
				{
					// stop current search
					model.SearchThread.Abort();
					
				}
				model.Worker = new SearchWorker(model);
				// start new search
				try {
					model.SearchThread = new Thread(model.Worker.Work);
					model.SearchThread.IsBackground = true;
					model.SearchThread.Start();
				}
				catch (Exception) {}
			}
		}
		#endregion
		
		#region Model Context Setups
		ObservableCollection<ItemViewModel> InitializeTreeContext()
		{
			var collection = new ObservableCollection<ItemViewModel>();
			foreach(var driveInfo in System.IO.DriveInfo.GetDrives())
			{
				if (driveInfo.IsReady)
				{
					var item = new ItemViewModel(new DirectoryTreeItem());
					item.Header = driveInfo.Name;
					item.Path = driveInfo.Name;
					item.Type = DirectoryTreeItem.TYPE.Drive;
					item.Children.Add(new ItemViewModel(new DirectoryTreeItem(DirectoryTreeItem.TYPE.DummyNode),item));
					item.Expanded += new RoutedEventHandler(treeViewItemExpanded);
					collection.Add(item);
				}
			}
			collection[0].IsExpanded = true;
			return collection;
		}
		
		void Fill(ItemViewModel item)
		{
			try
			{
				var dir = new DirectoryInfo(item.Path);
				
				foreach (DirectoryInfo dirItem in dir.GetDirectories())
				{
					var newItem = new ItemViewModel(new DirectoryTreeItem(),item);
					newItem.Header = dirItem.Name;
					newItem.Path = dirItem.FullName;
					newItem.Type = DirectoryTreeItem.TYPE.Folder;
					newItem.Children.Add(new ItemViewModel(new DirectoryTreeItem(DirectoryTreeItem.TYPE.DummyNode),newItem));
					newItem.Expanded += new RoutedEventHandler(treeViewItemExpanded);
					item.Children.Add(newItem);
				}
				
				foreach (FileInfo fi in dir.GetFiles())
				{
					var subItem = new ItemViewModel(new DirectoryTreeItem(),item);
					subItem.Header = fi.Name;
					subItem.Path = fi.FullName;
					subItem.Type = DirectoryTreeItem.TYPE.File;
					item.Children.Add(subItem);
				}
			} catch (Exception) {}
		}
		
		public void treeViewItemExpanded(object sender, RoutedEventArgs e)
		{
			var item = sender as ItemViewModel;
			if (item.Children.Count == 1 && item.Children[0].Type == DirectoryTreeItem.TYPE.DummyNode)
			{
				item.Children.Clear();
				Fill(item);
			}
		}
		
		public void SetNewRoot(string header, string path)
		{
			var newRoot = new ItemViewModel(new DirectoryTreeItem());
			newRoot.Header = header;
			newRoot.Path = path;
			newRoot.Children.Add(new ItemViewModel(new DirectoryTreeItem(DirectoryTreeItem.TYPE.DummyNode)));
			newRoot.Expanded += new RoutedEventHandler(treeViewItemExpanded);
			SetNewRoot(newRoot);
		}
		
		/// <summary>
		/// Sets new root
		/// </summary>
		public void SetNewRoot(ItemViewModel item)
		{
			firstGeneration.Clear();
			RootItem = item;
			rootItem.Type = DirectoryTreeItem.TYPE.Drive;
			RootItem.IsExpanded = true;
			firstGeneration.Add(item);
			if (item.Header.Contains(":"))
			{
				// add other drives too
				foreach (var i in (from i in InitializeTreeContext().ToList() where !item.Equals(i) select i))
				{
					firstGeneration.Add(i);
				}
			}
			// apply "navigation" template to all drive items
			if (treeView != null) 
			{
				var tree = treeView.Tree;
				if (tree != null && tree.HasItems)
				{	
					foreach (var root in firstGeneration)
					{
						var navItem = (TreeViewItem) tree.ItemContainerGenerator.ContainerFromItem(root);
						if (navItem != null) 
						{
							navItem.HeaderTemplate = (DataTemplate) tree.FindResource("rootItem");
							if (root.Equals(RootItem))
							{
								navItem.BringIntoView();
							}
						}
					}
				}	
			}
		}
		
		public List<ItemViewModel> GetExpandedItems()
		{
			var list = new List<ItemViewModel>();
			foreach (var item in FirstGeneration)
			{
				list.Add(item);
				if (item.IsExpanded)
				{
					list.AddRange(GetExpandedItemsRecursive(item.Children));
				}
			}
			return list;
		}
		
		List<ItemViewModel> GetExpandedItemsRecursive(List<ItemViewModel> items)
		{
			var list = new List<ItemViewModel>();
			foreach (var item in items)
			{
				list.Add(item);
				if (item.IsExpanded)
				{
					list.AddRange(GetExpandedItemsRecursive(item.Children));
				}
			}
			return list;
		}
		#endregion
	}
}
