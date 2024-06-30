using System;
using System.Windows;
using TM.DailyTrackR.Logic;

namespace TM.DailyTrackR.View
{
    public partial class LoginWindow : Window
    {
        private LoginController loginController;

        public LoginWindow()
        {
            InitializeComponent();
            string connectionString = @"Server=.\SQLEXPRESS;Database=TRACKR_DATA;Integrated Security=true;";
            loginController = new LoginController(connectionString);
            // shows the messagebox asking if the user is an admin or not
            loginController.AdminCheck();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;
            // if the authentication is successful, shows the user the main window
            if (loginController.AuthenticateUser(username, password))
            {
                SwitchToMainWindow(username);
            }
            else
            {
                MessageBox.Show("Login failed. Please check your credentials.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwitchToMainWindow(string username)
        {
            MainWindow mainWindow = new MainWindow(username, loginController.IsAdmin);
            mainWindow.Show();

            Application.Current.MainWindow.Close();
        }
    }
}
