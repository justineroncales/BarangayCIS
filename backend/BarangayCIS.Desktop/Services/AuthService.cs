using System;
using BarangayCIS.Desktop.Models;

namespace BarangayCIS.Desktop.Services
{
    public class AuthService
    {
        private string? _token;
        private UserDto? _currentUser;

        public string? Token => _token;
        public UserDto? CurrentUser => _currentUser;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

        public void SetToken(string token)
        {
            _token = token;
            // Save to settings or secure storage
            try
            {
                Properties.Settings.Default.AuthToken = token;
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Settings may not be initialized, ignore
            }
        }

        public void SetUser(UserDto user)
        {
            _currentUser = user;
        }

        public void Logout()
        {
            _token = null;
            _currentUser = null;
            try
            {
                Properties.Settings.Default.AuthToken = string.Empty;
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Settings may not be initialized, ignore
            }
        }

        public void LoadSavedToken()
        {
            try
            {
                var savedToken = Properties.Settings.Default.AuthToken;
                if (!string.IsNullOrEmpty(savedToken))
                {
                    _token = savedToken;
                }
            }
            catch
            {
                // Settings may not be initialized, ignore
            }
        }
    }
}
