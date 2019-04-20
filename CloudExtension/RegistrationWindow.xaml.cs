using System.Windows;

using static DataBase.ReturnCodeDescriptions;

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
                MessageBox.Show(Properties.Resources.PasswordsDontMatch);
                return;
            }

            var result = DataBase.Manager.Register(email.Text, password.Password);

            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show(Properties.Resources.RegistrationFailed + ". " + result.GetDescription(), Properties.Resources.Error);
                return;
            }

            MessageBox.Show(Properties.Resources.CodeMessage, Properties.Resources.Success);
        }

        void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (DataBase.Manager.Status != DataBase.ManagerStatus.RegistrationConfirmation)
            {
                MessageBox.Show(Properties.Resources.PerformRegistration);
                return;
            }

            var result = DataBase.Manager.ConfirmRegistration(code.Text);

            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show(Properties.Resources.ConfirmationFailed + ". " + result.GetDescription(), Properties.Resources.Error);
                return;
            }

            MessageBox.Show(Properties.Resources.RegistrationCompleted, Properties.Resources.Success);
            Close();
        }
    }
}
