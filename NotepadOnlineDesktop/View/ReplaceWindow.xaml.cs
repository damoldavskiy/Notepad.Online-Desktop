using System.Windows;

namespace NotepadOnlineDesktop.View
{
    public partial class ReplaceWindow : Window
    {
        public ViewModel.ReplaceWindow ViewModel;

        public ReplaceWindow()
        {
            InitializeComponent();
            text.Focus();
            DataContext = ViewModel = new ViewModel.ReplaceWindow();
        }
    }
}
