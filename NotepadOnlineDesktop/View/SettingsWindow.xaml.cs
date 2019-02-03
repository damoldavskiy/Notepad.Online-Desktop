using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NotepadOnlineDesktop.View
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public ViewModel.SettingsWindow ViewModel;

        public SettingsWindow()
        {
            InitializeComponent();
            
            DataContext = ViewModel = new ViewModel.SettingsWindow();
            Closed += ViewModel.Closed;
        }
    }
}
