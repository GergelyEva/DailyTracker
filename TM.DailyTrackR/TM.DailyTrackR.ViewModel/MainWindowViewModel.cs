namespace TM.DailyTrackR.ViewModel
{
    using Prism.Mvvm;

    public sealed class MainWindowViewModel : BindableBase
    {
        private string username;


        public MainWindowViewModel(string username)
        {
            this.username = username;
        }

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

    }
}
