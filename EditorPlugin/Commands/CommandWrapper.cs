using System;
using System.Windows.Input;

namespace EditorPlugin
{
	public class CommandWrapper : SimpleCommand
	{
		private ICommand wrappedCommand;

		public CommandWrapper(ICommand wrappedCommand)
		{
			this.wrappedCommand = wrappedCommand;
		}

		public static ICommand Unwrap(ICommand command)
		{
			CommandWrapper w = command as CommandWrapper;
			if (w != null)
				return w.wrappedCommand;
			else
				return command;
		}

		public override event EventHandler CanExecuteChanged
		{
			add { wrappedCommand.CanExecuteChanged += value; }
			remove { wrappedCommand.CanExecuteChanged -= value; }
		}

		public override void Execute(object parameter)
		{
			wrappedCommand.Execute(parameter);
		}

		public override bool CanExecute(object parameter)
		{
			return wrappedCommand.CanExecute(parameter);
		}
	}
}
