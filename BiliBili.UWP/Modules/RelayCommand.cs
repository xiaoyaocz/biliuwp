using System;
using System.Windows.Input;

namespace BiliBili.UWP.Modules
{
	public class RelayCommand<T> : ICommand
	{
		private Func<T, bool> _CanExecute;
		private Action<T> _Command;

		public RelayCommand(Action<T> command) : this(command, null)
		{
		}

		public RelayCommand(Action<T> command, Func<T, bool> canexecute)
		{
			if (command == null)
			{
				throw new ArgumentException("command");
			}
			_Command = command;
			_CanExecute = canexecute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return _CanExecute == null ? true : _CanExecute((T)parameter);
		}

		public void Execute(object parameter)
		{
			_Command((T)parameter);
		}
	}

	public class RelayCommand : ICommand
	{
		private Action<bool> _CanExecute;
		private Action _Command;

		public RelayCommand(Action command) : this(command, null)
		{
		}

		public RelayCommand(Action command, Action<bool> canexecute)
		{
			if (command == null)
			{
				throw new ArgumentException("command");
			}
			_Command = command;
			_CanExecute = canexecute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return _CanExecute == null;
		}

		public void Execute(object parameter)
		{
			_Command();
		}
	}
}