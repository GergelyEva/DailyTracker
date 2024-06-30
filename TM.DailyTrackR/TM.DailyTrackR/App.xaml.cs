using System;
using System.Windows;
using TM.DailyTrackR.Common;
using TM.DailyTrackR.View;
using TM.DailyTrackR.ViewModel;

namespace TM.DailyTrackR
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Register the Viewmodel and the corresponding window
            ViewService.Instance.RegisterView(typeof(LoginWindowViewModel), typeof(LoginWindow));
            ViewService.Instance.RegisterView(typeof(MainWindowViewModel), typeof(MainWindow));
            
            //instantiante de loginviewmodel, and show the login window first.
            LoginWindowViewModel loginViewModel = new LoginWindowViewModel();
            ViewService.Instance.ShowWindow(loginViewModel);
        }
    }
}
