
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ApplicationCore;
using AvalonDock;
using EditorPlugin;
using ExplorerPlugin;
using Framework.Core;
using MainWindowPlugin;

namespace ExplorerPlugin
{
	/// <summary>
	/// Description of AvalonEditPlugin.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[Export(typeof(FileExplorerPlugin))]
	public class FileExplorerPlugin : IPluginBase
	{
		[Import(typeof(MainWindow))]
		MainWindow mainWindow = null;
		
		[Import(typeof(Editor))]
		Editor editorPlugin = null;
		
		CompositionContainer container;
		DockingManager dockManager;
			
		string XmlFileName = System.AppDomain.CurrentDomain.BaseDirectory +@"\treeConfig.xml";
		
		public void Load(CompositionContainer container)
		{
			this.container = container;
			if (mainWindow == null)
				return;
			
			dockManager = mainWindow.DockManager;
			if (dockManager == null) 
			{
				return;
			}
			else
			{
				InitDirectoryTree();
				mainWindow.Closing += new CancelEventHandler(MainApp_MainWindow_Closing);
				content.Hide();
			}
		}
		
		DockableContent content;
		public DockableContent ExplorerContent { get { return content; } }
		
		DirectoryTree treeView;
		public DirectoryTree TreeView { get { return treeView; } }
			
		public void InitDirectoryTree()
		{
			var mainResizingPanel = dockManager.Content as ResizingPanel;
			if (mainResizingPanel == null || editorPlugin == null) 
				return;
			
			var pane = new DockablePane();
			content = new DockableContent();
			content.Name = "explorerContent";
			content.Title = "Explorer";
			mainWindow.RegisterName(content.Name, content);
			
			ResizingPanel.SetResizeWidth(pane, new GridLength(200));
			ResizingPanel.SetResizeWidth(content, new GridLength(200));
			
			treeView = new DirectoryTree(editorPlugin);
			treeView.Name = "treeView";
			mainWindow.RegisterName(treeView.Name, treeView);
			dockManager.MouseMove += new MouseEventHandler(DockManager_MouseMove);
			
			content.Content = treeView;
			pane.Items.Add(content);
			mainResizingPanel.Children.Insert(0,pane);
		}
		
		public void LoadModel()
		{
			treeView.LoadTree();
			treeView.Tree.UpdateLayout();
			if (File.Exists(XmlFileName))
			{
				ModelSerializer.DeserializeModel(treeView.Model, XmlFileName);	
				treeView.Tree.UpdateLayout();
				treeView.Tree.Items.Refresh();
			}
		}
		
		public void SaveModel()
		{
			if (treeView.Model != null)
			{
				if (!treeView.Model.SearchText.IsEmpty())
				{
					treeView.Model.FirstGeneration.Clear();
					treeView.Model.FirstGeneration.Add(treeView.Model.RootItem);
				}
				try 
				{
					ModelSerializer.SerializeModel(treeView.Model, XmlFileName);
				}
				catch(Exception)
				{
					// couldn't access file
				}
			}
		}
		
		void DockManager_MouseMove(object sender, MouseEventArgs e)
		{
			if (treeView.Model == null && e.GetPosition(sender as DockingManager).X < 30)
			{
				dockManager.MouseMove -= DockManager_MouseMove;
				if (mainWindow != null)
				{
					mainWindow.SetStatus("Loading file system...");
				}
				LoadModel();
				
				if (mainWindow != null)
				{
					mainWindow.SetStatus("Ready");
				}
			}
		}
		
		void MainApp_MainWindow_Closing(object sender, CancelEventArgs e)
		{
			SaveModel();
		}
		
		static ModelSerializer modelSerializer;
		public static ModelSerializer ModelSerializer 
		{ 
			get 
			{
				if (modelSerializer == null)
				{
					modelSerializer = new ModelSerializer();
				}
				return modelSerializer;
			}
		}
	}
}
