using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NotepadOnlineDesktop.ViewModel
{
    public class SettingsWindow : INotifyPropertyChanged
    {
        List<Model.SettingsPropertyItem> properties;

        Properties.Settings settings
        {
            get
            {
                return NotepadOnlineDesktop.Properties.Settings.Default;
            }
        }

        public List<Model.SettingsPageItem> Pages
        {
            get
            {
                var colorThemeComboBox = new ComboBox()
                {
                    Width = 120,
                    SelectedIndex = settings.theme == "light" ? 0 : 1,
                    ItemsSource = new[]
                    {
                        new TextBlock() { Text = "Light" },
                        new TextBlock() { Text = "Dark" }
                    }
                };
                colorThemeComboBox.SelectionChanged += (s, e) =>
                {
                    if (((ComboBox)s).SelectedIndex == 0)
                        settings.theme = "light";
                    else
                        settings.theme = "dark";
                };

                return new List<Model.SettingsPageItem>
                {
                    new Model.SettingsPageItem()
                    {
                        Header = "General",
                        Properties = new List<Model.SettingsPropertyItem>
                        {
                            new Model.SettingsPropertyItem()
                            {
                                Header = "Color theme",
                                Control = colorThemeComboBox
                            }
                        }
                    }
                };
            }
        }

        public List<Model.SettingsPropertyItem> Properties
        {
            get
            {
                return properties;
            }
            set
            {
                properties = value;
                OnPropertyChanged("Properties");
            }
        }

        public Model.SettingsPageItem SelectedPage { get; set; }
        
        public Model.ActionCommand PagesSelectionChanged
        {
            get
            {
                return new Model.ActionCommand(sender =>
                {
                    Properties = SelectedPage.Properties;
                });
            }
        }

        public Model.ActionCommand Save
        {
            get
            {
                return new Model.ActionCommand(sender =>
                {
                    settings.Save();
                });
            }
        }

        public void Closed(object sender, EventArgs e)
        {
            settings.Reload();
        }

        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
