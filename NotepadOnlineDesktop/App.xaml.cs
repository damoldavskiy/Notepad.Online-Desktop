using System.Windows;
using System.Windows.Threading;

namespace NotepadOnlineDesktop
{
    public partial class App : Application
    {
        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            foreach (var extension in Model.ExtensionManager.LoadedExtensions)
                if (e.Exception.Source == extension.GetType().Namespace)
                {
                    MessageBox.Show($"Extension {extension.Name} throwed exception: {e.Exception.Message}\n\nCorrect work of this extension is not guarenteed", "Extension error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            MessageBox.Show($"Unhandled application exception occured: {e.Exception.Message}\nSource: {e.Exception.Source}\n\nStack trace: {e.Exception.StackTrace}\n\nIt's recommended to contact with the developer: party_50@mail.ru", "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
