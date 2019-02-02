using System;
using System.Windows.Input;

namespace NotepadOnlineDesktop.Model
{
    public class ActionCommand : ICommand
    {
        Action<object> action;

        public event EventHandler CanExecuteChanged;

        public ActionCommand(Action<object> action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
