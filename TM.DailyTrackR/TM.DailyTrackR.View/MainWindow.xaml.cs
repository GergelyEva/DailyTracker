using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TM.DailyTrackR.DataType;
using TM.DailyTrackR.DataType.Enums;
using TM.DailyTrackR.Logic;

namespace TM.DailyTrackR.View
{
    public partial class MainWindow : Window
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=TRACKR_DATA;Integrated Security=true;";
        public ObservableCollection<TableInfo> Activities { get; set; }

        private bool isAdmin;
        private string username;
        private ActivityController activityController;
        private ExportController exportController;

        public string Username
        {
            get { return username; }
            set
            {
                if (username != value)
                {
                    username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public MainWindow(string username, bool isAdmin)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            this.isAdmin = isAdmin;
            this.username = username;
            Activities = new ObservableCollection<TableInfo>();
            DataContext = this;

            activityController = new ActivityController(connectionString, username, isAdmin);
            exportController = new ExportController(connectionString);

            calendar.SelectedDatesChanged += CalendarSelectedDatesChanged;

            if (calendar.SelectedDate.HasValue)
            {
                LoadActivitiesForSelectedDate();
            }
        }

        private void CalendarSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (calendar.SelectedDate.HasValue)
            {
                LoadActivitiesForSelectedDate();
            }
        }

        private void LoadActivitiesForSelectedDate()
        {
            Activities.Clear();
            activityController.LoadActivities(calendar.SelectedDate.Value, Activities);
            dailyDataGrid.ItemsSource = Activities;
            dailyDataGrid.Items.Refresh();
            overviewDataGrid.ItemsSource = Activities;
            overviewDataGrid.Items.Refresh();
            UserColumnVisibility();
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            NewActivityWindow newActivityWindow;

            if (isAdmin)
            {
                newActivityWindow = new NewActivityWindow("admin");
            }
            else
            {
                newActivityWindow = new NewActivityWindow(username);
            }

            if (newActivityWindow.ShowDialog() == true)
            {
                if (calendar.SelectedDate.HasValue)
                {
                    LoadActivitiesForSelectedDate();
                }
            }
        }

        private void DeleteMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (dailyDataGrid.SelectedItem is TableInfo selectedActivity)
            {
                var result = MessageBox.Show("Are you sure you want to delete this activity?", "Delete Activity", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    activityController.DeleteActivity(selectedActivity);
                    LoadActivitiesForSelectedDate();
                }
            }
        }

        private void DailyDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                TableInfo editedActivity = e.Row.Item as TableInfo;

                if (activityController.ValidateActivity(editedActivity))
                {
                    activityController.UpdateActivity(editedActivity);
                    LoadActivitiesForSelectedDate();
                }
                else
                {
                    MessageBox.Show("Invalid input. Please ensure all fields contain valid values.");
                }
            }
        }

        private void DailyDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dailyDataGrid.SelectedItem is TableInfo selectedActivity)
            {
                if (activityController.ValidateActivity(selectedActivity))
                {
                    activityController.UpdateActivity(selectedActivity);
                    LoadActivitiesForSelectedDate();
                }
                else
                {
                    MessageBox.Show("Invalid input. Please ensure all fields contain valid values.");
                }
            }
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = startDatePicker.SelectedDate;
            DateTime? endDate = endDatePicker.SelectedDate;

            if (startDate.HasValue && endDate.HasValue)
            {
                exportController.ExportActivities(startDate.Value, endDate.Value, "Excel");
            }
            else
            {
                MessageBox.Show("Please select a valid date range.");
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = startDatePicker.SelectedDate;
            DateTime? endDate = endDatePicker.SelectedDate;

            if (startDate.HasValue && endDate.HasValue)
            {
                exportController.ExportActivities(startDate.Value, endDate.Value, "CSV");
            }
            else
            {
                MessageBox.Show("Please select a valid date range.");
            }
        }

        private void UserColumnVisibility()
        {
            if (isAdmin)
            {
                dailyDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "User")?.SetValue(DataGridColumn.VisibilityProperty, Visibility.Visible);
                overviewDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "User")?.SetValue(DataGridColumn.VisibilityProperty, Visibility.Visible);
            }
            else
            {
                dailyDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "User")?.SetValue(DataGridColumn.VisibilityProperty, Visibility.Collapsed);
                overviewDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "User")?.SetValue(DataGridColumn.VisibilityProperty, Visibility.Collapsed);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
