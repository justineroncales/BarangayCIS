using System.Windows;
using BarangayCIS.Desktop.Views;

namespace BarangayCIS.Desktop
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            MainWindow = loginWindow;
        }
    }
}

