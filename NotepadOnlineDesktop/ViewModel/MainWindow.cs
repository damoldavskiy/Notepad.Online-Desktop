using NotepadOnlineDesktopExtensions;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

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
            public Func<string> GetName;
            public Action<string> OpenFile;
            public Action<string> OpenDirectory;
        }

        string name;
        bool saved = true;
        bool textWrap;
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
                return textWrap;
            }
            set
            {
                textWrap = value;
                OnPropertyChanged("TextWrap");
                OnPropertyChanged("WrappingType");
            }
        }

        public TextWrapping WrappingType
        {
            get
            {
                return textWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
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
                    new CommandBinding(ApplicationCommands.Find, Find_Executed)
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
                    MessageBox.Show("Notepad.Online Desktop\nVersion: Alpha\n\nHave a question? Contact with developer: party_50@mail.ru", "About", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        }

        public MainWindow(TextBox text, MenuItem extensionsParent)
        {
            this.text = text;
            text.TextChanged += Text_TextChanged;

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
            
            var instance = new Instance();
            instance.GetName = () => name;
            instance.GetText = () => text.Text;
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

        public void Closing(object sender, CancelEventArgs e)
        {
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
                    return;
                }

                text.CaretIndex = index + args.Word.Length;
                text.Select(index, args.Word.Length);
                text.Focus();
            };
            findWindow.Show();
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
            using (var stream = new StreamReader(name, Encoding.Default))
            {
                text.Text = stream.ReadToEnd();
                Name = name;
                Saved = true;
            }
        }

        void Save(string name)
        {
            using (var stream = new StreamWriter(name, false, Encoding.Default))
            {
                stream.Write(text.Text);
                Name = name;
                Saved = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
