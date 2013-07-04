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
    public class ToolbarPlugin : IPluginBase
    {
    	[ImportMany("ToolbarCommand", typeof(ICommand))]
        public Lazy<ICommand, IToolbarCommandMetadata>[] toolbarCommands = null;
        
        [Import(typeof(MainWindow))]
        MainWindow mainWindow = null;

        public void Load(CompositionContainer container)
        {
        	if (container == null || mainWindow == null || toolbarCommands == null)
        		return;
        	
			InitToolbar();
        }
        
        void InitToolbar()
		{
			var toolBar = (ToolBar) mainWindow.toolBar;
			if (toolBar == null) return;
			foreach (var commandGroup in toolbarCommands.OrderBy(c => c.Metadata.ToolbarOrder).GroupBy(c => c.Metadata.ToolbarCategory)) 
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
						if (command.Metadata.HotKey != null) mainWindow.InputBindings.Add(new KeyBinding(command.Value, (Key)command.Metadata.HotKey, modifierKey));
						toolBar.Items.Add(btn);
						
					} catch (Exception) 
					{
					}
				}
			}
			if (toolBar.HasItems)
				toolBar.Visibility = Visibility.Visible;
		}
    }
}
