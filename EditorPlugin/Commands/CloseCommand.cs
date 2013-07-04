
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using MainWindowPlugin;
using ApplicationCore;
using AvalonDock;
using ICSharpCode.ILSpy;

namespace EditorPlugin
{
	/// <summary>
	/// Description of CloseCommand.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_File", MenuIcon = "Images/Close.png", Header = "_Close", MenuCategory = "1",  MenuOrder = 1.3)]
	public class CloseCommand : SimpleCommand
	{
		
		public override void Execute(object parameter)
		{
			if (mainWindow != null) 
			{
				var documentContent = mainWindow.ActiveTabContent as DocumentContent;
				if (documentContent == null)
					return;
				documentContent.Close();
			}
		}
		
		public override bool CanExecute(object parameter)
		{
			if (mainWindow != null)
			{
				var tabPane = mainWindow.TabPane;
				
				if (tabPane != null && !tabPane.Items.IsEmpty)
				{
					return true;	
				}
			}
			return false;
		}
	}
}
