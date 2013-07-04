using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ApplicationCore;
using MainWindowPlugin;
using ICSharpCode.ILSpy;
using Microsoft.Win32;

namespace EditorPlugin
{
	/// <summary>
	/// Description of MenuPlugins.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_File", MenuIcon = "Images/Open.png", Header = "_Open", MenuCategory = "1",  MenuOrder = 1.2,  InputGestureText = "Ctrl+O", CommandHotKey = System.Windows.Input.Key.O, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "Open file", ToolbarIcon = "Images/Open.png", ToolbarCategory = "1", ToolbarOrder = 1.1, HotKey = System.Windows.Input.Key.O, ModifierKey = ModifierKeys.Control)]
	public class OpenCommand : SimpleCommand
	{	
		public override void Execute(object parameter)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.CheckFileExists = true;
			Nullable<bool> result = dlg.ShowDialog();
			if (result == true && editorPlugin != null)
			{
				editorPlugin.CreateNewTab(dlg.SafeFileName, dlg.FileName);
			}
		}
	}
}
