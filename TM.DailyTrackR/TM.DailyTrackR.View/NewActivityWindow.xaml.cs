using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace TM.DailyTrackR.View
{
    public partial class NewActivityWindow : Window
    {
        private string currentUser;
        private string connectionString = @"Server=localhost\SQLEXPRESS;Database=TRACKR_DATA;Integrated Security=true;";

        public NewActivityWindow(string user)
        {
            InitializeComponent();
            currentUser = user;
            //OnInputChanged w SelectionChanged/TextChanged= to check if any input has been made to those fields
            //to enable/disable the Save Button
            projectTypeComboBox.SelectionChanged += OnInputChanged;
            taskTypeComboBox.SelectionChanged += OnInputChanged;
            statusComboBox.SelectionChanged += OnInputChanged;
            descriptionTextBox.TextChanged += OnInputChanged;
            datePicker.SelectedDateChanged += OnInputChanged;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string procedureInsertActivity = "tm.InsertActivity";

            if (projectTypeComboBox.SelectedItem != null &&
                taskTypeComboBox.SelectedItem != null &&
                statusComboBox.SelectedItem != null)
            {
                int projectType = Convert.ToInt32(((ComboBoxItem)projectTypeComboBox.SelectedItem)?.Tag);
                int taskType = Convert.ToInt32(((ComboBoxItem)taskTypeComboBox.SelectedItem)?.Tag);
                string description = descriptionTextBox.Text;
                int status = Convert.ToInt32(((ComboBoxItem)statusComboBox.SelectedItem)?.Tag);
                DateTime? date = datePicker.SelectedDate;

                if (date == null)
                {
                    MessageBox.Show("Please select a date.");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (SqlCommand cmd = new SqlCommand(procedureInsertActivity, connection))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.AddWithValue("@ProjectType", projectType);
                                cmd.Parameters.AddWithValue("@TaskType", taskType);
                                cmd.Parameters.AddWithValue("@Description", description);
                                cmd.Parameters.AddWithValue("@Status", status);
                                cmd.Parameters.AddWithValue("@Username", currentUser);
                                cmd.Parameters.AddWithValue("@Date", date);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Activity saved successfully.");
                        this.DialogResult = true;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while saving the activity: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please fill in all fields.");
                }
            }
            else
            {
                MessageBox.Show("Please select values for all fields.");
            }
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            bool isAnyFieldFilled = datePicker.SelectedDate != null ||
                                    projectTypeComboBox.SelectedItem != null ||
                                    taskTypeComboBox.SelectedItem != null ||
                                    statusComboBox.SelectedItem != null ||
                                    !string.IsNullOrWhiteSpace(descriptionTextBox.Text);

            saveButton.IsEnabled = isAnyFieldFilled;
        }
    }
}
