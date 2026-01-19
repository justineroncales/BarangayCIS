using System.Windows.Controls;
using System.Windows;
using BarangayCIS.Desktop.Services;
using BarangayCIS.Desktop.ViewModels;

namespace BarangayCIS.Desktop.Views
{
    public partial class ResidentsView : UserControl
    {
        public ResidentsView()
        {
            InitializeComponent();
            var apiClient = new ApiClient();
            var authService = new AuthService();
            authService.LoadSavedToken();
            if (!string.IsNullOrEmpty(authService.Token))
            {
                apiClient.SetToken(authService.Token);
            }
            DataContext = new ResidentsViewModel(apiClient);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ResidentsViewModel vm)
            {
                _ = vm.LoadResidentsCommand.ExecuteAsync(null);
            }
        }
    }
}
