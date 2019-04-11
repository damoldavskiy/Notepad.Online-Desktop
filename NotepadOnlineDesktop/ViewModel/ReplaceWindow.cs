using System.ComponentModel;

namespace NotepadOnlineDesktop.ViewModel
{
    public class ReplaceWindow : INotifyPropertyChanged
    {
        public delegate void ReplaceHandler(object sender, Model.ReplaceEventArgs e);

        public event PropertyChangedEventHandler PropertyChanged;
        public event ReplaceHandler RequestReplace;

        string oldWord;
        string newWord;
        bool ignoreCase;
        bool regex;


        public string OldWord
        {
            get
            {
                return oldWord;
            }
            set
            {
                oldWord = value;
                OnPropertyChanged(nameof(OldWord));
            }
        }

        public string NewWord
        {
            get
            {
                return newWord;
            }
            set
            {
                newWord = value;
                OnPropertyChanged(nameof(NewWord));
            }
        }

        public bool IgnoreCase
        {
            get
            {
                return ignoreCase;
            }
            set
            {
                ignoreCase = value;
                OnPropertyChanged(nameof(IgnoreCase));
            }
        }

        public bool Regex
        {
            get
            {
                return regex;
            }
            set
            {
                regex = value;
                OnPropertyChanged(nameof(Regex));
            }
        }

        public Model.ActionCommand Replace
        {
            get
            {
                return new Model.ActionCommand(
                sender =>
                {
                    if (!string.IsNullOrEmpty(OldWord))
                        OnRequestReplace(new Model.ReplaceEventArgs(OldWord, NewWord ?? "", IgnoreCase, Regex));
                });
            }
        }

        public ReplaceWindow()
        {
            IgnoreCase = true;
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OnRequestReplace(Model.ReplaceEventArgs args)
        {
            RequestReplace?.Invoke(this, args);
        }
    }
}
