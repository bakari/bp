using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;

namespace ExplorerPlugin
{
	/// <summary>
	/// Wrapper for DirectoryTreeItem
	/// </summary>
	public class ItemViewModel : INotifyPropertyChanged, IEquatable<ItemViewModel>
	{
		private List<ItemViewModel> children;
		private ItemViewModel parent;
		private DirectoryTreeItem item;
		
		private bool isSelected;
		private bool isExpanded;
		
		#region Constructors
		public ItemViewModel(DirectoryTreeItem item) : this(item,null)
		{
		}
		
		public ItemViewModel(DirectoryTreeItem item, ItemViewModel parent)
		{
			this.item = item;
			this.parent = parent;
            this.children = item.Children.Select(child => new ItemViewModel(child, this)).ToList();
		}
		#endregion
		
		#region Properties
		public List<ItemViewModel> Children { get { return children; } }
		public string Header { get { return item.Header; } set { item.Header = value; } }
		public string Path { get { return item.Path; } set { item.Path = value; } }
		public DirectoryTreeItem.TYPE Type { get { return item.Type; } set { item.Type = value; } }
        public ItemViewModel Parent { get; set; }
		
		public bool IsExpanded 
		{
			get { return isExpanded; }
			set 
			{
				if (value != isExpanded)
				{
					isExpanded = value;
					this.OnPropertyChanged("IsExpanded");
					this.OnExpanded();
				}
				
				if (isExpanded && parent != null)
				{
					parent.isExpanded = true;
				}
			}
		}
		
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				if (value != isSelected)
				{
					isSelected = value;
					this.OnPropertyChanged("IsSelected");
				}
			}
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null) 
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		
		public event RoutedEventHandler Expanded;
		protected virtual void OnExpanded()
		{
			if (this.Expanded != null)
				this.Expanded(this, new RoutedEventArgs());
		}
		#endregion
		
		/// <summary>
		/// Brainless way to make a clone
		/// </summary>
		public ItemViewModel Clone()
		{
            var clone = new ItemViewModel(new DirectoryTreeItem())
            {
                Header = Header,
                Path = Path,
                Type = Type,
                IsExpanded = IsExpanded,
                IsSelected = IsSelected,
                Expanded = Expanded
            };
			
			if (Parent != null)
			{
				clone.Parent = Parent.Clone();
			}
			
			if (Type != DirectoryTreeItem.TYPE.File)
			{
				clone.Children.Add(new ItemViewModel(new DirectoryTreeItem(DirectoryTreeItem.TYPE.DummyNode),clone));			
			}
			return clone;
		}
		
		public bool Equals(ItemViewModel other)
		{
			if (other == null) return false;
			return this.Path == other.Path && this.Header == other.Header && this.Type == other.Type;
		}
	}
}
