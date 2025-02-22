using System;
using System.Windows.Input;

namespace UpdatePiWebConnection.Common
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            if (_execute == null)
                throw new ArgumentNullException(nameof(execute));

            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute is null || _canExecute.Invoke(parameter);


        public void Execute(object parameter) => _execute?.Invoke(parameter);

        public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    }
}
