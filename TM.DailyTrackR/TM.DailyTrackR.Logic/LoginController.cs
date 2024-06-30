using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace TM.DailyTrackR.Logic
{
    public class LoginController
    {
        private readonly string connectionString;
        private const int maxNrLoginAttempts = 3;
        private int loginCounter = 0;
        private bool isAdmin;

        public LoginController(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool IsAdmin => isAdmin;

        public void AdminCheck()
        {
            var result = MessageBox.Show("Are you an admin?", "Admin/User Authentication", MessageBoxButton.YesNo, MessageBoxImage.Question);
            isAdmin = result == MessageBoxResult.Yes;
        }

        public bool AuthenticateUser(string username, string password)
        {
            if (EmptyField(username, password))
            {
                MessageBox.Show("Username and password can't be empty.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            bool isAuthenticated = false;

            // If isAdmin is true, attempt admin login
            if (isAdmin)
            {
                isAuthenticated = AdminLogin(username, password);
            }
            else
            {
                // if the username is "admin", also attempt admin login
                if (username == "admin")
                {   //
                    AdminCheck();
                }
                else
                {
                    // Otherwise, attempt user login
                    isAuthenticated = UserExists(username, password);
                }
            }
           //in case that the authentication fails
            if (!isAuthenticated)
            {
                loginCounter++;

                if (loginCounter >= maxNrLoginAttempts)
                {
                    MessageBox.Show("Please contact tech support :)", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Username/Password is incorrect. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            return isAuthenticated;
        }
        //checks for empty fields
        private bool EmptyField(string username, string password)
        {
            return string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password);
        }

        //hardcoded username and password
        private bool AdminLogin(string username, string password)
        {
            return username == "admin" && password == "admin";
        }
        //function to check if the user exists in the database
        private bool UserExists(string username, string password)
        {
            string procedureFindUser = "tm.FindUser";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    using (SqlCommand command = new SqlCommand(procedureFindUser, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@username", username);

                        connection.Open();
                        var result = command.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int userExists))
                        {
                            return userExists > 0;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while finding user: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }
    }
}
