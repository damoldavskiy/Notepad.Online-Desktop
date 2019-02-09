using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CloudExtension
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (password.Password != conf_password.Password)
            {
                MessageBox.Show("Passwords don't match");
                return;
            }

            var result = DataBase.Manager.Register(login.Text, password.Password);

            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show($"Registration failed: {result}", "Error");
                return;
            }

            MessageBox.Show("You'll receive confirmation code in 1-3 minutes. Type one in the box below", "Success");
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var result = DataBase.Manager.Confirm(code.Text);

            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show($"Confirmation failed: {result}", "Error");
                return;
            }

            MessageBox.Show("Registration completed", "Success");
            Close();
        }
    }
}
