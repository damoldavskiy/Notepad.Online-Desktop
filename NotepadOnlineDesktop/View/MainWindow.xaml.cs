using System.Windows;

namespace NotepadOnlineDesktop.View
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public MainWindow()
        {
            Model.ThemeManager.Update();

            InitializeComponent();

            var viewModel = new ViewModel.MainWindow(text, extensions);
            viewModel.Close = Close;
            
            Closing += viewModel.Closing;
            CommandBindings.AddRange(viewModel.Bindings);
            DataContext = viewModel;

            Instance = this;
        }
    }
}
