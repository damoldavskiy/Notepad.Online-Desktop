﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace NotepadOnlineDesktop.ViewModel
{
    public class SettingsWindow : INotifyPropertyChanged
    {
        List<Model.SettingsPageItem> pages;
        List<Model.SettingsPropertyItem> properties;
        Model.SettingsPageItem selectedPage;
        bool restartNotify;

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
                return pages;
            }
            set
            {
                pages = value;
                OnPropertyChanged(nameof(Pages));
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
                OnPropertyChanged(nameof(Properties));
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
                OnPropertyChanged(nameof(SelectedPage));
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
                    MessageBox.Show("Settings saved." + (restartNotify ? " Changes will be accepted after restart" : ""), "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        }

        public SettingsWindow()
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

            var fontSize = new ComboBox()
            {
                Width = 120,
                SelectedValue = settings.fontSize,
                ItemsSource = new[] { 10, 12, 14, 16, 18, 20 }
            };
            fontSize.SelectionChanged += (s, e) =>
            {
                var value = (int)((ComboBox)s).SelectedValue;
                if (value != settings.fontSize)
                {
                    restartNotify = true;
                    settings.fontSize = value;
                }
            };

            var fontFamily = new ComboBox()
            {
                Width = 120,
                SelectedValue = settings.fontFamily,
                ItemsSource = Fonts.SystemFontFamilies
            };
            fontFamily.SelectionChanged += (s, e) =>
            {
                var value = (FontFamily)((ComboBox)s).SelectedValue;
                if (value != settings.fontFamily)
                {
                    restartNotify = true;
                    settings.fontFamily = value;
                }
            };

            var enableExtensions = new CheckBox()
            {
                IsChecked = settings.enableExtensions
            };
            enableExtensions.Checked += (s, e) =>
            {
                restartNotify = true;
                settings.enableExtensions = true;
            };
            enableExtensions.Unchecked += (s, e) =>
            {
                restartNotify = true;
                settings.enableExtensions = false;
            };

            Pages = new List<Model.SettingsPageItem>
                {
                    new Model.SettingsPageItem()
                    {
                        Header = "General",
                        Properties = new List<Model.SettingsPropertyItem>()
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
                    },
                    new Model.SettingsPageItem()
                    {
                        Header = "Editor",
                        Properties = new List<Model.SettingsPropertyItem>()
                        {
                            new Model.SettingsPropertyItem()
                            {
                                Header = "Font size",
                                Control = fontSize
                            },
                            new Model.SettingsPropertyItem()
                            {
                                Header = "Font family",
                                Control = fontFamily
                            }
                        }
                    },
                    new Model.SettingsPageItem()
                    {
                        Header="Extensions",
                        Properties = new List<Model.SettingsPropertyItem>()
                        {
                            new Model.SettingsPropertyItem()
                            {
                                Header = "Enable extension manager",
                                Control = enableExtensions
                            }
                        }
                    }
                };
            
            SelectedPage = Pages[0];
            Properties = SelectedPage.Properties;
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