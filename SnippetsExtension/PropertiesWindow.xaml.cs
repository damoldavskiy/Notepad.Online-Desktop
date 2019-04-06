using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NotepadOnlineDesktopExtensions;

namespace SnippetsExtension
{
    public partial class PropertiesWindow : Window
    {
        IApplicationInstance app;

        public PropertiesWindow(IApplicationInstance app)
        {
            InitializeComponent();
            this.app = app;
            snippets.IsChecked = Properties.Settings.Default.snippets;
            brackets.IsChecked = Properties.Settings.Default.brackets;
            spaces.IsChecked = Properties.Settings.Default.spaces;
        }

        void Snippets_Checked(object sender, RoutedEventArgs e)
        {
            var check = snippets.IsChecked == true;
            Properties.Settings.Default.snippets = check;
            Properties.Settings.Default.Save();
        }

        void Brackets_Checked(object sender, RoutedEventArgs e)
        {
            var check = brackets.IsChecked == true;
            Properties.Settings.Default.brackets = check;
            Properties.Settings.Default.Save();
        }

        void Spaces_Checked(object sender, RoutedEventArgs e)
        {
            var check = spaces.IsChecked == true;
            Properties.Settings.Default.spaces = check;
            Properties.Settings.Default.Save();
        }

        void File_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\snippets.ini"))
            {
                app.Open(Directory.GetCurrentDirectory() + "\\snippets.ini");
                Close();
            }
            else
            {
                MessageBox.Show("Snippets file not found", "Error");
            }
        }
    }
}
