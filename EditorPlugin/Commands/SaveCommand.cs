
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using AvalonDock;
using ICSharpCode.ILSpy;
using ICSharpCode.AvalonEdit;
using ApplicationCore;
using MainWindowPlugin;

namespace EditorPlugin
{
	/// <summary>
	/// SaveCommand.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_File", MenuIcon = "Images/Save.png", Header = "_Save", MenuCategory = "2",  MenuOrder = 1.4, InputGestureText = "Ctrl+S", CommandHotKey = System.Windows.Input.Key.S, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "Save file", ToolbarIcon = "Images/Save.png", ToolbarCategory = "1", ToolbarOrder = 1.2, HotKey = System.Windows.Input.Key.S, ModifierKey = ModifierKeys.Control)]
	public class SaveCommand : SimpleCommand
	{
		public override void Execute(object parameter)
		{
			if (editorPlugin == null)
				return;
			editorPlugin.SaveFile();
		}
		
		public override bool CanExecute(object parameter)
		{
			if (mainWindow != null)
			{
				var tabPane = mainWindow.TabPane;
				if (tabPane != null && !tabPane.Items.IsEmpty && (tabPane.SelectedItem as 
				                               DocumentContent) != null && (tabPane.SelectedItem as 
				                               DocumentContent).Title.EndsWith("*") &&
				                              (tabPane.SelectedItem as 
				                                DocumentContent).Tag != null)
				{
					return true;
				}
			}
			return false;
		}
	}
	
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_File", Header = "_Save As..", MenuCategory = "2",  MenuOrder = 1.5)]
	public class SaveAsCommand : SimpleCommand
	{	
		public override void Execute(object parameter)
		{
			if (editorPlugin == null || mainWindow == null)
				return;
			var content = mainWindow.ActiveTabContent as DocumentContent;
			if (content != null) content.Tag = null;
			editorPlugin.SaveFile();
		}
		
		public override bool CanExecute(object parameter)
		{
			if (mainWindow != null)
			{
				var tabPane = mainWindow.TabPane;
				if (editorPlugin != null && tabPane != null && tabPane.HasItems && editorPlugin.ActiveEditor != null)
				{
					return true;
				}
			}
			return false;
		}
	}
}
