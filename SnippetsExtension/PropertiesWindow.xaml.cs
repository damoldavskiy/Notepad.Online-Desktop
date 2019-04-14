using NotepadOnlineDesktopExtensions;

using System;
using System.IO;
using System.Windows;

namespace SnippetsExtension
{
    public partial class PropertiesWindow : Window
    {
        readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "Config\\";
        IApplicationInstance app;

        public PropertiesWindow(IApplicationInstance app)
        {
            InitializeComponent();
            this.app = app;
            snippets.IsChecked = Properties.Settings.Default.snippets;
            brackets.IsChecked = Properties.Settings.Default.brackets;
            spaces.IsChecked = Properties.Settings.Default.spaces;
            tabs.IsChecked = Properties.Settings.Default.tabs;
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

        void Tabs_Checked(object sender, RoutedEventArgs e)
        {
            var check = tabs.IsChecked == true;
            Properties.Settings.Default.tabs = check;
            Properties.Settings.Default.Save();
        }

        void Snippets_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(configPath + "Snippets.ini"))
            {
                app.Open(configPath + "Snippets.ini");
                Close();
            }
            else
            {
                var res = MessageBox.Show("Snippets file not found. Do you want to create one?", "Error", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(configPath);
                    using (var stream = new StreamWriter(configPath + "Snippets.ini"))
                    {
                        stream.Write("#\n# Snippets file\n#");
                    }
                    app.Open(configPath + "Snippets.ini");
                    Close();
                }
            }
        }

        void Brackets_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(configPath + "Brackets.ini"))
            {
                app.Open(configPath + "Brackets.ini");
                Close();
            }
            else
            {
                var res = MessageBox.Show("Brackets file not found. Do you want to create one?", "Error", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(configPath);
                    using (var stream = new StreamWriter(configPath + "Brackets.ini"))
                    {
                        stream.Write("#\n# Brackets file\n#");
                    }
                    app.Open(configPath + "Brackets.ini");
                    Close();
                }
            }
        }
    }
}
