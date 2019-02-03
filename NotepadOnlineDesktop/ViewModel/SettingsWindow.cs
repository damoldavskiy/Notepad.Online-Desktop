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
        Model.SettingsPageItem selectedPage;

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

                var askSaveCheckBox = new CheckBox()
                {
                    IsChecked = settings.askonexit
                };
                askSaveCheckBox.Checked += (s, e) => settings.askonexit = true;
                askSaveCheckBox.Unchecked += (s, e) => settings.askonexit = false;

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
                            },
                            new Model.SettingsPropertyItem()
                            {
                                Header = "Ask on exit",
                                Control = askSaveCheckBox
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

        public Model.SettingsPageItem SelectedPage
        {
            get
            {
                return selectedPage;
            }
            set
            {
                selectedPage = value;
                OnPropertyChanged("SelectedPage");
            }
        }

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
                    Model.ThemeManager.Update();
                });
            }
        }

        public void Closed(object sender, EventArgs e)
        {
            settings.Reload();
            Model.ThemeManager.Update();
        }

        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
