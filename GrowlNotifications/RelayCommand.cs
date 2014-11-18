using System;
using System.Windows.Input;

namespace WpfGrowlNotifications {
	public class RelayCommand : ICommand {
		private readonly Action _cmdExecutionAction;

		private readonly Func<bool> _canExecute;

		public RelayCommand(Action cmdExecutionAction)
			: this(cmdExecutionAction, null) {
		}

		public RelayCommand(Action cmdExecutionAction, Func<bool> canExecute) {
			if (cmdExecutionAction == null) {
				throw new ArgumentNullException("cmdExecutionAction");
			}

			_cmdExecutionAction = cmdExecutionAction;
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public void RaiseCanExecuteChanged() {
			var handler = CanExecuteChanged;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}
		}

		public bool CanExecute(object parameter) {
			return _canExecute == null || _canExecute();
		}

		public void Execute(object parameter) {
			if (CanExecute(parameter)) {
				_cmdExecutionAction();
			}
		}
	}
}