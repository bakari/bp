
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ApplicationCore;
using MainWindowPlugin;
using ICSharpCode.ILSpy;

namespace EditorPlugin
{
	/// <summary>
	/// Description of ExitCommand.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_File", MenuIcon = "Images/Delete.png", Header = "_Exit", MenuCategory = "4",  MenuOrder = 1.7)]
	public class ExitCommand : SimpleCommand
	{
		
		public override void Execute(object parameter)
		{
			App.Current.Shutdown();
		}
	}
}

