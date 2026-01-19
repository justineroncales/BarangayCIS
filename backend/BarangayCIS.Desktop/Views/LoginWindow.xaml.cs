using System.Windows;
using System.Windows.Controls;
using BarangayCIS.Desktop.Services;
using BarangayCIS.Desktop.ViewModels;

namespace BarangayCIS.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            var apiClient = new ApiClient();
            var authService = new AuthService();
            _viewModel = new LoginViewModel(apiClient, authService);
            DataContext = _viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && _viewModel != null)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }
    }
}
