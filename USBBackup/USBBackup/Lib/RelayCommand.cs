using System;
using System.Windows.Input;

namespace CSVChart.Bib
{
    class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _canExecute = canExecute ?? new Func<object, bool>(o => true);
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}
