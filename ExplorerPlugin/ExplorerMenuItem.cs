using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using ApplicationCore;
using AvalonDock;
using EditorPlugin;
using ICSharpCode.ILSpy;
using ExplorerPlugin;
using MainWindowPlugin;

namespace ExplorerPlugin
{
	/// <summary>
	/// Description of CloseCommand.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_View", MenuIcon = "Images/Explorer.png", Header = "_Explorer", MenuCategory = "1",  MenuOrder = 3)]
	public class ExplorerMenuItem : SimpleCommand
	{
		[Import(typeof(FileExplorerPlugin))]
		FileExplorerPlugin plugin = null;
		
		public override void Execute(object parameter)
		{
			if (plugin == null)
				return;
			
			var treeView = plugin.TreeView;
			var explorerContent = plugin.ExplorerContent;
			
			if (treeView == null || explorerContent == null) 
				return;
			
			if (treeView.Model == null)
				LoadModel(treeView);
			
			if (explorerContent.State == DockableContentState.AutoHide)
				explorerContent.Activate();
			else
				explorerContent.Show(mainWindow.DockManager);
		}
		
		private string XmlFileName = System.AppDomain.CurrentDomain.BaseDirectory +@"\treeConfig.xml";
		
		public void LoadModel(DirectoryTree treeView)
		{
			treeView.LoadTree();
			treeView.Tree.UpdateLayout();
			if (File.Exists(XmlFileName))
			{
				FileExplorerPlugin.ModelSerializer.DeserializeModel(treeView.Model, XmlFileName);	
				treeView.Tree.UpdateLayout();
				treeView.Tree.Items.Refresh();
			}
		}
	}
}
