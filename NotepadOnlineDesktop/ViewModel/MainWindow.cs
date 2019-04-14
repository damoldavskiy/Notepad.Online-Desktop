using Microsoft.Win32;

using NotepadOnlineDesktopExtensions;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NotepadOnlineDesktop.ViewModel
{
    class MainWindow : INotifyPropertyChanged
    {
        class Instance : IApplicationInstance
        {
            public event InputHandler OnInput;

            public string Text
            {
                get
                {
                    return GetText();
                }
                set
                {
                    SetText(value);
                }
            }

            public string Name
            {
                get
                {
                    return GetName();
                }
            }

            public int SelectionStart
            {
                get
                {
                    return GetSelectionStart();
                }
                set
                {
                    SetSelectionStart(value);
                }
            }

            public int SelectionLength
            {
                get
                {
                    return GetSelectionLength();
                }
                set
                {
                    SetSelectionLength(value);
                }
            }

            public void Open(string name)
            {
                OpenFile(name);
            }

            public void OpenFolder(string path)
            {
                OpenDirectory(path);
            }

            public bool RaiseOnInput(char key)
            {
                var args = new NotepadOnlineDesktopExtensions.InputEventArgs(key);
                OnInput?.Invoke(this, args);
                return args.Handled;
            }

            public bool RaiseOnInput(SpecKey specKey)
            {
                var args = new NotepadOnlineDesktopExtensions.InputEventArgs(specKey);
                OnInput?.Invoke(this, args);
                return args.Handled;
            }

            public Func<string> GetText;
            public Action<string> SetText;
            public Func<string> GetName;
            public Func<int> GetSelectionStart;
            public Action<int> SetSelectionStart;
            public Func<int> GetSelectionLength;
            public Action<int> SetSelectionLength;
            public Action<string> OpenFile;
            public Action<string> OpenDirectory;
        }

        string name;
        bool saved = true;
        TextBox text;

        string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("WindowTitle");
            }
        }

        bool Saved
        {
            get
            {
                return saved;
            }
            set
            {
                saved = value;
                OnPropertyChanged("WindowTitle");
            }
        }

        public string WindowTitle
        {
            get
            {
                return "Notepad.Online" + (Saved ? "" : " (Edited)") + (Name == null ? "" : " | " + Name);
            }
        }

        public bool TextWrap
        {
            get
            {
                return Properties.Settings.Default.textwrap;
            }
            set
            {
                Properties.Settings.Default.textwrap = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged("TextWrap");
                OnPropertyChanged("WrappingType");
            }
        }

        public TextWrapping WrappingType
        {
            get
            {
                return Properties.Settings.Default.textwrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
        }

        public Action Close { get; set; }

        public List<CommandBinding> Bindings
        {
            get
            {
                return new List<CommandBinding>()
                {
                    new CommandBinding(ApplicationCommands.New, New_Executed),
                    new CommandBinding(ApplicationCommands.Open, Open_Executed),
                    new CommandBinding(ApplicationCommands.Save, Save_Executed, Save_CanExecute),
                    new CommandBinding(ApplicationCommands.SaveAs, SaveAs_Executed),
                    new CommandBinding(ApplicationCommands.Close, Exit_Executed),
                    new CommandBinding(ApplicationCommands.Find, Find_Executed),
                    new CommandBinding(ApplicationCommands.Replace, Replace_Executed)
                };
            }
        }

        public Model.ActionCommand Settings
        {
            get
            {
                return new Model.ActionCommand(sender =>
                {
                    var window = new View.SettingsWindow();
                    window.ViewModel.SettingsUpdated += (s, e) =>
                    {
                        Model.ThemeManager.Update();
                        UpdateMainTextBox();
                    };
                    window.ShowDialog();
                });
            }
        }

        public Model.ActionCommand Manager
        {
            get
            {
                return new Model.ActionCommand(sender =>
                {
                    new View.ManagerWindow().ShowDialog();
                });
            }
        }

        public Model.ActionCommand About
        {
            get
            {
                return new Model.ActionCommand(sender =>
                {
                    MessageBox.Show("Notepad.Online Desktop\nVersion: 1.0\n\nHave a question? Contact with developer: party_50@mail.ru", "About", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        }

        public MainWindow(TextBox text, MenuItem extensionsParent)
        {
            if (Properties.Settings.Default.fontFamily is null)
                Properties.Settings.Default.fontFamily = Fonts.SystemFontFamilies.Where(font => font.ToString() == "Consolas").First();
            
            this.text = text;
            UpdateMainTextBox();
            text.TextChanged += Text_TextChanged;

            TextWrap = Properties.Settings.Default.textwrap;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                try
                {
                    Open(args[1]);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Unable to load file: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            if (Properties.Settings.Default.enableExtensions)
            {
                var instance = new Instance
                {
                    GetName = () => name,
                    GetText = () => text.Text,
                    SetText = value => text.Text = value,
                    GetSelectionStart = () => text.SelectionStart,
                    SetSelectionStart = (value) => text.SelectionStart = value,
                    GetSelectionLength = () => text.SelectionLength,
                    SetSelectionLength = (value) => text.SelectionLength = value,
                    OpenFile = name => Open(name),
                    OpenDirectory = path =>
                    {
                        if (AskBeforeClear())
                        {
                            var name = GetNameToOpen(path);
                            if (name != null)
                                Open(name);
                        }
                    }
                };
                text.PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == Key.Tab)
                        e.Handled = instance.RaiseOnInput('\t');
                    if (e.Key == Key.Space)
                        e.Handled = instance.RaiseOnInput(' ');
                    if (e.Key == Key.Enter)
                        e.Handled = instance.RaiseOnInput('\r');
                    if (e.Key == Key.Back)
                        e.Handled = instance.RaiseOnInput(SpecKey.Backspace);
                    if (e.Key == Key.Escape)
                        e.Handled = instance.RaiseOnInput(SpecKey.Escape);
                    if (e.Key == Key.Delete)
                        e.Handled = instance.RaiseOnInput(SpecKey.Delete);
                };
                text.PreviewTextInput += (s, e) =>
                {
                    if (e.Text.Length > 0)
                        e.Handled = instance.RaiseOnInput(e.Text[0]);
                };

                try
                {
                    #if DEBUG
                    Model.ExtensionManager.Load(@"C:\Projects\NotepadOnlineDesktop\SnippetsExtension\bin\Debug\");
                    #else
                    Model.ExtensionManager.Load(@"Extensions\");
                    #endif
                    Model.ExtensionManager.Initialize(instance, extensionsParent);
                }
                catch (DirectoryNotFoundException)
                { }
            }
            else
                extensionsParent.Visibility = Visibility.Collapsed;
        }

        public void Closing(object sender, CancelEventArgs e)
        {
            if (Properties.Settings.Default.askonexit)
                e.Cancel = !AskBeforeClear();
        }

        void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            Saved = false;
        }

        void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (AskBeforeClear())
            {
                text.Text = "";
                Name = null;
                Saved = true;
            }
        }

        void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (AskBeforeClear())
            {
                var name = GetNameToOpen();
                if (name != null)
                    Open(name);
            }
        }

        void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save(Name);
        }

        void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Name != null && !Saved;
        }

        void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var name = GetNameToSave();
            if (name != null)
                Save(name);
        }

        void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var findWindow = new View.FindWindow
            {
                Owner = View.MainWindow.Instance
            };
            findWindow.ViewModel.RequestFind += (s, args) =>
            {
                string block;

                if (args.DownDirection)
                    block = text.Text.Substring(text.SelectionStart + text.SelectionLength);
                else
                    block = text.Text.Substring(0, text.SelectionStart);
                
                var word = args.Regex ? args.Word : Regex.Escape(args.Word);

                var options = RegexOptions.Multiline;

                if (args.IgnoreCase)
                    options |= RegexOptions.IgnoreCase;
                if (args.UpDirection)
                    options |= RegexOptions.RightToLeft;

                var match = Regex.Match(block, word, options);

                if (!match.Success)
                {
                    MessageBox.Show("No matches", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                    text.Focus();
                    return;
                }

                var index = match.Index;
                if (args.DownDirection)
                    index += text.SelectionStart + text.SelectionLength;
                
                text.Focus();
                text.Select(index, match.Length);
            };
            findWindow.Show();
        }

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var replaceWindow = new View.ReplaceWindow
            {
                Owner = View.MainWindow.Instance
            };
            replaceWindow.ViewModel.RequestReplace += (s, args) =>
            {
                var comp = args.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

                text.Text = Regex.Replace(text.Text, args.Regex ? args.OldWord : Regex.Escape(args.OldWord), args.NewWord, comp);
                text.Focus();
            };
            replaceWindow.Show();
        }

        bool AskBeforeClear()
        {
            if (Saved)
                return true;

            var result = MessageBox.Show("Do you want to save the text?", "Closing current text", MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    if (Name != null)
                        Save(Name);
                    else
                    {
                        var name = GetNameToSave();
                        if (name != null)
                            Save(name);
                    }
                    return true;
                case MessageBoxResult.No:
                    return true;
                default:
                    return false;
            }
        }

        string GetNameToOpen(string defaultPath=null)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
                InitialDirectory = defaultPath
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        string GetNameToSave(string defaultPath=null)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
                InitialDirectory = defaultPath
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        void Open(string name)
        {
            text.Text = File.ReadAllText(name, Encoding.Default);
            Name = name;
            Saved = true;
        }

        void Save(string name)
        {
            File.WriteAllText(name, text.Text, Encoding.UTF8);
            Name = name;
            Saved = true;
        }

        void UpdateMainTextBox()
        {
            text.FontFamily = Properties.Settings.Default.fontFamily;
            text.FontSize = Properties.Settings.Default.fontSize;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
