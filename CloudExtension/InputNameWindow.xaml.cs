using System.ComponentModel;
using System.Windows;

namespace CloudExtension
{
    public partial class InputNameWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string text;

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public bool Canceled;

        public InputNameWindow()
        {
            InitializeComponent();
            DataContext = this;

            Text = "New file";
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        void Submit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
        }
    }
}
