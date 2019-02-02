using System.Windows;

namespace NotepadOnlineDesktop.View
{
    public partial class ManagerWindow : Window
    {

        public ManagerWindow()
        {
            InitializeComponent();

            var viewModel = new ViewModel.ManagerWindow();
            DataContext = viewModel;
        }
    }
}
