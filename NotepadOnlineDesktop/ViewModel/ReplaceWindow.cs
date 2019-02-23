using System.ComponentModel;

namespace NotepadOnlineDesktop.ViewModel
{
    public class ReplaceWindow : INotifyPropertyChanged
    {
        public delegate void ReplaceHandler(object sender, Model.ReplaceEventArgs e);

        public event PropertyChangedEventHandler PropertyChanged;
        public event ReplaceHandler RequestReplace;

        private string oldWord;
        private string newWord;
        private bool ignoreCase;

        public string OldWord
        {
            get
            {
                return oldWord;
            }
            set
            {
                oldWord = value;
                OnPropertyChanged("OldWord");
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
                OnPropertyChanged("NewWord");
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
                OnPropertyChanged("IgnoreCase");
            }
        }

        public Model.ActionCommand Replace
        {
            get
            {
                return new Model.ActionCommand(
                sender =>
                {
                    if (!string.IsNullOrEmpty(OldWord) && !string.IsNullOrEmpty(NewWord))
                        OnRequestReplace(new Model.ReplaceEventArgs(OldWord, NewWord, IgnoreCase));
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
