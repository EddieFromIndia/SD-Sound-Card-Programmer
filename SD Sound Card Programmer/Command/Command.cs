namespace SD_Sound_Card_Programmer;

public class Command : ICommand
{
    private readonly Action<object> executeMethod;
    private readonly Func<object, bool>? canExecuteMethod;

    public Command(Action<object> ExecutableMethod, Func<object, bool> CanExecuteMethod)
    {
        executeMethod = ExecutableMethod;
        canExecuteMethod = CanExecuteMethod;
    }

    public Command(Action<object> ExecutableMethod)
    {
        executeMethod = ExecutableMethod;
    }

    public bool CanExecute(object? parameter)
    {
        return canExecuteMethod == null || canExecuteMethod(parameter!);
    }

    public void Execute(object? parameter)
    {
        executeMethod(parameter!);
    }

    public event EventHandler? CanExecuteChanged {
        add {
            CommandManager.RequerySuggested += value;
        }
        remove {
            CommandManager.RequerySuggested -= value;
        }
    }
}
