using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CloudExtension
{
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            var properties = Properties.Settings.Default;
            login.Text = properties.login;
            password.Password = properties.password;
            path.Text = properties.path;
            
            accept.IsEnabled = false;
        }

        private void Authorize_Click(object sender, RoutedEventArgs e)
        {
            var log = login.Text.Trim();
            var pass = password.Password.Trim();

            var result = DataBase.Manager.Authorize(log, pass);
            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show("Error occured while authorizing: " + result, "Not authorized");
                return;
            }

            var properties = Properties.Settings.Default;
            properties.login = DataBase.Manager.Login;
            properties.password = DataBase.Manager.Password;
            properties.token = DataBase.Manager.Token;
            properties.Save();

            MessageBox.Show("Authorizing successful", "Success");
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            new RegistrationWindow().ShowDialog();
            Properties.Settings.Default.login = DataBase.Manager.Login;
            Properties.Settings.Default.password = DataBase.Manager.Password;
            Initialize();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            var dir = path.Text.Trim();
            if (!Directory.Exists(dir))
            {
                MessageBox.Show("Directory doesn't exist", "Error");
                return;
            }

            var properties = Properties.Settings.Default;
            properties.token = dir;
            properties.Save();
            accept.IsEnabled = false;
        }

        private void Local_Changed(object sender, RoutedEventArgs e)
        {
            if (path.Text.Length > 0)
                accept.IsEnabled = true;
        }
    }
}
