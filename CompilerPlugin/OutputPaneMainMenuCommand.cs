
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ApplicationCore;
using MainWindowPlugin;
using ICSharpCode.ILSpy;
using AvalonDock;
using EditorPlugin;
using System.Linq;

namespace CompilerPlugin
{
	/// <summary>
	/// Description of OutputPaneMainMenuCommand.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_View", MenuIcon = "Images/Output.png", Header = "_Output", MenuCategory = "2", MenuOrder = 3.2)]
	public class OutputPaneMainMenuCommand : SimpleCommand
	{	
		[Import(typeof(CompilerPlugin))]
		CompilerPlugin compilerPlugin = null;
		
		public override void Execute(object parameter)
		{
			if (compilerPlugin != null) 
				compilerPlugin.ShowOutputPane();
		}
	}
	
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_View", MenuIcon = "Images/Errors.png", Header = "_Errors", MenuCategory = "2", MenuOrder = 3.3)]
	public class ErrorPaneMainMenuCommand : SimpleCommand
	{
		[Import(typeof(CompilerPlugin))]
		CompilerPlugin compilerPlugin = null;
		
		public override void Execute(object parameter)
		{
			if (compilerPlugin != null) 
				compilerPlugin.ShowErrorPane();
		}
	}
}
