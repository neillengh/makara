using System.Windows;
using Makara.Desktop.ViewModels;

namespace Makara.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is LoginViewModel vm)
                vm.LoggedIn += () => DialogResult = true;
        };
    }

    private void OnLoginClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.LoginCommand.Execute(PwdBox.Password);
    }
}
