using NotepadOnlineDesktopExtensions;
using System.IO;
using System.Windows;

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

        void Snippets_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\Config\\Snippets.ini"))
            {
                app.Open(Directory.GetCurrentDirectory() + "\\Config\\Snippets.ini");
                Close();
            }
            else
            {
                MessageBox.Show("Snippets file not found", "Error");
            }
        }

        void Brackets_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\Config\\Brackets.ini"))
            {
                app.Open(Directory.GetCurrentDirectory() + "\\Config\\Brackets.ini");
                Close();
            }
            else
            {
                MessageBox.Show("Brackets file not found", "Error");
            }
        }
    }
}
