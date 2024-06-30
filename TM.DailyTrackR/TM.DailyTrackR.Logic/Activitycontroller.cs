using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows;
using TM.DailyTrackR.DataType;
using TM.DailyTrackR.DataType.Enums;

namespace TM.DailyTrackR.Logic
{
    public class ActivityController
    {
        private readonly string connectionString;
        private readonly string username;
        private readonly bool isAdmin;

        public ActivityController(string connectionString, string username, bool isAdmin)
        {
            this.connectionString = connectionString;
            this.username = username;
            this.isAdmin = isAdmin;
        }

        public void LoadActivities(DateTime date, ObservableCollection<TableInfo> activities)
        {
            activities.Clear();

            if (isAdmin)
            {
                LoadAdminActivities(date, activities);
            }
            else
            {
                LoadUserActivities(date, activities);
            }
        }

        private void LoadAdminActivities(DateTime date, ObservableCollection<TableInfo> activities)
        {
            string storedProcedure = "tm.GetActivitiesByDate";
            LoadActivitiesFromDatabase(date, activities, storedProcedure, true);
        }

        private void LoadUserActivities(DateTime date, ObservableCollection<TableInfo> activities)
        {
            string storedProcedure = "tm.GetActivitiesForUserBySpecificDate";
            LoadActivitiesFromDatabase(date, activities, storedProcedure, false);
        }

        private void LoadActivitiesFromDatabase(DateTime date, ObservableCollection<TableInfo> activities, string storedProcedure, bool isAdmin)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                    {
                        command.Parameters.AddWithValue("@SpecificDate", date.ToString("yyyy-MM-dd"));
                        if (!isAdmin)
                        {
                            command.Parameters.AddWithValue("@Username", username);
                        }
                        command.CommandType = CommandType.StoredProcedure;

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        int counter = 1;
                        while (reader.Read())
                        {
                            TableInfo data = new TableInfo
                            {
                                DatabaseId = reader.GetInt32(reader.GetOrdinal("id")),
                                No = counter++,
                                ProjectType = (ProjectType)reader.GetInt32(reader.GetOrdinal("project_type_id")),
                                TaskType = (TaskType)reader.GetInt32(reader.GetOrdinal("activity_type_id")),
                                Description = reader.GetString(reader.GetOrdinal("activity_description")),
                                Status = (Status)reader.GetInt32(reader.GetOrdinal("status_id")),
                                User = isAdmin ? reader.GetString(reader.GetOrdinal("username")) : username
                            };

                            activities.Add(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading activities: " + ex.Message);
            }
        }
        public void UpdateActivity(TableInfo activity)
        {
            string updateProcedure = "tm.UpdateActivity";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(updateProcedure, connection))
                    {
                        command.Parameters.AddWithValue("@No", activity.DatabaseId);
                        MessageBox.Show("Entry with the following id was updated: " + activity.DatabaseId);

                        command.Parameters.AddWithValue("@ProjectType", (int)activity.ProjectType);
                        command.Parameters.AddWithValue("@TaskType", (int)activity.TaskType);
                        command.Parameters.AddWithValue("@Description", activity.Description);
                        command.Parameters.AddWithValue("@Status", (int)activity.Status);
                        command.CommandType = CommandType.StoredProcedure;

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while updating the activity: " + ex.Message);
            }
        }

        public void DeleteActivity(TableInfo activity)
        {
            string deleteProcedure = "tm.DeleteEntry";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(deleteProcedure, connection))
                    {
                        command.Parameters.AddWithValue("@entry", activity.DatabaseId);
                        MessageBox.Show("Entry with the following id was deleted: " + activity.DatabaseId);

                        command.CommandType = CommandType.StoredProcedure;

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while deleting the activity: " + ex.Message);
            }
        }

        public bool ValidateActivity(TableInfo activity)
        {
            return !string.IsNullOrWhiteSpace(activity.Description) &&
                   Enum.IsDefined(typeof(ProjectType), activity.ProjectType) &&
                   Enum.IsDefined(typeof(TaskType), activity.TaskType) &&
                   Enum.IsDefined(typeof(Status), activity.Status);
        }
    }
}
