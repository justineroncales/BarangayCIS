using System;
using System.Windows;
using BarangayCIS.Desktop.Models;
using BarangayCIS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BarangayCIS.Desktop.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        public LoginViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter username and password";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var loginRequest = new LoginRequest
                {
                    Username = Username,
                    Password = Password
                };

                var response = await _apiClient.PostAsync<LoginResponse>("auth/login", loginRequest);
                
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    _authService.SetToken(response.Token);
                    _authService.SetUser(response.User);
                    _apiClient.SetToken(response.Token);
                    
                    // Navigate to main window
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        Application.Current.MainWindow?.Close();
                        Application.Current.MainWindow = mainWindow;
                    });
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
