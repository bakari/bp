
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
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
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace EditorPlugin
{
	/// <summary>
	/// Description of AvalonEditPlugin.
	/// </summary>
	[Export(typeof(IPluginBase))]
	public class AvalonEditPlugin : IPluginBase
	{
		App mainApp;
		Lazy<IPluginBase, IBaseMetadata>[] plugins;
		Lazy<ICommand, IMainMenuCommandMetadata>[] mainMenuCommands;
		Lazy<ICommand, IToolbarCommandMetadata>[] toolBarCommands;
		DockingManager dockManager;
		
		
		public void Load(ApplicationCore.App app)
		{
			mainApp = app;
			mainMenuCommands = app.mainMenuCommands;
			toolBarCommands = app.toolbarCommands;
		}
		
		public void PluginsLoaded(Lazy<IPluginBase, IBaseMetadata>[] plugins)
		{
//			this.plugins = plugins;
//			dockManager = (DockingManager) mainApp.MainWindow.FindName("dockManager");
//			if (dockManager == null) 
//			{
//				return;
//			}
//			else
//			{
//				InitTabPane();
//				InitToolbar();
//				InitMainMenu();
//				
//				MRU.UpdateMRUMenu();
//				MRU.LoadOpenedTabs();
//				
//				mainApp.MainWindow.Closing += new CancelEventHandler(MainApp_MainWindow_Closing);
//			}
		}

		void MainApp_MainWindow_Closing(object sender, CancelEventArgs e)
		{
			MRU.SaveOpenedTabs();
		}
		
		void InitTabPane()
		{
			var tabPane = new DocumentPane();
			tabPane.Name = "tabPane";
			mainApp.MainWindow.RegisterName("tabPane", tabPane);
			
			var centerResizingPanel = (ResizingPanel) mainApp.MainWindow.FindName("centerResizingPanel");
			if (centerResizingPanel == null)
			{
				var mainResizingPanel = new ResizingPanel();
				mainResizingPanel.Orientation = Orientation.Horizontal;
				mainResizingPanel.Name = "mainResizingPanel";
				mainApp.MainWindow.RegisterName("mainResizingPanel", mainResizingPanel);
				
				centerResizingPanel = new ResizingPanel();
				centerResizingPanel.Name = "centerResizingPanel";
				mainApp.MainWindow.RegisterName("centerResizingPanel", centerResizingPanel);
				centerResizingPanel.Orientation = Orientation.Vertical;
				
				mainResizingPanel.Children.Add(centerResizingPanel);
				centerResizingPanel.Children.Add(tabPane);
				dockManager.Content = mainResizingPanel;
			}
			else
			{
				centerResizingPanel.Children.Insert(0, tabPane);
			}
			
			Editor.CreateNewTab("Untitled", null);
		}
		
		public void InitMainMenu()
		{
			Menu mainMenu = (Menu) mainApp.MainWindow.FindName("mainMenu");
			if (mainMenu == null) return;
			foreach (var topLevelMenu in mainMenuCommands.OrderBy(c => c.Metadata.MenuOrder).GroupBy(c => c.Metadata.Menu)) {
				var topLevelMenuItem = mainMenu.Items.OfType<MenuItem>().FirstOrDefault(m => (m.Header as string) == topLevelMenu.Key);
				foreach (var category in topLevelMenu.GroupBy(c => c.Metadata.MenuCategory)) {
					if (topLevelMenuItem == null) {
						topLevelMenuItem = new MenuItem();
						topLevelMenuItem.Header = topLevelMenu.Key;
						mainMenu.Items.Add(topLevelMenuItem);
					} else if (topLevelMenuItem.Items.Count > 0) {
						topLevelMenuItem.Items.Add(new Separator());
					}
					foreach (var entry in category) {
						MenuItem menuItem = new MenuItem();
						menuItem.Command = entry.Value;
						if (!string.IsNullOrEmpty(entry.Metadata.Header))
							menuItem.Header = entry.Metadata.Header;
						if (!string.IsNullOrEmpty(entry.Metadata.ElementName))
						{
						    menuItem.Name = entry.Metadata.ElementName;
							ApplicationCore.App.Instance.RegisterName(entry.Metadata.ElementName, menuItem);
						}
						if (!string.IsNullOrEmpty(entry.Metadata.MenuIcon)) 
						{
							Uri uri;
							var name = entry.Value.GetType().Assembly.GetName();
							try {
								uri = new Uri("pack://application:,,,/" + name.Name + ";v" + name.Version + ";component/" + entry.Metadata.MenuIcon);
								BitmapImage image = new BitmapImage(uri);
								menuItem.Icon = new Image {
									Width = 16,
									Height = 16,
									Source = image
								};
							}
							catch (Exception) {}
						}
						var modifier = entry.Metadata.CommandModifierKey!=null ? (ModifierKeys)entry.Metadata.CommandModifierKey : ModifierKeys.None;
						if (entry.Metadata.CommandHotKey != null) 
						{
							mainApp.MainWindow.InputBindings.Add(new KeyBinding(entry.Value, (Key)entry.Metadata.CommandHotKey, modifier));
							menuItem.InputGestureText = ( !modifier.Equals(ModifierKeys.None)? modifier+"+" : "" ) + entry.Metadata.CommandHotKey.ToString();
						}
						
						menuItem.IsEnabled = entry.Metadata.IsEnabled;
						topLevelMenuItem.Items.Add(menuItem);
					}
				}
			}
		}
		
		void InitToolbar()
		{
			ToolBar toolBar = (ToolBar) mainApp.MainWindow.FindName("toolBar");
			if (toolBar == null) return;
			foreach (var commandGroup in toolBarCommands.OrderBy(c => c.Metadata.ToolbarOrder).GroupBy(c => c.Metadata.ToolbarCategory)) 
			{
				toolBar.Items.Add(new Separator());
				foreach (var command in commandGroup) 
				{
					try {
						Uri uri;
						var name = command.Value.GetType().Assembly.GetName();
						uri = new Uri("pack://application:,,,/" + name.Name + ";v" + name.Version + ";component/" + command.Metadata.ToolbarIcon);
						BitmapImage image = new BitmapImage(uri);
						var btn = new Button {
							Command = command.Value,
							ToolTip = command.Metadata.ToolTip,
							Tag = command.Metadata.Tag,
							Content = new Image { 
								Width = 16,
								Height = 16,
								Source = image
							}
						};
						ModifierKeys modifierKey;
						if (command.Metadata.ModifierKey == null) 
							modifierKey = ModifierKeys.None;
						else
							modifierKey = (ModifierKeys) command.Metadata.ModifierKey;
						if (command.Metadata.HotKey != null) mainApp.MainWindow.InputBindings.Add(new KeyBinding(command.Value, (Key)command.Metadata.HotKey, modifierKey));
						toolBar.Items.Add(btn);
						
					} catch (Exception exc) 
					{ 
						MessageBox.Show(exc.ToString()); 
					}
				}
			}
		}
	}
}
