
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ApplicationCore;
using ICSharpCode.ILSpy;
using MainWindowPlugin;

namespace ILSpyPlugin
{
	/// <summary>
	/// Assumes ILSpy in ILSpy\ILSpy.exe
	/// </summary>
	[ExportMainMenuCommand(Menu = "_View", MenuIcon = "ILSpy.ico", Header = "_ILSpy", MenuCategory = "3", MenuOrder = 3.4)]
	[ExportToolbarCommand(ToolTip = "Run ILSpy", ToolbarIcon = "ILSpy.ico", ToolbarCategory = "5", ToolbarOrder = 5.1)]
	public class ILSpyPlugin : ICommand
	{
		public void Execute(object parameter)
		{
			try 
			{
				var p = new Process();
				p.StartInfo.FileName = @"ILSpy\ILSpy.exe";
				
				// assembly ie. "ILSpy.exe"
				// tag ie. "ICSharpCode.ILSpy.LoadedAssembly" 
				//
				// p.StartInfo.Arguments = assembly +" /navigateTo:T:" +tag; 
				
				p.Start();
			}
			catch (Exception) 
			{
				MessageBox.Show("Couldn't locate ILSpy.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}
		
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
		
		public bool CanExecute(object parameter)
		{
			return true;
		}
	}
}
