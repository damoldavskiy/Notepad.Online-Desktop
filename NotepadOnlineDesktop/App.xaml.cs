using System.Windows;
using System.Windows.Threading;

namespace NotepadOnlineDesktop
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled exception occured: " + e.Exception.Message + "\nSource: " + e.Exception.Source + "\n\nStack trace: " + e.Exception.StackTrace + "\n\nIt's recommended to contact with the developer: party_50@mail.ru", "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
