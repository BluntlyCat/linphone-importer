using System.Windows;

namespace Contabo.Tools.Linphone.Importer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string DbFilePath { get; set; }
        public static DataBase DB { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DB = new DataBase();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            DB?.Close();
        }
    }
}
