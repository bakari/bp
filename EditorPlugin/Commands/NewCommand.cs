
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MainWindowPlugin;
using AvalonDock;
using ApplicationCore;
using ICSharpCode.ILSpy;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace EditorPlugin
{
	/// <summary>
	/// Description of MenuPlugins.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_File", MenuIcon = "Images/New.png", Header = "_New", MenuCategory = "1", MenuOrder = 1.1, InputGestureText = "Ctrl+N", CommandHotKey = System.Windows.Input.Key.N, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "New file", ToolbarIcon = "Images/New.png", ToolbarCategory = "1", ToolbarOrder = 1.0, HotKey = System.Windows.Input.Key.N, ModifierKey = ModifierKeys.Control)]
	public class NewCommand : SimpleCommand
	{
		DocumentPane tabPane;
		
		public override void Execute(object parameter)
		{
			var title = "Untitled";
			var count = 0;
			tabPane = mainWindow.TabPane;
			if (tabPane == null)
				return;
			
			while((from DocumentContent item in tabPane.Items where item.Title.Trim('*') == title select item).ToList().Count > 0)
			{
				count++;
				title = "Untitled(" +count +")";
			}
			if (editorPlugin == null)
				return;
			editorPlugin.CreateNewTab(title,null);
		}
	}
}
