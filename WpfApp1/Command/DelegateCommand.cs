using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp1.Command
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Action<object> action) { this.ExecuteAction = action; }
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            if (CanExecuteChanged != null)
            {
                this.CanExecuteFunc(parameter);
            }
            return true;
        }

        public void Execute(object? parameter)
        {
            if (ExecuteAction != null)
            {
                this.ExecuteAction(parameter);
            }
        }

        public Action<object> ExecuteAction {  get; set; }
        public Func<object,bool> CanExecuteFunc {  get; set; }
    }
}
