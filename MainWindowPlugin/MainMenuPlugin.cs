using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ApplicationCore;
using ICSharpCode.ILSpy;

namespace MainWindowPlugin
{
	[Export(typeof(IPluginBase))]
	[Export(typeof(MainMenuPlugin))]
    public class MainMenuPlugin : IPluginBase
    {
    	[ImportMany("MainMenuCommand", typeof(ICommand))]
        public Lazy<ICommand, IMainMenuCommandMetadata>[] mainMenuCommands = null;
        
        [Import(typeof(MainWindow))]
        MainWindow mainWindow = null;
        
        public bool Loaded { get; set; }
        
        public void Load(CompositionContainer container)
        {
        	if (container == null || mainWindow == null)
        		return;
        	
        	if (Loaded)
        		return;
        	Loaded = true;
       		
			InitMainMenu();
        }

       public void InitMainMenu()
		{
			var mainMenu = (Menu) mainWindow.mainMenu;
			if (mainMenu == null) 
				return;
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
							mainWindow.RegisterName(entry.Metadata.ElementName, menuItem);
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
							mainWindow.InputBindings.Add(new KeyBinding(entry.Value, (Key)entry.Metadata.CommandHotKey, modifier));
							if (!string.IsNullOrEmpty(entry.Metadata.InputGestureText))
								menuItem.InputGestureText = entry.Metadata.InputGestureText;
							else
								menuItem.InputGestureText = ( !modifier.Equals(ModifierKeys.None)? modifier+"+" : "" ) + entry.Metadata.CommandHotKey.ToString();
						}
						
						menuItem.IsEnabled = entry.Metadata.IsEnabled;
						topLevelMenuItem.Items.Add(menuItem);
					}
				}
			}
		}
    }
}
