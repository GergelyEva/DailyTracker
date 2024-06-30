namespace TM.DailyTrackR.Common
{
    using TM.DailyTrackR.Logic;

    public sealed class LogicHelper
    {
        private static readonly Lazy<LogicHelper> Lazy = new Lazy<LogicHelper>(() => new LogicHelper(), isThreadSafe: true);
        private LogicHelper()
        {
        }

        public static LogicHelper Instance { get { return Lazy.Value; } }

        public LoginController LoginController { get; }
        public ExportController ExportController { get; }
        public ActivityController ActivityController { get; }   
    }
}
