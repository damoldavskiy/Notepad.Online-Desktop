using System.Windows;

namespace NotepadOnlineDesktop.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new ViewModel.MainWindow(text, extensions);
            viewModel.Close = Close;
            
            Closing += viewModel.Closing;
            CommandBindings.AddRange(viewModel.Bindings);
            DataContext = viewModel;
        }
    }
}
