using System.Windows;
using System.Windows.Controls;
using BarangayCIS.Desktop.Services;
using BarangayCIS.Desktop.Views;

namespace BarangayCIS.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly AuthService _authService;

        public MainWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
            UpdateUserInfo();
        }

        private void UpdateUserInfo()
        {
            var user = _authService.CurrentUser;
            if (user != null)
            {
                UserInfoText.Text = $"Welcome, {user.Username} ({user.Role})";
            }
        }

        private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationListBox.SelectedItem is ListBoxItem selectedItem)
            {
                var tag = selectedItem.Tag?.ToString();
                if (string.IsNullOrEmpty(tag)) return;

                UserControl? content = tag switch
                {
                    "Dashboard" => new Views.DashboardView(),
                    "Residents" => new Views.ResidentsView(),
                    "Certificates" => new Views.CertificatesView(),
                    "Incidents" => new Views.IncidentsView(),
                    "Financial" => new Views.FinancialView(),
                    "Projects" => new Views.ProjectsView(),
                    "Health" => new Views.HealthView(),
                    "BHW" => new Views.BHWView(),
                    "SeniorCitizen" => new Views.SeniorCitizenView(),
                    "Reports" => new Views.ReportsView(),
                    "Announcements" => new Views.AnnouncementsView(),
                    "Staff" => new Views.StaffView(),
                    "Inventory" => new Views.InventoryView(),
                    "BusinessPermits" => new Views.BusinessPermitsView(),
                    "Suggestions" => new Views.SuggestionsView(),
                    "Disaster" => new Views.DisasterView(),
                    _ => null
                };

                if (content != null)
                {
                    ContentArea.Content = content;
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
            Application.Current.MainWindow = loginWindow;
        }
    }
}
