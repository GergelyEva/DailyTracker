using TM.DailyTrackR.DataType.Enums;

namespace TM.DailyTrackR.DataType
{
    public class TableInfo
    {
        public int No { get; set; }
        public ProjectType ProjectType { get; set; }
        public TaskType TaskType { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public string User { get; set; }
        public int DatabaseId { get; set; }
    }
}
