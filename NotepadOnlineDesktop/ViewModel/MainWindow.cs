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

            public void Open(string name)
            {
                OpenFile(name);
            }

            public void OpenFolder(string path)
            {
                OpenDirectory(path);
            }

            public Func<string> GetText;
            public Action<string> SetText;
            public Func<string> GetName;
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
                    new View.SettingsWindow().ShowDialog();
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
            text.FontFamily = Properties.Settings.Default.fontFamily;
            text.FontSize = Properties.Settings.Default.fontSize;
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
                var instance = new Instance();
                instance.GetName = () => name;
                instance.GetText = () => text.Text;
                instance.SetText = value => text.Text = value;
                instance.OpenFile = name => Open(name);
                instance.OpenDirectory = path =>
                {
                    if (AskBeforeClear())
                    {
                        var name = GetNameToOpen(path);
                        if (name != null)
                            Open(name);
                    }
                };

                try
                {
                    Model.ExtensionManager.Load(@"C:\Projects\NotepadOnlineDesktop\CloudExtension\bin\Debug\");
                    //Model.ExtensionManager.Load(@"Extensions\");
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

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
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
            var findWindow = new View.FindWindow();
            findWindow.Owner = View.MainWindow.Instance;
            findWindow.ViewModel.RequestFind += (s, args) =>
            {
                int index;
                var comp = args.IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
                
                if (args.DownDirection)
                    index = text.Text.IndexOf(args.Word, text.CaretIndex + text.SelectionLength, comp);
                else
                    index = text.Text.LastIndexOf(args.Word, text.CaretIndex, comp);

                if (index == -1)
                {
                    MessageBox.Show("No matches", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                    text.Focus();
                    return;
                }

                text.CaretIndex = index + args.Word.Length;
                text.Select(index, args.Word.Length);
                text.Focus();
            };
            findWindow.Show();
        }

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var replaceWindow = new View.ReplaceWindow();
            replaceWindow.Owner = View.MainWindow.Instance;
            replaceWindow.ViewModel.RequestReplace += (s, args) =>
            {
                var comp = args.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

                text.Text = Regex.Replace(text.Text, args.OldWord, args.NewWord, comp);
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
                        Save(GetNameToSave());
                    return true;
                case MessageBoxResult.No:
                    return true;
                default:
                    return false;
            }
        }

        string GetNameToOpen(string defaultPath=null)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Text file (*.txt)|*.txt";
            dialog.InitialDirectory = defaultPath;

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        string GetNameToSave(string defaultPath=null)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Text file (*.txt)|*.txt";
            dialog.InitialDirectory = defaultPath;

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        void Open(string name)
        {
            text.Text = File.ReadAllText(name, Encoding.UTF8);
            Name = name;
            Saved = true;
        }

        void Save(string name)
        {
            File.WriteAllText(name, text.Text, Encoding.UTF8);
            Name = name;
            Saved = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
