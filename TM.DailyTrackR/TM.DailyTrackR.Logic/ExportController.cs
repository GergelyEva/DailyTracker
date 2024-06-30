using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using TM.DailyTrackR.DataType;
using TM.DailyTrackR.DataType.Enums;

namespace TM.DailyTrackR.Logic
{
    public class ExportController
    {
        private readonly string connectionString;

        public ExportController(string connectionString)
        {
            this.connectionString = connectionString;
        }
        //gets the data for the selected range from the database
        public void ExportActivities(DateTime startDate, DateTime endDate, string fileType)
        {
            string procedureExportActivities = "tm.ExportActivities";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(procedureExportActivities, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        ObservableCollection<TableInfo> activitiesToExport = new ObservableCollection<TableInfo>();

                        while (reader.Read())
                        {
                            TableInfo data = new TableInfo
                            {
                                No = reader.GetInt32(reader.GetOrdinal("id")),
                                ProjectType = (ProjectType)reader.GetInt32(reader.GetOrdinal("project_type_id")),
                                TaskType = (TaskType)reader.GetInt32(reader.GetOrdinal("activity_type_id")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Status = (Status)reader.GetInt32(reader.GetOrdinal("status_id")),
                                User = reader.GetString(reader.GetOrdinal("username")),
                            };

                            activitiesToExport.Add(data);
                        }
                        //based which button was pressed, it calls the appropriate method
                        if (fileType == "CSV")
                        {
                            ExportToCsv(activitiesToExport, startDate, endDate);
                        }
                        else if (fileType == "Excel")
                        {
                            ExportToExcel(activitiesToExport, startDate, endDate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while exporting activities: " + ex.Message);
            }
        }

        private void ExportToCsv(ObservableCollection<TableInfo> activities, DateTime startDate, DateTime endDate)
        {
            try
            {
                string startDateString = startDate.ToString("dd.MM.yyyy");
                string endDateString = endDate.ToString("dd.MM.yyyy");

                string fileName = $"TeamWeekActivity_{startDateString}_{endDateString}.csv";
                string filePath = Path.Combine("C:/Users/HP/Desktop/TrackerFinal", fileName);

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"Team Activity in the period {startDateString} – {endDateString}");
                    writer.WriteLine();
                    //using GroupBy from linq
                    var activitiesByGroup = activities.GroupBy(a => a.ProjectType).ToList();

                    foreach (var groupOfActivity in activitiesByGroup)
                    {
                        writer.WriteLine(groupOfActivity.Key.ToString());
                        foreach (var activity in groupOfActivity)
                        {
                            writer.WriteLine($"{activity.Description},{activity.Status}");
                        }

                        writer.WriteLine();
                    }
                }

                MessageBox.Show($"TeamWeekActivity {startDateString} – {endDateString} successfully downloaded");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while exporting activities to CSV: " + ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel(ObservableCollection<TableInfo> activities, DateTime startDate, DateTime endDate)
        {
            try
            {
                string startDateString = startDate.ToString("dd.MM.yyyy");
                string endDateString = endDate.ToString("dd.MM.yyyy");

                string fileName = $"TeamWeekActivity_{startDateString}_{endDateString}.xlsx";
                string filePath = Path.Combine("C:/Users/HP/Desktop/TrackerFinal", fileName);

                //create a new excel
                using (var workbook = new XLWorkbook())
                {//creates a sheet in that excell, called Activities
                    var worksheet = workbook.Worksheets.Add("Activities");

                    //setting the title, it's place and formatting 
                    worksheet.Cell(1, 1).Value = $"Team Activity in the period {startDateString} – {endDateString}";
                    worksheet.Range(1, 1, 1, 2).Merge().Style.Font.SetBold().Font.SetFontSize(12);

                    int currentRow = 3;

                    var groupedActivities = activities.GroupBy(a => a.ProjectType).ToList();
                    //for the name of the group
                    foreach (var group in groupedActivities)
                    {  
                        worksheet.Cell(currentRow, 1).Value = group.Key.ToString();
                        worksheet.Cell(currentRow, 1).Style.Font.SetBold().Font.SetFontSize(12);
                        currentRow++;
                        //for every activity in the group
                        foreach (var activity in group)
                        {
                            worksheet.Cell(currentRow, 1).Value = activity.Description;
                            worksheet.Cell(currentRow, 2).Value = activity.Status.ToString();
                            currentRow++;
                        }

                        currentRow++;
                    }

                    workbook.SaveAs(filePath);
                }

                MessageBox.Show($"TeamWeekActivity_{startDateString} – {endDateString}.xlsx successfully exported");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while exporting activities to Excel: " + ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
