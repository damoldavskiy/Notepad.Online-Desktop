using System.Windows;

namespace NotepadOnlineDesktop.View
{
    public partial class FindWindow : Window
    {
        public ViewModel.FindWindow ViewModel;

        public FindWindow()
        {
            InitializeComponent();
            text.Focus();
            DataContext = ViewModel = new ViewModel.FindWindow();
        }
    }
}
