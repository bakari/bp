using System;
using System.Collections.Generic;

namespace ExplorerPlugin
{
	/// <summary>
	/// Item in DirectoryTree
	/// </summary>
	public class DirectoryTreeItem
	{
		private List<DirectoryTreeItem> children = new List<DirectoryTreeItem>();
		public IList<DirectoryTreeItem> Children { get{ return children; } }
		
		public string Header { get; set; }
		public string Path { get; set; }
		public TYPE Type { get; set; }
		
		public DirectoryTreeItem()
		{
		}
		
		public DirectoryTreeItem(TYPE type)
		{
			Header = type.ToString();
			Type = type;
		}
		
		public enum TYPE
		{
			File,
			Folder,
			Drive,
			Info,
			DummyNode
		}
	}
}
