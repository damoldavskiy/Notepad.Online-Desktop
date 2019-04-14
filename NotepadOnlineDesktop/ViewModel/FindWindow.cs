using System.ComponentModel;

namespace NotepadOnlineDesktop.ViewModel
{
    public class FindWindow : INotifyPropertyChanged
    {
        public delegate void FindHandler(object sender, Model.FindEventArgs e);

        public event PropertyChangedEventHandler PropertyChanged;
        public event FindHandler RequestFind;

        string word;
        bool ignoreCase;
        bool regex;
        bool upDirection;
        bool downDirection;

        public string Word
        {
            get
            {
                return word;
            }
            set
            {
                word = value;
                OnPropertyChanged(nameof(Word));
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

        public bool UpDirection
        {
            get
            {
                return upDirection;
            }
            set
            {
                upDirection = value;
                OnPropertyChanged(nameof(UpDirection));
            }
        }

        public bool DownDirection
        {
            get
            {
                return downDirection;
            }
            set
            {
                downDirection = value;
                OnPropertyChanged(nameof(DownDirection));
            }
        }

        public Model.ActionCommand Find
        {
            get
            {
                return new Model.ActionCommand(sender =>
                    OnRequestFind(new Model.FindEventArgs(Word ?? "", IgnoreCase, Regex, UpDirection, DownDirection))
                );
            }
        }

        public FindWindow()
        {
            IgnoreCase = true;
            DownDirection = true;
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OnRequestFind(Model.FindEventArgs args)
        {
            RequestFind?.Invoke(this, args);
        }
    }
}
