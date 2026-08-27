using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Makara.Desktop.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _currentPage = "工作流";

    public string CurrentPage
    {
        get => _currentPage;
        set { _currentPage = value; OnPropertyChanged(); }
    }

    public ICommand NavigateCommand { get; }

    public MainViewModel()
    {
        NavigateCommand = new RelayCommand(param =>
        {
            if (param is string page)
                CurrentPage = page switch
                {
                    "workflows" => "工作流",
                    "datasources" => "数据源",
                    "datasets" => "数据集",
                    "runs" => "执行记录",
                    "settings" => "设置",
                    _ => page
                };
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
