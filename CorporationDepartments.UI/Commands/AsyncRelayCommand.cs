﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CorporationDepartments.UI.Commands
{
	public class AsyncRelayCommand<T> : ICommand
	{
		private readonly Func<T, Task> _execute;
		private readonly Func<T, bool> _canExecute;
		private bool _isExecuting;

		public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter)
		{
			return !_isExecuting && (_canExecute?.Invoke((T)parameter) ?? true);
		}

		public async void Execute(object parameter)
		{
			if (CanExecute(parameter))
			{
				try
				{
					_isExecuting = true;
					CommandManager.InvalidateRequerySuggested();
					await _execute((T)parameter);
				}
				finally
				{
					_isExecuting = false;
					CommandManager.InvalidateRequerySuggested();
				}
			}
		}
	}

	public class AsyncRelayCommand : ICommand
	{
		private readonly Func<Task> _execute;
		private readonly Func<bool> _canExecute;
		private bool _isExecuting;

		public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter)
		{
			return !_isExecuting && (_canExecute?.Invoke() ?? true);
		}

		public async void Execute(object parameter)
		{
			if (CanExecute(parameter))
			{
				try
				{
					_isExecuting = true;
					CommandManager.InvalidateRequerySuggested();
					await _execute();
				}
				finally
				{
					_isExecuting = false;
					CommandManager.InvalidateRequerySuggested();
				}
			}
		}
	}
}
