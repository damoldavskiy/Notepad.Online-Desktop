using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotepadOnlineDesktop.ViewModel
{
    public class FindWindow : INotifyPropertyChanged
    {
        public delegate void FindHandler(object sender, Model.FindEventArgs e);

        public event PropertyChangedEventHandler PropertyChanged;
        public event FindHandler RequestFind;

        private string word;
        private bool ignoreCase;
        private bool upDirection;
        private bool downDirection;

        public string Word
        {
            get
            {
                return word;
            }
            set
            {
                word = value;
                OnPropertyChanged("Word");
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

        public bool UpDirection
        {
            get
            {
                return upDirection;
            }
            set
            {
                upDirection = value;
                OnPropertyChanged("UpDirection");
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
                OnPropertyChanged("DownDirection");
            }
        }

        public Model.ActionCommand Find
        {
            get
            {
                return new Model.ActionCommand(sender =>
                    OnRequestFind(new Model.FindEventArgs(Word ?? "", IgnoreCase, UpDirection, DownDirection))
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
