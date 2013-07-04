
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using System.Windows.Input;
using ApplicationCore;
using MainWindowPlugin;

namespace EditorPlugin
{
	/// <summary>
	/// Description of SimpleCommand.
	/// </summary>
	public class SimpleCommand : ICommand, IPluginBase
	{
		[Import(typeof(MainWindow))]
		protected MainWindow mainWindow;
		
		[Import(typeof(Editor))]
		protected Editor editorPlugin;
		
		protected CompositionContainer container;
		
		public virtual event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
		
		public virtual void Execute(object parameter)
		{
		}
		
		public virtual bool CanExecute(object parameter)
		{
			return true;
		}
		
		public virtual void Load(CompositionContainer container)
		{
			this.container = container;
		}
	}
}
