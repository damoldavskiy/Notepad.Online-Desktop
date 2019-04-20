using System.IO;
using System.Windows;

using static DataBase.ReturnCodeDescriptions;

namespace CloudExtension
{
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow()
        {
            InitializeComponent();
            Initialize();
        }

        void Initialize()
        {
            var properties = Properties.Settings.Default;
            email.Text = properties.email;
            password.Password = properties.password;
            path.Text = properties.path;
            
            accept.IsEnabled = false;
        }

        async void Login_Click(object sender, RoutedEventArgs e)
        {
            var log = email.Text.Trim();
            var pass = password.Password.Trim();

            var result = await DataBase.Manager.LoginAsync(log, pass);
            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show(Properties.Resources.LoginError + ". " + result.GetDescription(), Properties.Resources.NotSigned);
                return;
            }

            var properties = Properties.Settings.Default;
            properties.email = DataBase.Manager.Email;
            properties.password = DataBase.Manager.Password;
            properties.token = DataBase.Manager.Token;
            properties.Save();

            MessageBox.Show(Properties.Resources.YouAreSigned, Properties.Resources.Success);
        }

        void Register_Click(object sender, RoutedEventArgs e)
        {
            new RegistrationWindow().ShowDialog();
            Properties.Settings.Default.email = DataBase.Manager.Email;
            Properties.Settings.Default.password = DataBase.Manager.Password;
            Initialize();
        }

        void Accept_Click(object sender, RoutedEventArgs e)
        {
            var info = new DirectoryInfo(path.Text.Trim() + "\\");
            if (!info.Exists)
            {
                MessageBox.Show(Properties.Resources.DirectoryDoesntExist, Properties.Resources.Error);
                return;
            }
            
            var properties = Properties.Settings.Default;
            properties.path = info.FullName;
            properties.Save();
            accept.IsEnabled = false;
        }

        void Local_Changed(object sender, RoutedEventArgs e)
        {
            if (path.Text.Length > 0)
                accept.IsEnabled = true;
        }
    }
}
