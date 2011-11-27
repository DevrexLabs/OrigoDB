using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Todo.Wpf
{
    public class DelegateCommand : ICommand
    {

        Action _executeAction;
        Func<bool> _canExecuteFunction;


        public DelegateCommand(Action executeAction)
        {
            _executeAction = executeAction;
            _canExecuteFunction = () => true;
        }

        public DelegateCommand(Action executeAction, Func<bool> canExecuteFunction)
        {
            _executeAction = executeAction;
            _canExecuteFunction = canExecuteFunction;
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunction.Invoke();
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            _executeAction.Invoke();
        }
    }
}
