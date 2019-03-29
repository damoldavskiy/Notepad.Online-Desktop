using System.Windows;

namespace CloudExtension
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        void Register_Click(object sender, RoutedEventArgs e)
        {
            if (password.Password != conf_password.Password)
            {
                MessageBox.Show("Passwords don't match");
                return;
            }

            var result = DataBase.Manager.Register(email.Text, password.Password);

            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show($"Registration failed: {result}", "Error");
                return;
            }

            MessageBox.Show("You'll receive confirmation code in 1-3 minutes. Type one in the box below", "Success");
        }

        void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (DataBase.Manager.Status != DataBase.ManagerStatus.RegistrationConfirmation)
            {
                MessageBox.Show("Perform registration firstly");
                return;
            }

            var result = DataBase.Manager.ConfirmRegistration(code.Text);

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
